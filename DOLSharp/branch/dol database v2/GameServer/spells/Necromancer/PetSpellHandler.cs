/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Spells;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Handler for spells that are issued by the player, but cast
	/// by his pet.
	/// </summary>
	/// <author>Aredhel</author>
	[SpellHandler("PetSpell")]
	class PetSpellHandler : SpellHandler
	{
		/// <summary>
		/// Cast the spell.
		/// </summary>
		public override void CastSpell()
		{
			GameLiving target = Caster.TargetObject as GameLiving;

			if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, target, Spell, SpellLine)
				&& CheckBeginCast(target))
			{
				if (Spell.CastTime > 0)
				{
					m_interrupted = false;
					SendSpellMessages();
					m_castTimer = new DelayedCastTimer(Caster, this, target);
					m_castTimer.Start(1 + CalculateCastingTime());
					SendCastAnimation();

					if (m_caster.IsMoving || m_caster.IsStrafing)
						CasterMoves();
				}
				else
				{
					FinishSpellCast(target);
				}
			}

            if (!IsCasting)
                OnAfterSpellCastSequence();
		}

		/// <summary>
		/// Calculate casting time based on delve and dexterity stat bonus.
		/// Necromancers do not benefit from ToA Casting Speed Bonuses.
		/// </summary>
		/// <returns></returns>
		public override int CalculateCastingTime()
		{
			int ticks = m_spell.CastTime;
			ticks = (int)(ticks * Math.Max(0.4, DexterityCastTimeReduction));
			if (ticks < 1)
				ticks = 1;
			return ticks;
		}

		/// <summary>
		/// Check if we have a pet to start with.
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			if (!base.CheckBeginCast(selectedTarget))
				return false;

			if (Caster.ControlledNpc == null)
			{
				MessageToCaster("You must have a pet summoned to cast this spell!",
					eChatType.CT_SpellResisted);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Called when spell has finished casting.
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			GamePlayer player = Caster as GamePlayer;
			if (player == null || player.ControlledNpc == null) return;

			// Now deduct mana for the spell (the pet will cast for free,
			// mana pool is *not* shared).

			int powerCost = CalculateNeededPower(Caster);
			if (powerCost > 0)
				Caster.ChangeMana(Caster, DOL.GS.GameLiving.eManaChangeType.Spell, -powerCost);

			// If there is an ID, create a sub spell for the pet.

			ControlledNpc petBrain = player.ControlledNpc as ControlledNpc;
			if (petBrain != null && Spell.SubSpellID > 0)
			{
				Spell spell = SkillBase.GetSpellByID(Spell.SubSpellID);
                if (spell != null && spell.SubSpellID == 0)
                {
                    spell.Level = Spell.Level;
                    petBrain.Notify(GameNPCEvent.PetSpell, this,
                        new PetSpellEventArgs(spell, SpellLine, target));
                }
			}

            //For Facilitate Painworking
            if (m_spell.RecastDelay > 0 && m_startReuseTimer)
            {
                if (m_caster is GamePlayer)
                {
                    foreach (Spell sp in SkillBase.GetSpellList(m_spellLine.KeyName))
                    {
                        if (sp.SpellType == m_spell.SpellType && sp.RecastDelay == m_spell.RecastDelay && sp.Group == m_spell.Group)
                        {
                            m_caster.DisableSkill(sp, sp.RecastDelay);
                        }
                    }
                }
                else if (m_caster is GameNPC)
                    m_caster.DisableSkill(m_spell, m_spell.RecastDelay);
            }
		}

		/// <summary>
		/// Creates a new pet spell handler.
		/// </summary>
		public PetSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
			: base(caster, spell, spellLine)
		{
		}
	}
}
