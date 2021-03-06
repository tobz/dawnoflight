/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// AbstractCraftingSkill is the base class for all crafting skill
	/// </summary>
	public abstract class AbstractCraftingSkill
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaration
		/// <summary>
		/// How close a player can be to make a item with a forge , lathe ect ...
		/// </summary>
		public const int CRAFT_DISTANCE = 256;

		/// <summary>
		/// The icone number used by this craft.
		/// </summary>
		private byte m_icon;

		/// <summary>
		/// The name show for this craft.
		/// </summary>
		private string m_name;

		/// <summary>
		/// The crafting skill id of this craft
		/// </summary>
		private eCraftingSkill m_eskill;
		
		/// <summary>
		/// The player currently crafting
		/// </summary>
		protected const string PLAYER_CRAFTER = "PLAYER_CRAFTER";

		/// <summary>
		/// The item in construction
		/// </summary
		protected const string ITEM_CRAFTER = "ITEM_CRAFTER";

		/// <summary>
		/// The enum index of this crafting skill
		/// </summary>
		public eCraftingSkill eSkill 
		{
			get
			{
				return m_eskill;
			}
			set
			{
				m_eskill = value;
			}
		}

		/// <summary>
		/// The icon of this crafting skill
		/// </summary>
		public byte Icon 
		{
			get
			{
				return m_icon;
			}
			set
			{
				m_icon = value;
			}
		}

		/// <summary>
		/// The name of this crafting skill
		/// </summary>
		public string Name 
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}
		#endregion

		#region First call function and callback

		/// <summary>
		/// Called when player craft an item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public int CraftItem(CraftItemData item, GamePlayer player)
		{
			if( !GameServer.ServerRules.IsAllowedToCraft(player, item.TemplateToCraft))
			{
				return 0;
			}

			if(	!CheckTool(player, item))
			{
				return 0;
			}

			if( !CheckSecondCraftingSkillRequirement(player, item))
			{
				return 0;
			}

			if(	!CheckRawMaterial(player, item))
			{
				return 0;
			}
			
			if (player.IsCrafting)
			{
				player.CraftTimer.Stop();
				player.Out.SendMessage("You stop your current work.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendCloseTimerWindow();
				return 0;
			}

			if (player.IsMoving || player.Strafing)
			{
				player.Out.SendMessage("You move and interrupt your crafting.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return 0;
			}

			int craftingTime = GetCraftingTime(player,item);
			player.Out.SendMessage("You begin work on the "+item.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			player.Out.SendTimerWindow("Currently Making: "+item.TemplateToCraft.Name, craftingTime);
			player.CraftTimer = new RegionTimer(player);
			player.CraftTimer.Callback = new RegionTimerCallback(MakeItem);
			player.CraftTimer.Properties.setProperty(PLAYER_CRAFTER,player);
			player.CraftTimer.Properties.setProperty(ITEM_CRAFTER,item);
			player.CraftTimer.Start(craftingTime * 1000);
			return 1;
		}

		/// <summary>
		/// Make the item when craft time is finished 
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		protected virtual int MakeItem(RegionTimer timer)
		{
			GamePlayer player = (GamePlayer)timer.Properties.getObjectProperty( PLAYER_CRAFTER,null );
			CraftItemData item = (CraftItemData)timer.Properties.getObjectProperty( ITEM_CRAFTER,null );
			if (player == null || item == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("There was a problem getting back the item to the player in craft system.");
				return 0;
			}

			player.CraftTimer.Stop();
			player.Out.SendCloseTimerWindow();
			
			if (Util.Chance(CalculateChanceToMakeItem(player, item)))
			{
				if(! RemoveUsedMaterials(player,item))
				{
					player.Out.SendMessage("You have not all needed raw materials to create this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return 0;
				}
				
				BuildCraftedItem(player,item);
			
				GainCraftingSkillPoints(player,item);
			}
			else if (Util.Chance(CalculateChanceToLooseMaterial(player,item)))
			{	
				if(!LooseRawMaterial(player,item))
				{
					player.Out.SendMessage("You have not all needed raw materials to create this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}

				player.Out.SendPlaySound(eSoundType.Craft, 0x01);
			}
			else
			{
				player.Out.SendMessage("You fail to make the "+item.TemplateToCraft.Name+" but lose no materials!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendPlaySound(eSoundType.Craft, 0x02);
			}

			return 0;
		}
		
		#endregion

		#region Requirement check

		/// <summary>
		/// Check if the player own all needed tools
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		public abstract bool CheckTool(GamePlayer player, CraftItemData craftItemData);

		/// <summary>
		/// Check if the player have enough secondary crafting skill to build the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		public virtual bool CheckSecondCraftingSkillRequirement(GamePlayer player, CraftItemData craftItemData)
		{
			int minimumLevel = CalculateSecondCraftingSkillMinimumLevel(craftItemData);

			if(minimumLevel <= 0) return true; // no requirement needed
	
			foreach (RawMaterial material in craftItemData.RawMaterials)
			{
				switch(material.MaterialTemplate.Model)
				{
					case 522 :	//"cloth square"
					case 537 :	//"heavy thread"
					{
						if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < minimumLevel)
						{
							player.Out.SendMessage("You don't have the minimum necessary Clothworking skill ("+minimumLevel+") to create the "+craftItemData.TemplateToCraft.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}

					case 521 :	//"leather square"
					{
						if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < minimumLevel)
						{
							player.Out.SendMessage("You don't have the minimum necessary Leathercrafting skill ("+minimumLevel+") to create the "+craftItemData.TemplateToCraft.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}

					case 519 :	//"metal bars"
					{
						if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < minimumLevel)
						{
							player.Out.SendMessage("You don't have the minimum necessary Metalworking skill ("+minimumLevel+") to create the "+craftItemData.TemplateToCraft.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}

					case 520 :	//"wooden boards"
					{
						if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < minimumLevel)
						{
							player.Out.SendMessage("You don't have the minimum necessary Woodworking skill ("+minimumLevel+") to create the "+craftItemData.TemplateToCraft.Name+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// check if player have raw mateiral to make item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		public virtual bool CheckRawMaterial(GamePlayer player, CraftItemData craftItemData)
		{
			ArrayList missingMaterials = null;

			lock(player.Inventory)
			{
				ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				
				foreach (RawMaterial material in craftItemData.RawMaterials)
				{
					bool result = false;
					int count = material.CountNeeded;		
					foreach (GenericItem item in allBackpackItems)
					{
						if (item.Name == material.MaterialTemplate.Name)
						{
							StackableItem stack = item as StackableItem;
							if(stack != null)
							{
								if(stack.Count >= count)
								{
									result = true;
									break;
								}
								else
								{
									count-= stack.Count;
								}
							}
							else
							{
								if(count <= 1)
								{
									result = true;
									break;
								}
								else
								{
									count--;
								}
							}
						}
					}
					if(result == false)
					{
						if(missingMaterials == null) missingMaterials = new ArrayList(5);
						missingMaterials.Add(material);
					}
				}

				if(missingMaterials != null)
				{
					player.Out.SendMessage("You do not have the ingredients to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					player.Out.SendMessage("You are missing :",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					foreach (RawMaterial material in missingMaterials)
					{
						player.Out.SendMessage("("+material.CountNeeded+") "+material.MaterialTemplate.Name,eChatType.CT_System,eChatLoc.CL_SystemWindow);
					}
					return false;
				}
				
				return true;
			}	
		}
		#endregion

		#region Gain points

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public virtual void GainCraftingSkillPoints(GamePlayer player, CraftItemData item)
		{
			int gainPointChance = CalculateChanceToGainPoint(player, item);

			foreach (RawMaterial material in item.RawMaterials)
			{
				if(Util.Chance(gainPointChance))
				{
					switch(material.MaterialTemplate.Model)
					{
						case 522 :	//"cloth square"
						case 537 :	//"heavy thread"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill cap == primary skill
							{
								player.IncreaseCraftingSkill(eCraftingSkill.ClothWorking, 1);
							}
							break;
						}

						case 521 :	//"leather square"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.LeatherCrafting) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill cap == primary skill
							{
								player.IncreaseCraftingSkill(eCraftingSkill.LeatherCrafting, 1);
							}
							break;
						}

						case 519 :	//"metal bars"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill == primary skill
							{
								player.IncreaseCraftingSkill(eCraftingSkill.MetalWorking, 1);
							}
							break;
						}

						case 520 :	//"wooden boards"
						{
							if (player.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill == primary skill
							{
								player.IncreaseCraftingSkill(eCraftingSkill.WoodWorking, 1);
							}
							break;
						}
					}
				}
			}
			player.Out.SendUpdateCraftingSkills();
		}
		#endregion

		#region Use materials and created crafted item

		/// <summary>
		/// Remove used raw material from player inventory
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		public bool RemoveUsedMaterials(GamePlayer player, CraftItemData craftItemData)
		{
			Hashtable dataSlots = new Hashtable(10);
			lock(player.Inventory)
			{
				ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				
				foreach (RawMaterial material in craftItemData.RawMaterials)
				{
					bool result = false;
					int count = material.CountNeeded;
					foreach (GenericItem item in allBackpackItems)
					{
						if (item.Name == material.MaterialTemplate.Name)
						{
							StackableItem stack = item as StackableItem;
							if(stack != null) // is the item is stackable
							{
								if(stack.Count >= count)
								{
									if(stack.Count == count)
									{
										dataSlots.Add(stack, null);
									}
									else
									{
										dataSlots.Add(stack, count);
									}
									result = true;
									break;
								}
								else
								{
									dataSlots.Add(stack, null);
									count-= stack.Count;
								}
							}
							else
							{
								dataSlots.Add(item, null);
								if(count <= 1)
								{
									result = true;
									break;
								}
								else
								{
									count--;
								}
							}
						}
					}
					if(result == false)
					{
						return false;
					}
				}

				GamePlayerInventory playerInventory = player.Inventory as GamePlayerInventory;
				playerInventory.BeginChanges();
				foreach(DictionaryEntry de in dataSlots)
				{
					if(de.Value == null)
					{
						playerInventory.RemoveItem((GenericItem)de.Key);
					}
					else
					{
						playerInventory.RemoveCountFromStack((StackableItem)de.Key, (int)de.Value);
					}
				}
				playerInventory.CommitChanges();
			}
			return true;//all raw material removed and item created
		}

		/// <summary>
		/// Make the crafted item and add it to player's inventory
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		protected virtual void BuildCraftedItem(GamePlayer player ,CraftItemData craftItemData)
		{
			GenericItem newItem = craftItemData.TemplateToCraft.CreateInstance();

			if (newItem is EquipableItem) ((EquipableItem)newItem).Quality = GetQuality(player,craftItemData);
			
			player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, newItem);

			int quality = 94;
			if(newItem is EquipableItem) quality = ((EquipableItem)newItem).Quality;
			player.Out.SendMessage("You successfully make the "+craftItemData.TemplateToCraft.Name+"! (" +quality+ ")",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
			
			if(quality >= 100)
			{
				player.Out.SendMessage("Congratulation, you make a masterpiece !",eChatType.CT_Skill,eChatLoc.CL_SystemWindow);	
				player.Out.SendPlaySound(eSoundType.Craft, 0x04);
			}
			else
			{
				player.Out.SendPlaySound(eSoundType.Craft, 0x03);
			}
		}

		/// <summary>
		/// loose raw material when failed in craft
		/// </summary>
		/// <param name="player"></param>
		/// <param name="craftItemData"></param>
		/// <returns></returns>
		public bool LooseRawMaterial(GamePlayer player,CraftItemData craftItemData)
		{
			player.Out.SendMessage("You fail to make the " +craftItemData.TemplateToCraft.Name+ ", and lose some materials!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			
			Hashtable dataSlots = new Hashtable(5);

			lock(player.Inventory)
			{
				ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
				
				foreach (RawMaterial material in craftItemData.RawMaterials)
				{
					int count = 0;
					if (material != null && Util.Chance(60)) // 60% chance to loseeach material
					{
						count = material.CountNeeded * Util.Random(0, 60) / 100; // calculate how much material are lost
						if (count <= 0) count = 1;
					}

					if(count <= 0) continue; // don't remove this material

					player.Out.SendMessage("You lose ("+count+") "+material.MaterialTemplate.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);

					bool result = false;
					foreach (GenericItem item in allBackpackItems)
					{
						if (item!=null && item.Name == material.MaterialTemplate.Name)
						{
							StackableItem stack = item as StackableItem;
							if(stack != null) // is the item is stackable
							{
								if(stack.Count >= count)
								{
									if(stack.Count == count)
									{
										dataSlots.Add(stack, null);
									}
									else
									{
										dataSlots.Add(stack, count);
									}
									result = true;
									break;
								}
								else
								{
									dataSlots.Add(stack, null);
									count-= stack.Count;
								}
							}
							else
							{
								dataSlots.Add(item, null);
								if(count <= 1)
								{
									result = true;
									break;
								}
								else
								{
									count--;
								}
							}
						}
					}
					if(result == false)
					{
						return false;
					}
				}

				GamePlayerInventory playerInventory = player.Inventory as GamePlayerInventory;
				playerInventory.BeginChanges();
				foreach(DictionaryEntry de in dataSlots)
				{
					if(de.Value == null)
					{
						playerInventory.RemoveItem((GenericItem)de.Key);
					}
					else
					{
						playerInventory.RemoveCountFromStack((StackableItem)de.Key, (int)de.Value);
					}
				}
				playerInventory.CommitChanges();
			}
			return true;
		}
		#endregion

		#region Calcul functions

		/// <summary>
		/// Calculate chance to succes
		/// </summary>
		public virtual int CalculateChanceToMakeItem(GamePlayer player, CraftItemData item)
		{
			int[] basesTable = new int[12] { 70, 68, 66, 64, 62, 60, 58, 56, 54, 52, 50, 50 };
			
			int playerCraftLevel = player.GetCraftingSkillValue(m_eskill) / 100;
			if (playerCraftLevel > 11) playerCraftLevel = 11;

			int baseChance = basesTable[playerCraftLevel];

			//modificator based on difficulty of item and current player's level
			int chanceModifier = player.GetCraftingSkillValue(m_eskill) - item.CraftingLevel;
			if(chanceModifier > 0) chanceModifier /= 2;
			else chanceModifier *= 2;

			int finalChances = baseChance + chanceModifier;

			if(finalChances < 2) finalChances = 2;
			else if(finalChances > 100) finalChances = 100;
				
			return finalChances;  // red 20, orange 45 ,yellow 70 ,bleu 80, green 90 
		}

		/// <summary>
		/// Calculate chance to gain point
		/// </summary>
		public virtual int CalculateChanceToGainPoint(GamePlayer player,CraftItemData item)
		{
			int[] basesTable = new int[12] { 80, 80, 75, 70, 65, 60, 55, 50, 45, 40, 35, 35 };
			
			int playerCraftLevel = player.GetCraftingSkillValue(m_eskill) / 100;
			if (playerCraftLevel > 11) playerCraftLevel = 11;

			int baseChance = basesTable[playerCraftLevel];

			int chanceModifier = item.CraftingLevel - player.GetCraftingSkillValue(m_eskill);
			if (chanceModifier > 0) chanceModifier /=2;
			else chanceModifier *=2;
			
			int finalChances = baseChance + chanceModifier;
			if(finalChances < 0) finalChances = 0;
			else if(finalChances > 98) finalChances = 98;

			return finalChances;

			//skill 0-100 : red 98, orange 92 ,yellow 85 ,bleu 60, green 30
			//skill 900-1000 : red 55, orange 47 ,yellow 40 ,bleu 15, green 0
		}

		/// <summary>
		/// Calculate crafting time
		/// </summary>
		public virtual int GetCraftingTime(GamePlayer player,CraftItemData ItemCraft)
		{
			double baseMultiplier = (ItemCraft.CraftingLevel / 100) + 1;

			ushort materialsCount = 0;
			foreach (RawMaterial material in ItemCraft.RawMaterials)
			{
				materialsCount += material.CountNeeded;
			}

			//if the item is gray con, crafting process will be almost two times faster
			if (((ushort)player.GetCraftingSkillValue(m_eskill) - ItemCraft.CraftingLevel) > 45) baseMultiplier *= 0.55;

			//at least 1s
			int craftingTime = (int)(baseMultiplier * materialsCount / 4);
			if(craftingTime < 1) craftingTime = 1;

			return craftingTime;
		}

		/// <summary>
		/// Calculate chance to lose material
		/// </summary>
		public virtual int CalculateChanceToLooseMaterial(GamePlayer player, CraftItemData item)
		{
			if (player.GetCraftingSkillValue(m_eskill) >= item.CraftingLevel) return 0;

			return item.CraftingLevel - player.GetCraftingSkillValue(m_eskill);
		}

		/// <summary>
		/// Calculate the minumum needed secondary crafting skill level to make the item
		/// </summary>
		public virtual int CalculateSecondCraftingSkillMinimumLevel(CraftItemData item)
		{
			return 0;
		}

		/// <summary>
		/// Calculate crafted item quality
		/// </summary>
		public byte GetQuality(GamePlayer player,CraftItemData item)
		{
			// 2% chance to get masterpiece, 1:6 chance to get 94-99%, if legendary or if grey con
			// otherwise moving the most load towards 94%, the higher the item con to the crafter skill

			// legendary
			if (player.GetCraftingSkillValue(m_eskill) >= 1000) {
				if (Util.Chance(2)) {
					return 100;	// 2% chance for master piece
				}
				return (byte) (94 + Util.Random(5));
			}

			int delta = GetItemCon(player.GetCraftingSkillValue(m_eskill), item.CraftingLevel);
			if (delta < -2) {
				if (Util.Chance(2)) return 100; // grey items get 2% chance to be master piece
				return (byte) (94 + Util.Random(5)); // handle grey items like legendary
			}

			// this is a type of roulette selection, imagine a roulette wheel where all chances get different sized
			// fields where the ball can drop, the bigger the field, the bigger the chance
			// field size is modulated by item con and can be also 0

			// possible chance allocation scheme for yellow item
			// 99:
			// 98:
			// 97: o
			// 96: oo
			// 95: ooo
			// 94: oooo
			// where one 'o' marks 100 size, this example results in 10% chance for yellow items to be 97% quality
			
			delta = delta * 100;

			int[] chancePart = new int[6]; // index ranges from 94%(0) to 99%(5)
			int sum = 0;
			for (int i=0; i<6; i++) {
				chancePart[i] = Math.Max((4-i)*100 - delta, 0);	// 0 minimum
				sum += chancePart[i];
			}
			
			// selection
			int rand = Util.Random(sum);
			for (int i=5; i>=0; i--) {
				if (rand < chancePart[i]) return (byte) (94 + i);
				rand -= chancePart[i];
			}

			// if something still not clear contact Thrydon/Blue

			return 94;
		}


		/// <summary>
		/// get item con color compared to crafters skill, TODO no floating point calculation yet
		/// </summary>
		/// <param name="crafterSkill"></param>
		/// <param name="itemCraftingLevel"></param>
		/// <returns></returns>
		public int GetItemCon(int crafterSkill, int itemCraftingLevel) {
			int diff = itemCraftingLevel - crafterSkill;
			if (diff <= -50) return -3; // grey
			else if (diff <= -31) return -2; // green
			else if (diff <= -11) return -1; // blue
			else if (diff <= 0) return 0; // yellow
			else if (diff <= 19) return 1; // orange
			else if (diff <= 49) return 2; // red
			else return 3; // impossible
		}
		#endregion
	}
}
