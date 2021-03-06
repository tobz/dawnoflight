//Andraste v2.0 - Vico

using System;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.SkillHandler;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirBolt")]
	public class VampiirBoltSpellHandler : SpellHandler
	{
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (Caster.InCombat == true)
			{
				MessageToCaster("You cannot cast this spell in combat!", eChatType.CT_SpellResisted);
				return false;
			}
			return base.CheckBeginCast(selectedTarget);
		}
		public override void StartSpell(GameLiving target)
		{
			foreach (GameLiving targ in SelectTargets(target))
			{
				DealDamage(targ);
			}
		}

		private void DealDamage(GameLiving target)
		{
			int ticksToTarget = WorldMgr.GetDistance(m_caster, target) * 100 / 85; // 85 units per 1/10s
			int delay = 1 + ticksToTarget / 100;
			foreach (GamePlayer player in target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, (ushort)(delay), false, 1);
			}
			BoltOnTargetAction bolt = new BoltOnTargetAction(Caster, target, this);
			bolt.Start(1 + ticksToTarget);
		}

		public override void FinishSpellCast(GameLiving target)
		{
			if (target is Keeps.GameKeepDoor || target is Keeps.GameKeepComponent)
			{
				MessageToCaster("Your spell has no effect on the keep component!", eChatType.CT_SpellResisted);
				return;
			}
			base.FinishSpellCast(target);
		}

		protected class BoltOnTargetAction : RegionAction
		{
			protected readonly GameLiving m_boltTarget;
			protected readonly VampiirBoltSpellHandler m_handler;

			public BoltOnTargetAction(GameLiving actionSource, GameLiving boltTarget, VampiirBoltSpellHandler spellHandler)
				: base(actionSource)
			{
				if (boltTarget == null)
					throw new ArgumentNullException("boltTarget");
				if (spellHandler == null)
					throw new ArgumentNullException("spellHandler");
				m_boltTarget = boltTarget;
				m_handler = spellHandler;
			}

			protected override void OnTick()
			{
				GameLiving target = m_boltTarget;
				GameLiving caster = (GameLiving)m_actionSource;
				if (target == null) return;
				if (target.CurrentRegionID != caster.CurrentRegionID) return;
				if (target.ObjectState != GameObject.eObjectState.Active) return;
				if (!target.IsAlive) return;

				if (target == null) return;
				if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

				int power = 0;

				if (target is GameNPC || target.Mana > 0)
				{
					if (target is GameNPC) power = (int)Math.Round(((double)(target.Level) * (double)(m_handler.Spell.Value) * 2) / 100);
					else power = (int)Math.Round((double)(target.MaxMana) * (((double)m_handler.Spell.Value) / 250));
					if (target.Mana < power) power = target.Mana;
					caster.Mana += power;
					if (target is GamePlayer)
					{
						target.Mana -= power;
						((GamePlayer)target).Out.SendMessage(caster.Name + " takes " + power + " power!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
					}
					if (target.Mana < 0) target.Mana = 0;
					if (caster is GamePlayer)
					{
						((GamePlayer)caster).Out.SendMessage("You receive " + power + " power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					}
				}
				else ((GamePlayer)caster).Out.SendMessage("You did not receive any power from " + target.Name + "!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, caster);
			}
		}

		public VampiirBoltSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
}