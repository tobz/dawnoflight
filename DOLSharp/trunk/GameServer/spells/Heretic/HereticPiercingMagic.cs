using System;
using System.Collections;
using DawnOfLight.GameServer.Effects;
using DawnOfLight.GameServer.Events;
using DawnOfLight.GameServer.Events.GameObjects;
using DawnOfLight.GameServer.GameObjects;
using DawnOfLight.GameServer.Network;
using DawnOfLight.GameServer.Utilities;
using DawnOfLight.GameServer.World;

namespace DawnOfLight.GameServer.Spells.Heretic
{

	[SpellHandler("HereticPiercingMagic")]
	public class HereticPiercingMagic : SpellHandler
	{
        protected GameLiving focustarget = null;
        protected ArrayList m_focusTargets = null;
        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
            focustarget = target;
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (m_focusTargets == null)
                m_focusTargets = new ArrayList();
            GameLiving living = effect.Owner as GameLiving;
            lock (m_focusTargets.SyncRoot)
            {
                if (!m_focusTargets.Contains(effect.Owner))
                    m_focusTargets.Add(effect.Owner);

                MessageToCaster("You concentrated on the spell!", ChatType.CT_Spell);
            }
        }
        protected virtual void BeginEffect()
        {
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.CastStarting, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.AddHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
        }
        public void EventAction(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;
            MessageToCaster("You lose your concentration!", ChatType.CT_SpellExpires);
            RemoveEffect();
        }
        protected virtual void RemoveEffect()
        {
            if (m_focusTargets != null)
            {
                lock (m_focusTargets.SyncRoot)
                {
                    foreach (GameLiving living in m_focusTargets)
                    {
                        GameSpellEffect effect = FindEffectOnTarget(living, this);
                        if (effect != null)
                            effect.Cancel(false);
                    }
                }
            }
            MessageToCaster("You lose your concentration!", ChatType.CT_Spell);
            if (Spell.Pulse != 0 && Spell.Frequency > 0)
                CancelPulsingSpell(Caster, Spell.SpellType);

            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackFinished, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.CastStarting, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Moving, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.Dying, new DOLEventHandler(EventAction));
            GameEventMgr.RemoveHandler(m_caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(EventAction));
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendInterruptAnimation(m_caster);
            }
        }
	
		public HereticPiercingMagic(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
