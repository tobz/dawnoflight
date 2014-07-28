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
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.GameObjects.CustomNPC;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.Trainers.hibernia
{
	/// <summary>
	/// Enchanter Trainer
	/// </summary>
	[NPCGuildScript("Enchanter Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Enchanter Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class EnchanterTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Enchanter; }
		}

		public const string WEAPON_ID1 = "enchanter_item";

		public EnchanterTrainer() : base()
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
			
			// check if class matches.
			if (player.CharacterClass.ID == (int) TrainedClass)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "EnchanterTrainer.Interact.Text2", this.Name, player.GetName(0, false)), ChatType.CT_Say, ChatLocation.CL_ChatWindow);
			}
			else
			{
				// perhaps player can be promoted
				if (CanPromotePlayer(player))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "EnchanterTrainer.Interact.Text1", this.Name), ChatType.CT_System, ChatLocation.CL_PopupWindow);
					if (!player.IsLevelRespecUsed)
					{
						OfferRespecialize(player);
					}
				}
				else
				{
					CheckChampionTraining(player);
				}
			}
			return true;
		}

		/// <summary>
		/// checks whether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Magician && (player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen));
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
			String lowerCase = text.ToLower();

			if (lowerCase == LanguageMgr.GetTranslation(player.Client.Account.Language, "EnchanterTrainer.WhisperReceiveCase.Text1"))
			{
				// promote player to other class
				if (CanPromotePlayer(player))
				{
					PromotePlayer(player, (int)eCharacterClass.Enchanter, LanguageMgr.GetTranslation(player.Client.Account.Language, "EnchanterTrainer.WhisperReceive.Text1", player.GetName(0, false)), null);
					player.ReceiveItem(this, WEAPON_ID1);
				}
			}
			return true;
		}
	}
}
