using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.RealmAbilities.effects;
using DawnOfLight.GameServer.Utilities;

namespace DawnOfLight.GameServer.RealmAbilities.handlers
{
    public class DivineInterventionAbility : TimedRealmAbility
	{
        public DivineInterventionAbility(DBAbility dba, int level) : base(dba, level) { }
		public const int poolDuration = 1200000; // 20 minutes

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;
            GamePlayer player = living as GamePlayer;

			Group playerGroup = player.Group;

			if (playerGroup == null)
			{
				player.Out.SendMessage("You must be in a group to use this ability!", ChatType.CT_SpellResisted, ChatLocation.CL_SystemWindow);
				return;
			}

			int poolValue = 0;
			
			if(ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
			{
	            switch (Level)
	            {
	                case 1: poolValue = 1000; break;
	                case 2: poolValue = 1500; break;
	                case 3: poolValue = 2000; break;
	                case 4: poolValue = 2500; break;
	                case 5: poolValue = 3000; break;
	            }				
			}
			else
			{
	            switch (Level)
	            {
	                case 1: poolValue = 1000; break;
	                case 2: poolValue = 2000; break;
	                case 3: poolValue = 3000; break;
	            }				
			}


			foreach (GamePlayer groupMember in playerGroup.GetPlayersInTheGroup())
			{
				DivineInterventionEffect DIEffect = groupMember.EffectList.GetOfType<DivineInterventionEffect>();
				if (DIEffect != null)
				{
					player.Out.SendMessage("You are already protected by a pool of healing", ChatType.CT_SpellResisted, ChatLocation.CL_SystemWindow);
					return;
				}
			}
            DisableSkill(living);
            if (player != null)
            {
                new DivineInterventionEffect(poolValue).Start(player);
            }
		}
        public override int GetReUseDelay(int level)
        {
            return 600;
        }
	}
}
