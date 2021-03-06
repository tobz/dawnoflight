using System.Collections.Generic;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;

namespace DawnOfLight.GameServer.RealmAbilities.handlers
{
	/// <summary>
	/// Ignore Pain, healing
	/// </summary>
	public class IgnorePainAbility : TimedRealmAbility
	{
		public IgnorePainAbility(DBAbility dba, int level) : base(dba, level) { }

		/// <summary>
		/// Action
		/// </summary>
		/// <param name="living"></param>
		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			int heal = 0;

			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: heal = 20; break;
					case 2: heal = 35; break;
					case 3: heal = 50; break;
					case 4: heal = 65; break;
					case 5: heal = 80; break;
				}				
			}
			else
			{
				switch (Level)
				{
					case 1: heal = 20; break;
					case 2: heal = 50; break;
					case 3: heal = 80; break;
				}				
			}

			int healed = living.ChangeHealth(living, GameLiving.eHealthChangeType.Spell, living.MaxHealth * heal / 100);

			SendCasterSpellEffectAndCastMessage(living, 7004, healed > 0);

			GamePlayer player = living as GamePlayer;
			if (player != null)
			{
				if (healed > 0) player.Out.SendMessage("You heal yourself for " + healed + " hit points.", ChatType.CT_Spell, ChatLocation.CL_SystemWindow);
				if (heal > healed)
				{
					player.Out.SendMessage("You are fully healed.", ChatType.CT_Spell, ChatLocation.CL_SystemWindow);
				}
			}
			if (healed > 0) DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 900;
		}

		public override void AddEffectsInfo(IList<string> list)
		{
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				list.Add("Level 1: Value: 20%");
				list.Add("Level 2: Value: 35%");
				list.Add("Level 3: Value: 50%");
				list.Add("Level 4: Value: 65%");
				list.Add("Level 5: Value: 80%");
				list.Add("");
				list.Add("Target: Self");
				list.Add("Casting time: instant");
			}
			else
			{
				list.Add("Level 1: Value: 20%");
				list.Add("Level 2: Value: 50%");
				list.Add("Level 3: Value: 80%");
				list.Add("");
				list.Add("Target: Self");
				list.Add("Casting time: instant");
			}
		}
	}
}