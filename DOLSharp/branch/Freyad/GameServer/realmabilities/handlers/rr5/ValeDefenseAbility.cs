using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using DOL.Database;
using DOL.GS.Spells;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Vale Defense Realm Ability
	/// </summary>
	public class ValeDefenseAbility : RR5RealmAbility
	{
		public ValeDefenseAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			GameLiving player = living;

			if (player == null)
				return;

			Spell subspell = SkillBase.GetSpellByID(7063);
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(player, subspell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
			if(player.Group==null)
			{
				spellhandler.CastSpell(player);
			}
			else foreach (GamePlayer member in player.Group.GetPlayersInTheGroup())
			{
				if(member!=null) spellhandler.CastSpell(member);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Vale Defense.");
			list.Add("Ablative buffer 50% with 1000 hits points buffer pool.");
			list.Add("");
			list.Add("Target: Group");
			list.Add("Duration: 10 min");
			list.Add("Casting time: instant");
		}

	}
}
