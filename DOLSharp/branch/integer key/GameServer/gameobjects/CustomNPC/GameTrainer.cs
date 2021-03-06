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
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;

namespace DOL.GS
{
	/// <summary>
	/// The mother class for all class trainers
	/// </summary>
	public class GameTrainer : GameNPC
	{
		public virtual eCharacterClass TrainedClass 
		{
			get { return eCharacterClass.Unknown; }
		}
		/// <summary>
		/// Constructs a new GameTrainer
		/// </summary>
		public GameTrainer()
		{
		}

		#region GetExamineMessages
		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			string TrainerClassName = "";
			switch (ServerProperties.Properties.SERV_LANGUAGE)
			{
				case "EN":
					{
						int index = -1;
						if (GuildName.Length > 0)
							index = GuildName.IndexOf(" Trainer");
						if (index >= 0)
							TrainerClassName = GuildName.Substring(0, index);
					}
					break;
				case "DE":
					TrainerClassName = GuildName;
					break;
				default:
					{
						int index = -1;
						if (GuildName.Length > 0)
							index = GuildName.IndexOf(" Trainer");
						if (index >= 0)
							TrainerClassName = GuildName.Substring(0, index);
					}
					break;
			}

			IList list = new ArrayList();
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.YouTarget", GetName(0, false)));
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false), TrainerClassName));
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameTrainer.GetExamineMessages.RightClick"));
			return list;
		}
		#endregion

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{			
			if (!base.Interact(player)) return false;

			player.GainExperience(0);//levelup

			if (player.FreeLevelState == 2)
			{
				player.PlayerCharacter.LastFreeLevel = player.Level;
				//long xp = GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 3) - GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 2);
				long xp = GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel + 1) - GameServer.ServerRules.GetExperienceForLevel(player.PlayerCharacter.LastFreeLevel);
				//player.PlayerCharacter.LastFreeLevel = player.Level;
				player.GainExperience(xp);
				player.PlayerCharacter.LastFreeLeveled = DateTime.Now;
				player.Out.SendPlayerFreeLevelUpdate();
			}
			
			// Turn to face player
			TurnTo(player, 10000);

			return true;
		}

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;
			GamePlayer player = source as GamePlayer;
			if (player == null) return false;

			//Now we turn the npc into the direction of the person
			TurnTo(player, 10000);

			return true;
		}

		/// <summary>
		/// For Recieving Respec Stones. 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			GamePlayer player = source as GamePlayer;
			if (player != null)
			{
				switch (item.Id_nb)
				{
					case "respec_single":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountSingleSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecSingle"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_full":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountAllSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecFull", item.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
					case "respec_realm":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountRealmSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.ReceiveItem.RespecRealm"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
				}
			}


			return base.ReceiveItem(source, item);
		}

		public virtual bool CanPromotePlayer(GamePlayer player)
		{
			return true;
		}

		public void PromotePlayer(GamePlayer player)
		{
			if (TrainedClass != eCharacterClass.Unknown)
				PromotePlayer(player, (int)TrainedClass, "", null);
		}

		/// <summary>
		/// Called to promote a player
		/// </summary>
		/// <param name="player">the player to promote</param>
		/// <param name="classid">the new classid</param>
		/// <param name="messageToPlayer">the message for the player</param>
		/// <param name="gifts">Array of inventory items as promotion gifts</param>
		/// <returns>true if successfull</returns>
		public bool PromotePlayer(GamePlayer player, int classid, string messageToPlayer, InventoryItem[] gifts)
		{

			if (player == null) return false;

			IClassSpec oldClass = player.CharacterClass;

			// Player was promoted
			if (player.SetCharacterClass(classid))
			{
				player.RemoveAllStyles();

				if (messageToPlayer != "")
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Says", this.Name, messageToPlayer), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Upgraded", player.CharacterClass.Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				player.CharacterClass.OnLevelUp(player);
				player.UpdateSpellLineLevels(true);
				player.RefreshSpecDependantSkills(true);
				player.StartPowerRegeneration();
				//player.Out.SendUpdatePlayerSpells();
				player.Out.SendUpdatePlayerSkills();
				player.Out.SendUpdatePlayer();

				// Initiate equipment
				if (gifts != null && gifts.Length > 0)
				{
					for (int i = 0; i < gifts.Length; i++)
					{
						player.ReceiveItem(this, gifts[i]);
					}
				}

				// after gifts
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.PromotePlayer.Accepted", player.CharacterClass.Profession), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				Notify(GameTrainerEvent.PlayerPromoted, this, new PlayerPromotedEventArgs(player, oldClass));

				player.SaveIntoDatabase();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Add a gift to the player
		/// </summary>
		/// <param name="template">the template ID of the item</param>
		/// <param name="player">the player to give it to</param>
		/// <returns>true if succesful</returns>
		public virtual bool addGift(String template, GamePlayer player)
		{
			ItemTemplate temp = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), template);
			if (!player.Inventory.AddTemplate(temp, 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameTrainer.AddGift.NotEnoughSpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
	}
}
