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
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS
{
	public class Fletching : AbstractProfession
	{
        protected override String Profession
        {
            get
            {
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,
                    "CraftersProfession.Fletcher");
            }
        }

		public Fletching()
		{
			Icon = 0x0C;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Fletching");
			eSkill = eCraftingSkill.Fletching;
		}

		protected override bool CheckForTools(GamePlayer player, DBCraftedItem craftItemData)
		{
			if (craftItemData.ItemTemplate.Object_Type != (int)eObjectType.Arrow && 
                craftItemData.ItemTemplate.Object_Type != (int)eObjectType.Bolt)
			{
				foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
				{
					if (item.Model == 481) // Lathe
						return true;
				}

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, 
                    "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);

				player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, 
                    "Crafting.CheckTool.FindLathe"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return false;
			}
			return true;
		}

		public override int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem item)
		{
			switch (item.ItemTemplate.Object_Type)
			{
				case (int)eObjectType.Fired:  //tested
				case (int)eObjectType.Longbow: //tested
				case (int)eObjectType.Crossbow: //tested
				case (int)eObjectType.Instrument: //tested
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.CompositeBow:
					return item.CraftingLevel - 20;

				case (int)eObjectType.Arrow: //tested
				case (int)eObjectType.Bolt: //tested
				case (int)eObjectType.Thrown:
					return item.CraftingLevel - 15;

				case (int)eObjectType.Staff: //tested
					return item.CraftingLevel - 35;
			}

			return base.GetSecondaryCraftingSkillMinimumLevel(item);
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, item)))
			{
				player.GainCraftingSkill(eCraftingSkill.Fletching, 1);
				base.GainCraftingSkillPoints(player, item);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
