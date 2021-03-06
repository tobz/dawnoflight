using System;
using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.RealmAbilities.effects;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.RealmAbilities.handlers
{
	public class TheEmptyMindAbility : TimedRealmAbility
	{
		public TheEmptyMindAbility(DBAbility dba, int level) : base(dba, level) { }

		public const Int32 m_duration = 45000; //45 seconds

		public override void Execute(GameLiving living)
		{
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			foreach (GamePlayer t_player in living.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (t_player == living && living is GamePlayer)
				{
					(living as GamePlayer).Out.SendMessage("You clear your mind and become more resistant to magic damage!", ChatType.CT_Spell, ChatLocation.CL_SystemWindow);
				}
				else
				{
					t_player.Out.SendMessage(living.Name + " casts a spell!", ChatType.CT_Spell, ChatLocation.CL_SystemWindow);
				}
			}
			
			int effectiveness = 10;
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
				switch (Level)
				{
					case 1: effectiveness = 10; break;
					case 2: effectiveness = 15; break;
					case 3: effectiveness = 20; break;
					case 4: effectiveness = 25; break;
					case 5: effectiveness = 30; break;
					default: effectiveness = 0; break;
				}				
			}
			else
			{
				switch (Level)
				{
					case 1: effectiveness = 10; break;
					case 2: effectiveness = 20; break;
					case 3: effectiveness = 30; break;
					default: effectiveness = 0; break;
				}
			}
			
			
			new TheEmptyMindEffect(effectiveness).Start(living);
			DisableSkill(living);
		}

		public override int GetReUseDelay(int level)
		{
			return 600;
		}
	}
}
