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
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// Default class for spell handler
	/// should be used as a base class for spell handler
	/// </summary>
	public class SpellHandler : ISpellHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private DelayedCastTimer m_castTimer;
		protected Spell m_spell;
		protected SpellLine m_spellLine;
		protected GameLiving m_caster;
		protected bool m_interrupted = false;
		protected bool m_startReuseTimer = true;
		protected const string INTERRUPT_TIMEOUT_PROPERTY = "CAST_INTERRUPT_TIMEOUT";
		protected const int SPELL_INTERRUPT_DURATION = 2000; //2 sec for all spells?

		public event CastingCompleteCallback CastingCompleteEvent;
		//public event SpellEndsCallback SpellEndsEvent;

		/// <summary>
		/// spell handler constructor
		/// <param name="caster">living that is casting that spell</param>
		/// <param name="spell">the spell to cast</param>
		/// <param name="spellLine">the spell line that spell belongs to</param>
		/// </summary>
		public SpellHandler(GameLiving caster, Spell spell, SpellLine spellLine)
		{
			m_caster = caster;
			m_spell = spell;
			m_spellLine = spellLine;
		}

		/// <summary>
		/// Returns the string representation of the SpellHandler
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(128)
				.Append("Caster=").Append(Caster==null ? "(null)" : Caster.Name)
				.Append(", IsCasting=").Append(IsCasting)
				.Append(", m_interrupted=").Append(m_interrupted)
				.Append("\nSpell: ").Append(Spell==null?"(null)":Spell.ToString())
				.Append("\nSpellLine: ").Append(SpellLine==null?"(null)":SpellLine.ToString())
				.ToString();
		}

		#region Pulsing Spells

		/// <summary>
		/// When spell pulses
		/// </summary>
		public virtual void OnSpellPulse(PulsingSpellEffect effect)
		{
			if (Caster.Alive == false)
			{
				effect.Cancel(false);
				return;
			}
			if (Caster.ObjectState != eObjectState.Active)
				return;
			if (Caster.Stun || Caster.Mez)
				return;

			// no instrument anymore = stop the song
			if (m_spell.InstrumentRequirement != 0)
			{
				Instrument instrument = GetCasterInstrument();
				if (instrument == null || (int)instrument.Type != m_spell.InstrumentRequirement)
				{
					MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
					effect.Cancel(false);
					return;
				}
			}

			if (Caster.Mana > Spell.PulsePower)
			{
				Caster.ChangeMana(null, -Spell.PulsePower);
				if (Spell.InstrumentRequirement != 0 || !HasPositiveEffect)
					SendEffectAnimation(Caster, 0, true, 1); // pulsing auras or songs
				StartSpell(Caster.TargetObject as GameLivingBase);
			}
			else
			{
				effect.Cancel(false);
			}
		}

		/// <summary>
		/// Checks if caster holds the right instrument for this spell
		/// </summary>
		/// <returns>true if right instrument</returns>
		protected Instrument GetCasterInstrument()
		{
			if(Caster.Inventory != null)
			{
				switch (Caster.ActiveWeaponSlot)
				{
					case GameLiving.eActiveWeaponSlot.Standard : return Caster.Inventory.GetItem(eInventorySlot.RightHandWeapon) as Instrument;
					case GameLiving.eActiveWeaponSlot.TwoHanded: return Caster.Inventory.GetItem(eInventorySlot.TwoHandWeapon) as Instrument;
					case GameLiving.eActiveWeaponSlot.Distance : return Caster.Inventory.GetItem(eInventorySlot.DistanceWeapon) as Instrument;
				}
			}
			return null;
		}

		/// <summary>
		/// Cancels first pulsing spell of type
		/// </summary>
		/// <param name="living">owner of pulsing spell</param>
		/// <param name="spellType">type of spell to cancel</param>
		/// <returns>true if any spells were canceled</returns>
		public static bool CancelPulsingSpell(GameLiving living, string spellType)
		{
			lock (living.ConcentrationEffects)
			{
				for (int i = 0; i < living.ConcentrationEffects.Count; i++)
				{
					PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
					if (effect == null)
						continue;
					if (effect.SpellHandler.Spell.SpellType == spellType)
					{
						effect.Cancel(false);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Cancels all pulsing spells
		/// </summary>
		/// <param name="living"></param>
		public static void CancelAllPulsingSpells(GameLiving living)
		{
			lock (living.ConcentrationEffects)
			{
				for (int i = 0; i < living.ConcentrationEffects.Count; i++)
				{
					PulsingSpellEffect effect = living.ConcentrationEffects[i] as PulsingSpellEffect;
					if (effect == null)
						continue;
					effect.Cancel(false);
				}
			}
		}

		#endregion

		/// <summary>
		/// called whenever the player clicks on a spell icon
		/// or a GameLiving wants to cast a spell
		/// </summary>
		public virtual void CastSpell()
		{
			// nightshade is unstealthed even if no target, target is same realm, target is too far
			if (Caster is GamePlayer)
			{
				((GamePlayer) Caster).Stealth(false);
			}

			m_interrupted = false;
			GameLivingBase target = Caster.TargetObject as GameLivingBase;
			bool check = CheckBeginCast(target);
			if (Spell.Pulse != 0 && CancelPulsingSpell(Caster, Spell.SpellType))
			{
				// is done even if caster is sitting
				if (Spell.InstrumentRequirement == 0)
					MessageToCaster("You cancel your effect.", eChatType.CT_Spell);
				else
					MessageToCaster("You stop playing your song.", eChatType.CT_Spell);
			}
			else if (GameServer.ServerRules.IsAllowedToCastSpell(Caster, target, Spell, m_spellLine) && check)
			{
				if (Spell.CastTime > 0)
				{
					// no instant cast
					m_interrupted = false;
					if (Spell.InstrumentRequirement == 0)
						MessageToCaster("You begin casting a " + Spell.Name + " spell!", eChatType.CT_Spell);
					else
						MessageToCaster("You begin playing " + Spell.Name + "!", eChatType.CT_Spell);

					//set the time when casting to can not quickcast during a minimum time
//					if (m_caster is GamePlayer && ((GamePlayer)m_caster).IsQuickCasting)
//						((GamePlayer)m_caster).TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, GameTimer.CurrentTick);

					m_castTimer = new DelayedCastTimer(Caster, this, target);
					m_castTimer.Start(1 + CalculateCastingTime());
					SendCastAnimation();

					if (m_caster is GamePlayer)
					{
						if (m_caster.IsMoving || ((GamePlayer) m_caster).Strafing)
						{
							CasterMoves();
						}
					}
				}
				else
				{
					// instant cast
					FinishSpellCast(target);
				}
			}
			if (!IsCasting)
			{
				OnAfterSpellCastSequence();
			}
		}

		/// <summary>
		/// Is called when the caster moves
		/// </summary>
		public virtual void CasterMoves()
		{
			if (Spell.InstrumentRequirement != 0)
				return; // song can be played while moving
			MessageToCaster("You move and interrupt your spellcast!", eChatType.CT_System);
			InterruptCasting();
		}

		/// <summary>
		/// casting sequence has a chance for interrupt through attack from enemy
		/// the final decision and the interrupt is done here
		/// TODO: con level dependend
		/// </summary>
		/// <param name="attacker">attacker that interrupts the cast sequence</param>
		/// <returns>true if casting was interrupted</returns>
		public virtual bool CasterIsAttacked(GameLiving attacker)
		{
			if (Caster.EffectList.GetOfType(typeof (QuickCastEffect)) != null)
				return false;
			if (IsCasting)
			{
				double mod = Caster.GetConLevel(attacker);
				double chance = 65;
				chance += mod*10;
				chance = Math.Max(1, chance);
				chance = Math.Min(99, chance);
				if (Util.Chance((int) chance))
				{
					Caster.TempProperties.setProperty(INTERRUPT_TIMEOUT_PROPERTY, Caster.Region.Time + SPELL_INTERRUPT_DURATION);
					MessageToCaster(attacker.GetName(0, true) + " is attacking you and your spell is interrupted!", eChatType.CT_SpellResisted);
					InterruptCasting(); // always interrupt at the moment
					return true;
				}
			}
			return false;
		}

		#region begin & end cast check

		/// <summary>
		/// All checks before any casting begins
		/// </summary>
		/// <param name="selectedTarget"></param>
		/// <returns></returns>
		public virtual bool CheckBeginCast(GameLivingBase selectedTarget)
		{
			if (m_caster.ObjectState != eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.Alive)
			{
				MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				Instrument instrument = GetCasterInstrument();
				if (instrument == null || (int)instrument.Type != m_spell.InstrumentRequirement)
				{
					MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.Sitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster.AttackState)
			{
				if (m_spell.CastTime != 0)
				{
					m_caster.StopAttack();
					return false;
				}
			}

			if (m_spell.CastTime > 0 && m_caster is GamePlayer)
			{
				long leftseconds = Math.Max(
					Caster.TempProperties.getLongProperty(INTERRUPT_TIMEOUT_PROPERTY, 0) - Caster.Region.Time,
					Caster.SwingTimeLeft);
				if (leftseconds > 0)
				{
					MessageToCaster("You must wait " + (leftseconds/1000+1).ToString() + " seconds to cast a spell!", eChatType.CT_System);
					return false;
				}
				Caster.TempProperties.removeProperty(INTERRUPT_TIMEOUT_PROPERTY);
			}

			if (m_spell.RecastDelay > 0 && m_caster is GamePlayer)
			{
				int left = ((GamePlayer) m_caster).GetSkillDisabledDuration(m_spell);
				if (left > 0)
				{
					MessageToCaster("You must wait " + (left/1000 + 1).ToString() + " seconds to use this spell!", eChatType.CT_System);
					return false;
				}
			}

			if (m_spell.Target == "Area")
			{
				if (!m_caster.GroundTarget.CheckDistance(m_caster.Position, CalculateSpellRange()))
				{
					MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
//				if (!Caster.GroundTargetInView)
//				{
//					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
//					return false;
//				}
			}
			else if (m_spell.Target != "Self" && m_spell.Target != "Group" && m_spell.Range > 0)
			{
				//all spells that need a target

				if (selectedTarget == null || selectedTarget.ObjectState != eObjectState.Active)
				{
					MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
					return false;
				}

				if (!m_caster.Position.CheckDistance(selectedTarget.Position, CalculateSpellRange()))
				{
					MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return false;
				}


				switch (m_spell.Target.ToLower())
				{
					case "enemy":
						if (selectedTarget == m_caster)
						{
							MessageToCaster("You can't attack yourself! ", eChatType.CT_System);
							return false;
						}

						//enemys have to be in front and in view for targeted spells
						if (!(m_caster.IsObjectInFront(selectedTarget, 180) && m_caster.TargetInView))
						{
							MessageToCaster("Your target is not in view.", eChatType.CT_SpellResisted);
							return false;
						}

						if (!GameServer.ServerRules.IsAllowedToAttack(Caster, selectedTarget, false))
						{
							return false;
						}
						break;

					case "corpse":
						if (selectedTarget.Alive || !GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, true))
						{
							MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "realm":
						if (!GameServer.ServerRules.IsSameRealm(Caster, selectedTarget, false))
						{
							return false;
						}
						break;

					case "pet":
						if (Caster is GamePlayer)
						{
							GamePlayer casterPlayer = (GamePlayer)Caster;
							if (casterPlayer.ControlledNpc == null)
							{
								// TODO: correct message?
								MessageToCaster("Not controlling anything.", eChatType.CT_SpellResisted);
								return false;
							}
						}
						break;
				}

				//heals/buffs/rez need LOS only to start casting
				if (!m_caster.TargetInView)
				{
					MessageToCaster("Your target is not in view.", eChatType.CT_SpellResisted);
					return false;
				}

				if (m_spell.Target != "Corpse" && !selectedTarget.Alive)
				{
					MessageToCaster(selectedTarget.GetName(0, true) + " is dead!", eChatType.CT_SpellResisted);
					return false;
				}
			}

			if (m_caster.Mana < CalculateNeededPower(selectedTarget))
			{
				MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Concentration > 0)
			{
				if (m_caster.Concentration < m_spell.Concentration)
				{
					MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
					return false;
				}

				if (m_caster.ConcentrationEffects.ConcSpellsCount >= 20)
				{
					MessageToCaster("You can only cast up to 20 simultaneous concentration spells!", eChatType.CT_SpellResisted);
					return false;
				}
			}

//			if (m_caster is GamePlayer) 
//			{
////				if (this.CheckDisabled(spl)) {
////					this.Out.SendMessage("You must wait for the "+spl.RecastDelay+" second recast delay to expire!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
////					return false;
////				}
////				if (((bool)this.TempProperties.getObjectProperty("MEZZED", false))!=false) {
////					//TODO: Code for Realm Abilitys which nullify mez or stun.. eg purge
////					return false;
////				}
////				if (((bool)this.TempProperties.getObjectProperty("STUNNED", false))!=false) {
////					//TODO: Code for Realm Abilitys which nullify mez or stun.. eg purge
////					return false;
////				}
//			} else {
//				return true;
//			}

			// Cancel engage if user starts attack
			EngageEffect engage = (EngageEffect) m_caster.EffectList.GetOfType(typeof (EngageEffect));
			if (engage != null)
			{
				engage.Cancel(false);
			}

			return true;
		}

		public virtual void CheckLOSYouToPet(GamePlayer player, ushort response)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100) // In view ?
				return;
			MessageToLiving(player, "Your pet not in view.", eChatType.CT_SpellResisted);
			InterruptCasting(); // break;
		}

		public virtual void CheckLOSYouToTarget(GamePlayer player, ushort response)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100) // In view ?
				return;
			MessageToLiving(player, "Your can't see target now.", eChatType.CT_SpellResisted);
			InterruptCasting(); // break;
		}

		public virtual void CheckLOSPetToTarget(GamePlayer player, ushort response)
		{
			if (player == null) // Hmm
				return;
			if ((response & 0x100) == 0x100) // In view ?

				return;
			MessageToLiving(player, "Your pet can't see target.", eChatType.CT_SpellResisted);
			InterruptCasting(); // break;
		}

		/// <summary>
		/// Checks after casting before spell is executed
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CheckEndCast(GameLivingBase target)
		{
			if (m_interrupted)
			{
				return false;
			}

			if (m_caster.ObjectState != eObjectState.Active)
			{
				return false;
			}

			if (!m_caster.Alive)
			{
				MessageToCaster("You are dead and can't cast!", eChatType.CT_System);
				return false;
			}

			if (m_spell.InstrumentRequirement != 0)
			{
				Instrument instrument = GetCasterInstrument();
				if (instrument == null || (int)instrument.Type != m_spell.InstrumentRequirement)
				{
					MessageToCaster("You are not wielding the right type of instrument!", eChatType.CT_SpellResisted);
					return false;
				}
			}
			else if (m_caster.Sitting) // songs can be played if sitting
			{
				//Purge can be cast while sitting but only if player has negative effect that
				//don't allow standing up (like stun or mez)
				MessageToCaster("You can't cast while sitting!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Target == "Area")
			{
				if (!m_caster.GroundTarget.CheckDistance(m_caster.Position, CalculateSpellRange()))
				{
					MessageToCaster("Your area target is out of range.  Select a closer target.", eChatType.CT_SpellResisted);
					return false;
				}
//				if (!Caster.GroundTargetInView)
//				{
//					MessageToCaster("Your ground target is not in view!", eChatType.CT_SpellResisted);
//					return false;
//				}
			}
			else if (m_spell.Target != "Self" && m_spell.Target != "Group" && m_spell.Range > 0)
			{
				//all spells that need a target

				if (target == null || target.ObjectState != eObjectState.Active)
				{
					MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
					return false;
				}

				if (!m_caster.Position.CheckDistance(target.Position, CalculateSpellRange()))
				{
					MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
					return false;
				}

				switch (m_spell.Target)
				{
					case "Enemy":
						//enemys have to be in front and in view for targeted spells
						if (!(m_caster.IsObjectInFront(target, 180) && m_caster.TargetInView))
						{
							MessageToCaster("Your target is not in view.  The spell fails.", eChatType.CT_SpellResisted);
							return false;
						}

						if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, false))
						{
							return false;
						}
						break;

					case "Corpse":
						if (target.Alive || !GameServer.ServerRules.IsSameRealm(Caster, target, true))
						{
							MessageToCaster("This spell only works on dead members of your realm!", eChatType.CT_SpellResisted);
							return false;
						}
						break;

					case "Realm":
						if (!GameServer.ServerRules.IsSameRealm(Caster, target, false))
						{
							return false;
						}
						break;

					case "Pet":
						if (Caster is GamePlayer)
						{
							GamePlayer casterPlayer = (GamePlayer)Caster;
							if (casterPlayer.ControlledNpc == null)
							{
								// TODO: correct message?
								MessageToCaster("Not controlling anything.", eChatType.CT_SpellResisted);
								return false;
							}
						}
						break;
				}
			}

			if (m_caster.Mana <= 0 && Spell.Power != 0)
			{
				MessageToCaster("You have exhausted all of your power and cannot cast spells!", eChatType.CT_SpellResisted);
				return false;
			}
			if (m_caster.Mana < CalculateNeededPower(target))
			{
				MessageToCaster("You don't have enough power to cast that!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_caster.Concentration < m_spell.Concentration)
			{
				MessageToCaster("This spell requires " + m_spell.Concentration + " concentration points to cast!", eChatType.CT_SpellResisted);
				return false;
			}

			if (m_spell.Concentration > 0 && m_caster.ConcentrationEffects.ConcSpellsCount >= 20)
			{
				MessageToCaster("You can only cast up to 20 simultaneous concentration spells!", eChatType.CT_SpellResisted);
				return false;
			}

			return true;
		}

		#endregion

		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual int CalculateNeededPower(GameLivingBase target)
		{
			double power = m_spell.Power*1.2;
			eProperty focusProp = SkillBase.SpecToFocus(SpellLine.Spec);
			if (focusProp != eProperty.Undefined)
			{
				double focusBonus = Caster.GetModified(focusProp)*0.4;
				if (Spell.Level > 0)
					focusBonus /= Spell.Level;
				if (focusBonus > 0.4)
					focusBonus = 0.4;
				else if (focusBonus < 0)
					focusBonus = 0;
				power -= m_spell.Power*focusBonus;
			}
			// percent of maxPower if less than zero
			if (power < 0)
				power = Caster.MaxMana*power*-0.01;
			// doubled power usage if quickcasting
			if (Caster.EffectList.GetOfType(typeof (QuickCastEffect)) != null && Spell.CastTime > 0)
				power *= 2;
			return (int) power;
		}

		/// <summary>
		/// Calculates the range to target needed to cast the spell
		/// </summary>
		/// <returns></returns>
		public virtual int CalculateSpellRange()
		{
			return Math.Max(32, (int)(Spell.Range*Caster.GetModified(eProperty.SpellRange)*0.01));
		}

		/// <summary>
		/// Called whenever the casters casting sequence is to interrupt immediately
		/// </summary>
		public virtual void InterruptCasting()
		{
			if (m_interrupted || !IsCasting)
				return;

			if (IsCasting)
			{
				foreach (GamePlayer player in m_caster.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendInterruptAnimation(m_caster);
				}
			}

			m_interrupted = true;
			if (m_castTimer != null)
			{
				m_castTimer.Stop();
				m_castTimer = null;
				if (m_caster is GamePlayer)
					((GamePlayer)m_caster).ClearSpellQueue();
				else if (m_caster is GameNPC)
					((GameNPC)m_caster).StopSpellAttack();
			}
			OnAfterSpellCastSequence();
		}

		/// <summary>
		/// Casts a spell after the CastTime delay
		/// </summary>
		private class DelayedCastTimer : GameTimer
		{
			/// <summary>
			/// The spellhandler instance with callbacks
			/// </summary>
			private readonly SpellHandler m_handler;
			/// <summary>
			/// The target object at the moment of CastSpell call
			/// </summary>
			private readonly GameLivingBase m_target;

			/// <summary>
			/// Constructs a new DelayedSpellTimer
			/// </summary>
			/// <param name="actionSource">The caster</param>
			/// <param name="handler">The spell handler</param>
			/// <param name="target">The target object</param>
			public DelayedCastTimer(GameLiving actionSource, SpellHandler handler, GameLivingBase target) : base(actionSource.Region.TimeManager)
			{
				if (handler == null)
					throw new ArgumentNullException("handler");
				m_handler = handler;
				m_target = target;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLivingBase target = m_target;

				try
				{
					if (m_handler.CheckEndCast(target))
					{
						m_handler.FinishSpellCast(target);
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(ToString(), e);
				}
				m_handler.OnAfterSpellCastSequence();
			}

			/// <summary>
			/// Returns short information about the timer
			/// </summary>
			/// <returns>Short info about the timer</returns>
			public override string ToString()
			{
				return new StringBuilder(base.ToString(), 128)
					.Append(" spellhandler: (").Append(m_handler.ToString()).Append(')')
					.ToString();
			}
		}

		/// <summary>
		/// Calculates the effective casting time
		/// </summary>
		/// <returns>effective casting time in milliseconds</returns>
		public virtual int CalculateCastingTime()
		{
			int ticks = m_spell.CastTime;
			if (m_caster is GamePlayer && Spell.InstrumentRequirement == 0) // dex has no effect on songs - can't play a song faster, right?
			{
				if (m_caster.EffectList.GetOfType(typeof (QuickCastEffect)) != null)
				{
					return 2000; //always 2 sec
				}
				ticks = (int)(ticks * (1.0 - Math.Min(0.6, (((GamePlayer)m_caster).GetModified(eProperty.Dexterity) - 60)/600.0)));
			}
			if (ticks < 1)
				ticks = 1; // at least 1 tick
			return ticks;
		}

		#region animations

		public virtual void SendCastAnimation()
		{
			ushort castTime = (ushort)(CalculateCastingTime()/100);
			foreach (GamePlayer player in m_caster.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellCastAnimation(m_caster, m_spell.ClientEffect, castTime);
			}
		}

		public virtual void SendEffectAnimation(GameLiving target, ushort boltDuration, bool noSound, byte success)
		{
			if (target == null)
				target = m_caster;

			foreach (GamePlayer player in target.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendSpellEffectAnimation(m_caster, target, m_spell.ClientEffect, boltDuration, noSound, success);
			}
		}

		public virtual void SendInterruptCastAnimation()
		{
			foreach (GamePlayer player in m_caster.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendInterruptAnimation(m_caster);
			}
		}

		#endregion

		/// <summary>
		/// called after normal spell cast is completed and effect has to be started
		/// </summary>
		public virtual void FinishSpellCast(GameLivingBase target)
		{
			// endurance
			m_caster.EndurancePercent -= 5;

			// messages
			if (Spell.InstrumentRequirement == 0)
			{
				MessageToCaster("You cast a " + m_spell.Name + " spell!", eChatType.CT_Spell);
				foreach (GamePlayer player in m_caster.GetInRadius(typeof(GamePlayer), WorldMgr.INFO_DISTANCE))
				{
					if (player != m_caster)
						player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			if (m_spell.Pulse != 0 && m_spell.Frequency > 0)
			{
				CancelAllPulsingSpells(Caster);
				PulsingSpellEffect effect = new PulsingSpellEffect(this);
				effect.Start();
				// show animation on caster for positive spells, negative shows on every StartSpell
				if (m_spell.Target == "Self" || m_spell.Target == "Group")
					SendEffectAnimation(Caster, 0, false, 1);
			}

			StartSpell(target); // and action

			//the quick cast is unallowed whenever you miss the spell	
			//set the time when casting to can not quickcast during a minimum time
			if (m_caster is GamePlayer)
			{
				QuickCastEffect quickcast = (QuickCastEffect) m_caster.EffectList.GetOfType(typeof (QuickCastEffect));
				if (quickcast != null && Spell.CastTime > 0)
				{
					((GamePlayer) m_caster).TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, m_caster.Region.Time);
					((GamePlayer) m_caster).DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
					quickcast.Cancel(false);
				}
			}

			// disable spells with recasttimer (Disables group of same type with same delay)
			if (m_spell.RecastDelay > 0 && m_startReuseTimer && m_caster is GamePlayer)
			{
				foreach (Spell sp in SkillBase.GetSpellList(m_spellLine.KeyName))
				{
					if (sp.SpellType == m_spell.SpellType && sp.RecastDelay == m_spell.RecastDelay && sp.Group == m_spell.Group)
					{
						((GamePlayer) m_caster).DisableSkill(sp, sp.RecastDelay);
					}
				}
			}
		}

		/// <summary>
		/// Select all targets for this spell
		/// </summary>
		/// <param name="castTarget"></param>
		/// <returns></returns>
		public virtual IList SelectTargets(GameLivingBase castTarget)
		{
			ArrayList list = new ArrayList(8);

			switch (Spell.Target.ToLower())
			{
					// GTAoE
				case "area":
					if (Spell.Radius > 0)
					{
						foreach (GameLivingBase living in Caster.Region.GetInRadius(typeof(GameLivingBase), Caster.GroundTarget, (ushort) Spell.Radius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, living, true))
							{
								list.Add(living);
							}
						}
					}
					break;

				case "corpse":
					if (castTarget != null && !castTarget.Alive)
						list.Add(castTarget);
					break;

				case "pet":
					if (Caster is GamePlayer)
					{
						IControlledBrain npc = ((GamePlayer)Caster).ControlledNpc;
						if (npc != null)
							list.Add(npc.Body);
					}
					else
					{
						// ...
					}
					break;

				case "enemy":
					if (Spell.Radius > 0)
					{
						if (castTarget == null || Spell.Range == 0)
							castTarget = Caster;
						foreach (GameLivingBase living in castTarget.GetInRadius(typeof(GameLivingBase), (ushort) Spell.Radius))
						{
							if (GameServer.ServerRules.IsAllowedToAttack(Caster, living, true))
							{
								list.Add(living);
							}
						}
					}
					else
					{
						if (castTarget != null && GameServer.ServerRules.IsAllowedToAttack(Caster, castTarget, true))
							list.Add(castTarget);
					}
					break;

				case "realm":
					if (Spell.Radius > 0)
					{
						if (castTarget == null || Spell.Range == 0)
							castTarget = Caster;
						foreach (GameLivingBase living in castTarget.GetInRadius(typeof(GameLivingBase), (ushort) Spell.Radius))
						{
							if (GameServer.ServerRules.IsSameRealm(Caster, living, true))
							{
								list.Add(living);
							}
						}
					}
					else
					{
						if (castTarget != null && GameServer.ServerRules.IsSameRealm(Caster, castTarget, true))
							list.Add(castTarget);
					}
					break;

				case "self":
					list.Add(Caster);
					break;

				case "group":
					if (Caster is GamePlayer)
					{
						GamePlayer casterPlayer = (GamePlayer)Caster;
						PlayerGroup group = casterPlayer.PlayerGroup;
						int spellRange = CalculateSpellRange();
						if (group == null)
						{
							list.Add(casterPlayer);
							IControlledBrain npc = casterPlayer.ControlledNpc;
							if (npc != null)
							{
								if (casterPlayer.Position.CheckDistance(npc.Body.Position, spellRange))
									list.Add(npc.Body);
							}
						}
						else
						{
							lock (group)
							{
								foreach (GamePlayer groupPlayer in group)
								{
									// only players in range
									if (casterPlayer.Position.CheckDistance(groupPlayer.Position, spellRange))
										list.Add(groupPlayer);

									IControlledBrain npc = groupPlayer.ControlledNpc;
									if (npc != null)
									{
										if (casterPlayer.Position.CheckDistance(npc.Body.Position, spellRange))
											list.Add(npc.Body);
									}
								}
							}
						}
					}
					else
					{
						list.Add(Caster);
					}
					break;

			}
			return list;
		}

		/// <summary>
		/// called when spell effect has to be started and applied to targets
		/// </summary>
		/// <param name="target">The current target object</param>
		public virtual void StartSpell(GameLivingBase target)
		{
			foreach (GameLivingBase spellTarget in SelectTargets(target))
			{
				GameLiving t = spellTarget as GameLiving;
				if(t == null)
				{
					MessageToCaster("The spell fails to affect "+ spellTarget.GetName(0, false) +"!", eChatType.CT_SpellResisted);
					continue;
				}

				if (Util.Chance(CalculateSpellResistChance(t)))
				{
					OnSpellResisted(t);
					continue;
				}

				if (Spell.Radius == 0)
				{
					ApplyEffectOnTarget(t, 1.0);
				}
				else if (Spell.Target == "Area")
				{
					int dist = t.Position.GetDistance(Caster.GroundTarget);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (1 - dist/(double) Spell.Radius));
					}
				}
				else
				{
					int dist = t.Position.GetDistance(target.Position);
					if (dist >= 0)
					{
						ApplyEffectOnTarget(t, (1 - dist/(double) Spell.Radius));
					}
				}
			}
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected virtual int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = Spell.Duration;
			if (Spell.InstrumentRequirement != 0)
			{
				Instrument instrument = GetCasterInstrument();
				if (instrument != null)
				{
					duration *= 1.0 + Math.Min(1.0, instrument.Level/(double)Caster.Level); // up to 200% duration for songs
					duration *= instrument.Condition/100 * instrument.Quality/100;
				}
			}

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		/// <summary>
		/// Creates the corresponding spell effect for the spell
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		/// <returns></returns>
		protected virtual GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, CalculateEffectDuration(target, effectiveness), 0, effectiveness);
		}

		/// <summary>
		/// Apply effect on target or do spell action if non duration spell
		/// </summary>
		/// <param name="target">target that gets the effect</param>
		/// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
		public virtual void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			if (effectiveness <= 0)
				return; // no effect

			if (Spell.Duration > 0 || Spell.Concentration > 0)
			{
				if (!target.Alive)
					return;
				eChatType noOverwrite = (Spell.Pulse == 0) ? eChatType.CT_SpellResisted : eChatType.CT_SpellPulse;
				GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
				GameSpellEffect overwriteEffect = null;
				bool foundInList = false;
				lock (target.EffectList)
				{
					foreach (IGameEffect effect in target.EffectList)
					{
						if (effect is GameSpellEffect)
						{
							GameSpellEffect gsp = (GameSpellEffect) effect;
							if (gsp.SpellHandler.IsOverwritable(neweffect))
							{
								foundInList = true;
								if (gsp is GameSpellAndImmunityEffect)
								{
									GameSpellAndImmunityEffect immunity = (GameSpellAndImmunityEffect) gsp;
									if (immunity.ImmunityState && immunity.Owner is GamePlayer)
									{
										SendEffectAnimation(target, 0, false, 0); //resisted effect
										MessageToCaster(immunity.Owner.GetName(0, true) + " can't have that effect again yet!", noOverwrite);
										break;
									}
								}
								if (IsNewEffectBetter(gsp, neweffect))
								{
									overwriteEffect = gsp;
								}
								else
								{
									if (target == m_caster)
									{
										MessageToCaster("You already have that effect. Wait until it expires.  Spell failed.", noOverwrite);
									}
									else
									{
										MessageToCaster(target.GetName(0, true) + " already has that effect.", noOverwrite);
										MessageToCaster("Wait until it expires.  Spell Failed.", noOverwrite);
									}
									// show resisted effect if spell failed
									if (Spell.Pulse == 0)
										SendEffectAnimation(target, 0, false, 0);
								}
								break;
							}
						}
					}
				}

				if (!foundInList)
				{
					neweffect.Start(target);
				}
				else if (overwriteEffect != null)
				{
					overwriteEffect.Overwrite(neweffect);
				}
			}
			else
			{
				OnDirectEffect(target, effectiveness);
			}
		}

		/// <summary>
		/// Called when cast sequence is complete
		/// </summary>
		public virtual void OnAfterSpellCastSequence()
		{
			if (CastingCompleteEvent != null)
			{
				CastingCompleteEvent(this);
			}
		}

		/// <summary>
		/// Determines wether this spell is better than given one
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <returns>true if this spell is better version than compare spell</returns>
		public virtual bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect)
		{
			Spell oldspell = oldeffect.Spell;
			Spell newspell = neweffect.Spell;
//			if (oldspell.SpellType != newspell.SpellType)
//			{
//				if (log.IsWarnEnabled)
//					log.Warn("Spell effect compare with different types " + oldspell.SpellType + " <=> " + newspell.SpellType + "\n" + Environment.StackTrace);
//				return false;
//			}
			if (oldspell.Concentration > 0)
				return false;
			if (newspell.Damage < oldspell.Damage)
				return false;
			if (newspell.Value < oldspell.Value)
				return false;
			//makes problems for immunity effects
			if (oldeffect is GameSpellAndImmunityEffect == false || ((GameSpellAndImmunityEffect) oldeffect).ImmunityState == false)
			{
				if (neweffect.Duration <= oldeffect.RemainingTime)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable cannot stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public virtual bool IsOverwritable(GameSpellEffect compare)
		{
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (compare.Spell.SpellType != Spell.SpellType)
				return false;
			return true;
		}

		/// <summary>
		/// execute non duration spell effect on target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="effectiveness"></param>
		public virtual void OnDirectEffect(GameLiving target, double effectiveness)
		{
		}

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public virtual void OnEffectStart(GameSpellEffect effect)
		{
			if (Spell.Pulse == 0)
				SendEffectAnimation(effect.Owner, 0, false, 1);
		}

		/// <summary>
		/// When an applied effect pulses
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public virtual void OnEffectPulse(GameSpellEffect effect)
		{
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public virtual int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			return 0;
		}

		/// <summary>
		/// Calculates chance of spell getting resisted
		/// </summary>
		/// <param name="target">the target of the spell</param>
		/// <returns>chance that spell will be resisted for specific target</returns>
		public virtual int CalculateSpellResistChance(GameLiving target)
		{
			if (HasPositiveEffect)
				return 0;
			return 100 - CalculateToHitChance(target);
		}

		/// <summary>
		/// When spell was resisted
		/// </summary>
		/// <param name="target">the target that resisted the spell</param>
		protected virtual void OnSpellResisted(GameLiving target)
		{
			SendEffectAnimation(target, 0, false, 0);
			if (target is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)target).Brain as IControlledBrain;
				if(brain != null)
				{
					GamePlayer owner = brain.Owner;
					if (owner != null && owner.ControlledNpc != null && target == owner.ControlledNpc.Body)
					{
						MessageToLiving(owner, "Your "+ target.Name +" resist the effect!", eChatType.CT_SpellResisted); 
					}
				}
			}
			else
			{
				MessageToLiving(target, "You resist the effect!", eChatType.CT_SpellResisted);
			}
			MessageToCaster(target.GetName(0, true) + " resists the effect!", eChatType.CT_SpellResisted);
			
			if (Spell.Damage != 0)
			{
				// notify target about missed attack for spells with damage
				AttackData ad = new AttackData();
				ad.Attacker = Caster;
				ad.Target = target;
				ad.AttackType = AttackData.eAttackType.Spell;
				ad.AttackResult = GameLiving.eAttackResult.Missed;
				target.OnAttackedByEnemy(ad, target);
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, ad.AttackType, Caster);
			}
			else if (Spell.CastTime > 0)
			{
				target.StartInterruptTimer(SPELL_INTERRUPT_DURATION, AttackData.eAttackType.Spell, Caster);
			}

			if (target is GameNPC)
			{
				IAggressiveBrain aggroBrain = ((GameNPC)target).Brain as IAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
			target.LastAttackedByEnemyTick = target.Region.Time;
			Caster.LastAttackTick = Caster.Region.Time;
		}

		#region messages

		/// <summary>
		/// sends a message to the caster
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public void MessageToCaster(string message, eChatType type)
		{
			if (m_caster is GamePlayer)
			{
				((GamePlayer) m_caster).Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// sends a message to a living
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public void MessageToLiving(GameLiving living, string message, eChatType type)
		{
			if (living is GamePlayer && message != null && message.Length > 0)
			{
				((GamePlayer) living).Out.SendMessage(message, type, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		public Spell Spell
		{
			get { return m_spell; }
		}

		public SpellLine SpellLine
		{
			get { return m_spellLine; }
		}

		public GameLiving Caster
		{
			get { return m_caster; }
		}

		public bool IsCasting
		{
			get { return m_castTimer != null && m_castTimer.IsAlive; }
		}

		public virtual bool HasPositiveEffect
		{
			get
			{
				if (m_spell.Target != "Enemy")
					return true;
				return false;
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public virtual IList DelveInfo
		{
			get
			{
				ArrayList list = new ArrayList();

				list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add("Instrument require: " + GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement));
				if (Spell.Damage != 0)
					list.Add("Damage: " + Spell.Damage.ToString("0.###;0.###'%'"));
				if (Spell.LifeDrainReturn != 0)
					list.Add("Health returned: " + Spell.LifeDrainReturn + "% of damage dealt");
				else if (Spell.Value != 0)
					list.Add("Value: " + Spell.Value.ToString("0.###;0.###'%'"));
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
				if (Spell.Power != 0)
					list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				if (Spell.Concentration != 0)
					list.Add("Concentration cost: " + Spell.Concentration);
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);
				if (Spell.DamageType != eDamageType.Natural)
					list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));
				
				return list;
			}
		}

		/// <summary>
		/// Find effect by spell type
		/// </summary>
		/// <param name="target"></param>
		/// <param name="spellType"></param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, string spellType)
		{
			lock (target.EffectList)
			{
				foreach (IGameEffect fx in target.EffectList)
				{
					if (!(fx is GameSpellEffect))
						continue;
					GameSpellEffect effect = (GameSpellEffect) fx;
					if (fx is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect) fx).ImmunityState)
						continue; // ignore immunity effects
					if (effect.SpellHandler.Spell != null && (effect.SpellHandler.Spell.SpellType == spellType))
					{
						return effect;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find effect by spell handler
		/// </summary>
		/// <param name="target"></param>
		/// <param name="spellHandler"></param>
		/// <returns>first occurance of effect in target's effect list or null</returns>
		public static GameSpellEffect FindEffectOnTarget(GameLiving target, ISpellHandler spellHandler)
		{
			lock (target.EffectList)
			{
				foreach (IGameEffect effect in target.EffectList)
				{
					GameSpellEffect gsp = effect as GameSpellEffect;
					if (gsp == null)
						continue;
					if (gsp.SpellHandler != spellHandler)
						continue;
					if (gsp is GameSpellAndImmunityEffect && ((GameSpellAndImmunityEffect) gsp).ImmunityState)
						continue; // ignore immunity effects
					return gsp;
				}
			}
			return null;
		}

		/// <summary>
		/// Find pulsing spell by spell handler
		/// </summary>
		/// <param name="living"></param>
		/// <param name="handler"></param>
		/// <returns>first occurance of spellhandler in targets' conc list or null</returns>
		public static PulsingSpellEffect FindPulsingSpellOnTarget(GameLiving living, ISpellHandler handler)
		{
			lock (living.ConcentrationEffects)
			{
				foreach (IConcentrationEffect concEffect in living.ConcentrationEffects)
				{
					PulsingSpellEffect pulsingSpell = concEffect as PulsingSpellEffect;
					if (pulsingSpell == null) continue;
					if (pulsingSpell.SpellHandler == handler)
						return pulsingSpell;
				}
				return null;
			}
		}

		#region various helpers

		/// <summary>
		/// Level mod for effect between target and caster if there is any
		/// </summary>
		/// <returns></returns>
		public virtual double GetLevelModFactor()
		{
			return 0.085; // 8.5% diff per level distance
		}

		/// <summary>
		/// Calculates min damage variance %
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="min">returns min variance</param>
		/// <param name="max">returns max variance</param>
		public virtual void CalculateDamageVariance(GameLiving target, out double min, out double max)
		{
			int speclevel = 1;
			if (m_caster is GamePlayer)
			{
				speclevel = ((GamePlayer) m_caster).GetModifiedSpecLevel(m_spellLine.Spec);
			}
			min = 1.25;
			max = 1.25;

			if (target.Level > 0)
			{
				min = 0.25 + (speclevel - 1)/(double) target.Level;
			}

			if (speclevel - 1 > target.Level)
			{
				double overspecBonus = (speclevel - 1 - target.Level)*0.005;
				min += overspecBonus;
				max += overspecBonus;
			}

			// add level mod
			min += GetLevelModFactor()*(m_caster.Level - target.Level);
			max += GetLevelModFactor()*(m_caster.Level - target.Level);
			if (max < 0.25)
				max = 0.25;
			if (min > max)
				min = max;
			if (min < 0)
				min = 0;
		}

		/// <summary>
		/// Calculates the base 100% spell damage which is then modified by damage variance factors
		/// </summary>
		/// <returns></returns>
		public virtual double CalculateDamageBase()
		{
			double spellDamage = Spell.Damage;
			GamePlayer player = Caster as GamePlayer;
			if (player != null && player.CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				int manaStatValue = player.GetModified((eProperty) player.CharacterClass.ManaStat);
				spellDamage *= (manaStatValue + 200) / 275.0;
				if (spellDamage < 0)
					spellDamage = 0;
			}
			return spellDamage;
		}

		/// <summary>
		/// Calculates the chance that the spell lands on target
		/// can be negative or above 100%
		/// </summary>
		/// <param name="target">spell target</param>
		/// <returns>chance that the spell lands on target</returns>
		public virtual int CalculateToHitChance(GameLivingBase target)
		{
			int hitchance = 85 + ((Spell.Level - target.Level) >> 1);
			if (!(m_caster is GamePlayer && target is GamePlayer))
			{
				// level mod
				hitchance -= (int) (m_caster.GetConLevel(target)*10);
			}
			return hitchance;
		}

		/// <summary>
		/// Calculates damage to target with resist chance and stores it in ad
		/// </summary>
		/// <param name="target">spell target</param>
		/// <returns>attack data</returns>
		public AttackData CalculateDamageToTarget(GameLiving target)
		{
			return CalculateDamageToTarget(target, 1);
		}

		/// <summary>
		/// Calculates damage to target with resist chance and stores it in ad
		/// </summary>
		/// <param name="target">spell target</param>
		/// <param name="effectiveness">value from 0..1 to modify damage</param>
		/// <returns>attack data</returns>
		public virtual AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
		{
			AttackData ad = new AttackData();
			ad.Attacker = m_caster;
			ad.Target = target;
			ad.AttackType = AttackData.eAttackType.Spell;

			double minVariance;
			double maxVariance;

			CalculateDamageVariance(target, out minVariance, out maxVariance);
			double spellDamage = CalculateDamageBase();
			if (Caster is GamePlayer)
				spellDamage *= ((GamePlayer)Caster).PlayerEffectiveness;

//			log.Info("100% damage="+spellDamage+"; minDamage="+(spellDamage*minVariance)+"; maxDamage="+(spellDamage*maxVariance));

			int finalDamage = Util.Random((int) (minVariance*spellDamage), (int) (maxVariance*spellDamage));

			int hitChance = CalculateToHitChance(ad.Target);
			if (hitChance < 55)
			{
				finalDamage += (int) (finalDamage*(hitChance - 55)*0.01);
				hitChance = 55;
			}
			else if (hitChance > 100)
			{
				finalDamage += (int) (finalDamage*(hitChance - 100)*0.01);
				hitChance = 100;
			}
			// apply effectiveness
			finalDamage = (int) (finalDamage*effectiveness);

			int resistModifier = 0;
			int cdamage = 0;
			if (finalDamage < 0)
				finalDamage = 0;

			resistModifier = finalDamage*ad.Target.GetResist(Spell.DamageType)/-100;
			finalDamage += resistModifier;
			
			// cap to +200% of base damage
			if (finalDamage > Spell.Damage*3)
			{
				finalDamage = (int) (Spell.Damage*3);
			}

			if (Util.Chance(m_caster.SpellCriticalChance) && (finalDamage >= 1))
			{
				int critmax = (ad.Target is GamePlayer) ? finalDamage/2 : finalDamage;
				cdamage = Util.Random(finalDamage/10, critmax); //think min crit is 10% of damage
			}

			ad.Damage = finalDamage;
			ad.CriticalDamage = cdamage;
			ad.DamageType = Spell.DamageType;
			ad.Modifier = resistModifier;

			return ad;
		}

		/// <summary>
		/// Sends damage text messages but makes no damage
		/// </summary>
		/// <param name="ad"></param>
		public virtual void SendDamageMessages(AttackData ad)
		{
			string modmessage = "";
			if (ad.Modifier > 0)
				modmessage = " (+" + ad.Modifier + ")";
			if (ad.Modifier < 0)
				modmessage = " (" + ad.Modifier + ")";

			MessageToCaster(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit);
			if (ad.CriticalDamage > 0)
				MessageToCaster("You critical hit for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit);
		}

		/// <summary>
		/// Make damage to target and send spell effect but no messages
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		public virtual void DamageTarget(AttackData ad, bool showEffectAnimation)
		{
			DamageTarget(ad, showEffectAnimation, 0x14); //spell damage attack result
		}

		/// <summary>
		/// Make damage to target and send spell effect but no messages
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="showEffectAnimation"></param>
		/// <param name="attackResult"></param>
		public virtual void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult)
		{
			ad.AttackResult = GameLiving.eAttackResult.HitUnstyled;
			if (showEffectAnimation)
			{
				SendEffectAnimation((GameLiving)ad.Target, 0, false, 1);
			}
			foreach (GamePlayer player in ad.Target.GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendCombatAnimation(ad.Attacker, ad.Target, 0, 0, 0, 0, (byte) attackResult, ad.Target.HealthPercent);
			}
			// send animation before dealing damage else dead livings show no animation
			ad.Target.OnAttackedByEnemy(ad, ad.Target);
			ad.Attacker.DealDamage(ad);
		}

		#endregion
	}
}