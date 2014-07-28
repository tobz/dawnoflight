/* 01/03/2005
   Written by Gavinius */

using System.Collections;
using DawnOfLight.GameServer.commands.Player;
using DawnOfLight.GameServer.Crafting;
using DawnOfLight.GameServer.Language;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.GameObjects.CustomNPC
{
	[NPCGuildScript("Name Registrar")]
	public class NameRegistrar : GameNPC
	{
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(2);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "NameRegistrar.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
            return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if(base.Interact(player))
			{
				/* Get primary crafting skill (if any) */
				int CraftSkill = 0;
				if (player.CraftingPrimarySkill != eCraftingSkill.NoCrafting)
					CraftSkill = player.GetCraftingSkillValue(player.CraftingPrimarySkill);

				/* Check if level and/or crafting skill let you have a lastname */
				if (player.Level < LastnameCommandHandler.LASTNAME_MIN_LEVEL && CraftSkill < LastnameCommandHandler.LASTNAME_MIN_CRAFTSKILL)
					SayTo(player, ChatLocation.CL_SystemWindow, LanguageMgr.GetTranslation(player.Client.Account.Language, "NameRegistrar.ReturnToMe", LastnameCommandHandler.LASTNAME_MIN_LEVEL));
				else
                    SayTo(player, ChatLocation.CL_SystemWindow, LanguageMgr.GetTranslation(player.Client.Account.Language, "NameRegistrar.LastName"));
                return true;
			}
			return false;
		}

	}
}

