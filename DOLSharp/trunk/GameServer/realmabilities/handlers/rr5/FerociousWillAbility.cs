using System.Collections.Generic;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.RealmAbilities.effects.rr5;

namespace DawnOfLight.GameServer.RealmAbilities.handlers.rr5
{
	/// <summary>
	/// Mastery of Concentration RA
	/// </summary>
	public class FerociousWillAbility : RR5RealmAbility
	{
		public FerociousWillAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;



			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				SendCasterSpellEffectAndCastMessage(player, 7065, true);
				FerociousWillEffect effect = new FerociousWillEffect();
				effect.Start(player);
			}
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			list.Add("Gives the zerker an ABS buff that ticks up by 5% every 5 seconds for a max of 25% at 25 seconds. Lasts 30 seconds total.");
			list.Add("");
			list.Add("Target: Self");
			list.Add("Duration: 30 sec");
			list.Add("Casting time: instant");
		}

	}
}
