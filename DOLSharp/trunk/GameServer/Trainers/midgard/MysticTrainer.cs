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

using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Trainers.midgard
{
	/// <summary>
	/// Mystic Trainer
	/// </summary>
	[NPCGuildScript("Mystic Trainer", eRealm.Midgard)]		// this attribute instructs DOL to use this script for all "Mystic Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MysticTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Mystic; }
		}

		public const string PRACTICE_WEAPON_ID = "trimmed_branch";
		
		public MysticTrainer() : base(eChampionTrainerType.Mystic)
		{
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			
			// check if class matches
			if (player.CharacterClass.ID == (int)TrainedClass)
			{
				// player can be promoted
				if (player.Level>=5)
				{
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Runemaster] or [Spiritmaster]?\"", ChatType.CT_Say, ChatLocation.CL_PopupWindow);
				}
				else
				{
					OfferTraining(player);
				}

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, InventorySlot.MinEquipable, InventorySlot.LastBackpack) == null)
				{
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice branch]?\"",ChatType.CT_Say,ChatLocation.CL_PopupWindow);
				}
			}
			else
			{
				CheckChampionTraining(player);
			}
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

			switch (text) {
				case "Runemaster":
					if(player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Kobold || player.Race == (int) eRace.Norseman || player.Race == (int) eRace.Dwarf){
						player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",ChatType.CT_Say,ChatLocation.CL_PopupWindow);
					}
					else{
						player.Out.SendMessage(this.Name + " says, \"The path of a Runemaster is not available to your race. Please choose another.\"",ChatType.CT_Say,ChatLocation.CL_PopupWindow);
					}
					return true;
				case "Spiritmaster":
					if(player.Race == (int) eRace.Kobold || player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Norseman){
						player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",ChatType.CT_Say,ChatLocation.CL_PopupWindow);
					}
					else{
						player.Out.SendMessage(this.Name + " says, \"The path of a Spiritmaster is not available to your race. Please choose another.\"",ChatType.CT_Say,ChatLocation.CL_PopupWindow);
					}
					return true;
				case "practice branch":
					if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, InventorySlot.Min_Inv, InventorySlot.Max_Inv) == null)
					{
						player.ReceiveItem(this,PRACTICE_WEAPON_ID);
					}
					return true;
					
			}
			return true;
		}
	}
}
