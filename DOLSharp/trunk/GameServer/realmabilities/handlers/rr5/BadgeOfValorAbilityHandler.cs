using DawnOfLight.Database;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.RealmAbilities.effects.rr5;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.RealmAbilities.handlers.rr5
{
    public class BadgeOfValorAbilityHandler : RR5RealmAbility
    {
		public BadgeOfValorAbilityHandler(DBAbility dba, int level) : base(dba, level) { }

        int m_reuseTimer = 900;

        public override void Execute(GameLiving living)
        {
            #region preCheck
			if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

			if (living.EffectList.CountOfType<BadgeOfValorEffect>() > 0)
            {
				if (living is GamePlayer)
					(living as GamePlayer).Out.SendMessage("You already an effect of that type!", ChatType.CT_SpellResisted, ChatLocation.CL_SystemWindow);
                return;
            }

            #endregion


            //send spelleffect
			foreach (GamePlayer visPlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				visPlayer.Out.SendSpellEffectAnimation(living, living, 7057, 0, false, 0x01);

            new BadgeOfValorEffect().Start(living);
            living.DisableSkill(this, m_reuseTimer * 1000);
        }
    }
}
