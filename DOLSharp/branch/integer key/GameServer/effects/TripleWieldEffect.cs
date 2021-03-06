using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DOL.Events;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	/// <summary>
	/// TripleWield
	/// </summary>
	public class TripleWieldEffect : TimedEffect
	{
		public TripleWieldEffect(int duration) : base(duration)
		{
		}

		public override void Start(GameLiving target)
		{
			base.Start(target);
			GamePlayer player = target as GamePlayer;
            foreach(GamePlayer p in player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
			    p.Out.SendSpellEffectAnimation(player, player, Icon, 0, false, 1);
			    p.Out.SendSpellCastAnimation(player, Icon, 0);
            }
			GameEventMgr.AddHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));			
		}

		public override void Stop()
		{
			base.Stop();
			GamePlayer player = Owner as GamePlayer;
			GameEventMgr.RemoveHandler(player, GameLivingEvent.AttackFinished, new DOLEventHandler(EventHandler));
		}

		/// <summary>
		/// Handler fired on every melee attack by effect target
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected void EventHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs atkArgs = arguments as AttackFinishedEventArgs;
			if (atkArgs == null) return;
			if (atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitUnstyled
				&& atkArgs.AttackData.AttackResult != GameLiving.eAttackResult.HitStyle) return;
			if (atkArgs.AttackData.Target == null) return;
			GameLiving target = atkArgs.AttackData.Target;
			if (target == null) return;
			if (target.ObjectState != GameObject.eObjectState.Active) return;
			if (target.IsAlive == false) return;
			GameLiving attacker = sender as GameLiving;
			if (attacker == null) return;
			if (attacker.ObjectState != GameObject.eObjectState.Active) return;
			if (attacker.IsAlive == false) return;
			if (atkArgs.AttackData.IsOffHand) return; // only react to main hand
			if (atkArgs.AttackData.Weapon == null) return; // no weapon attack

			int modifier = 100;
			//double dpsCap = (1.2 + 0.3 * attacker.Level) * 0.7;
			//double dps = Math.Min(atkArgs.AttackData.Weapon.DPS_AF/10.0, dpsCap);
			double baseDamage = atkArgs.AttackData.Weapon.DPS_AF/10.0*
			                    atkArgs.AttackData.WeaponSpeed;

			modifier += (int)(25 * atkArgs.AttackData.Target.GetConLevel(atkArgs.AttackData.Attacker));
			modifier = Math.Min(300, modifier);
			modifier = Math.Max(75, modifier);
			
			double damage = baseDamage * modifier * 0.001; // attack speed is 10 times higher (2.5spd=25)			
			double damageResisted = damage * target.GetResist(eDamageType.Body) * -0.01;
			
			AttackData ad = new AttackData();
			ad.Attacker = attacker;
			ad.Target = target;
			ad.Damage = (int)(damage + damageResisted);
			ad.Modifier = (int)damageResisted;
			ad.DamageType = eDamageType.Body;
			ad.AttackType = AttackData.eAttackType.Spell;
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
            ad.WeaponSpeed = atkArgs.AttackData.WeaponSpeed; 

			GamePlayer owner = attacker as GamePlayer;
			if (owner != null) {
				owner.Out.SendMessage(String.Format("Your magical blade hits {0} for {1} extra damage!", target.GetName(0, false), ad.Damage), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				GamePlayer playerTarget = target as GamePlayer;
				if (playerTarget != null) {
					owner.Out.SendMessage(String.Format("{0}'s magical blade does {1} extra damage to you!", attacker.GetName(0, false), ad.Damage), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);					
				}
			}
			
			target.OnAttackedByEnemy(ad);
			attacker.DealDamage(ad);

			foreach (GamePlayer player in ad.Attacker.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendCombatAnimation(null, target, 0, 0, 0, 0, 0x0A, target.HealthPercent);
			}
			//			log.Debug(String.Format("spell damage: {0}; damage: {1}; resisted damage: {2}; damage type {3}; minSpread {4}.", Spell.Damage, ad.Damage, ad.Modifier, ad.DamageType, m_minDamageSpread));
			//			log.Debug(String.Format("dpsCap: {0}; dps: {1}; dmg {2}; spread: {6}; resDmg: {3}; atkSpeed: {4}; resist: {5}.", dpsCap, dps, damage, damageResisted, attacker.AttackSpeed(null), ad.Target.GetResist(Spell.DamageType), spread));
		}		

		public override string Name { get { return "Triple Wield"; } }

		public override ushort Icon { get { return 7102; } }

		public override System.Collections.IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();
				list.Add("Magical blade that does additional damage.");
				list.AddRange(base.DelveInfo);
				return list;
			}
		}
	}
}