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
using DawnOfLight.GameServer.AI.Brain;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.GameObjects.Atlantis
{
    /// <summary>
    /// Base class for all Atlantis scholar type NPCs.
    /// </summary>
    /// <author>Aredhel</author>
    public class Researcher : GameNPC
    {
        public Researcher()
            : base() { }

		/// <summary>
		/// Can trade untradable items
		/// </summary>
		public override bool CanTradeAnyItem
		{
			get
			{
				return true;
			}
		}

        public override bool AddToWorld()
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
			switch (Realm)
			{
				case eRealm.Albion: 
					template.AddNPCEquipment(InventorySlot.TorsoArmor, 2230); break;
				case eRealm.Midgard:
					template.AddNPCEquipment(InventorySlot.TorsoArmor, 2232);
					template.AddNPCEquipment(InventorySlot.ArmsArmor, 2233);
					template.AddNPCEquipment(InventorySlot.LegsArmor, 2234);
					template.AddNPCEquipment(InventorySlot.HandsArmor, 2235);
					template.AddNPCEquipment(InventorySlot.FeetArmor, 2236);
					break;
				case eRealm.Hibernia:
					template.AddNPCEquipment(InventorySlot.TorsoArmor, 2231); ; break;
			}

            Inventory = template.CloseTemplate();
            Flags = eFlags.PEACE;	// Peace flag.
            return base.AddToWorld();
        }

        /// <summary>
		/// How friendly this NPC is to a player.
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>Aggro state as a string.</returns>
        public override string GetAggroLevelString(GamePlayer player, bool firstLetterUppercase)
        {
            IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
            String aggroLevelString;

            if (GameServer.ServerRules.IsSameRealm(this, player, true))
            {
                if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Friendly2");
                else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Friendly1");
            }
            else if (aggroBrain != null && aggroBrain.AggroLevel > 0)
            {
                if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Aggressive2");
                else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Aggressive1");
            }
            else
            {
                if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Neutral2");
                else aggroLevelString = LanguageMgr.GetTranslation(player.Client.Account.Language, 
                    "GameNPC.GetAggroLevelString.Neutral1");
            }

            return aggroLevelString;
        }

        /// <summary>
        /// Returns a list of examine messages.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = new ArrayList(4);
			list.Add(String.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "Researcher.GetExamineMessages.YouExamine",
									Name, (Name.EndsWith("a") || Name.EndsWith("le")) ?
									LanguageMgr.GetTranslation(player.Client.Account.Language, "Researcher.GetExamineMessages.She") :
									LanguageMgr.GetTranslation(player.Client.Account.Language, "Researcher.GetExamineMessages.He"),
									GetAggroLevelString(player, false))));
            return list;
        }

        /// <summary>
        /// Turn the researcher to face the player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            TurnTo(player, 10000);
            return true;
        }
    }
}
