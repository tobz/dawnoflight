using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;
using DOL.GS.Effects;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Effect handler for Barrier Of Fortitude
	/// </summary>
	public class BedazzlingAuraEffect : TimedEffect, IGameEffect
	{
		private int m_value;

		/// <summary>
		/// Default constructor for AmelioratingMelodiesEffect
		/// </summary>
		public BedazzlingAuraEffect()
			: base(30000)
		{

		}

		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="player">The player to start the effect for</param>
		/// <param name="duration">The effectduration in secounds</param>
		/// <param name="value">The percentage additional value for all magic resis</param>
		public void Start(GameLiving living, int value)
		{
			m_value = value;

			if (living.TempProperties.getProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, false))
				return;

			base.Start(living);

			living.AbilityBonus[(int)eProperty.Resist_Body] += m_value;
			living.AbilityBonus[(int)eProperty.Resist_Cold] += m_value;
			living.AbilityBonus[(int)eProperty.Resist_Energy] += m_value;
			living.AbilityBonus[(int)eProperty.Resist_Heat] += m_value;
			living.AbilityBonus[(int)eProperty.Resist_Matter] += m_value;
			living.AbilityBonus[(int)eProperty.Resist_Spirit] += m_value;

			if (living is GamePlayer)
			{
				GameEventMgr.AddHandler(living, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
				(living as GamePlayer).Out.SendCharResistsUpdate();
			}
			living.TempProperties.setProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb, true);
		}

		/// <summary>
		/// Called when a player leaves the game
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		private static void PlayerLeftWorld(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;

			BarrierOfFortitudeEffect BoFEffect = (BarrierOfFortitudeEffect)player.EffectList.GetOfType(typeof(BarrierOfFortitudeEffect));
			if (BoFEffect != null)
			{
				BoFEffect.Cancel(false);
			}
		}

		public override void Stop()
		{
			base.Stop();

			m_owner.AbilityBonus[(int)eProperty.Resist_Body] -= m_value;
			m_owner.AbilityBonus[(int)eProperty.Resist_Cold] -= m_value;
			m_owner.AbilityBonus[(int)eProperty.Resist_Energy] -= m_value;
			m_owner.AbilityBonus[(int)eProperty.Resist_Heat] -= m_value;
			m_owner.AbilityBonus[(int)eProperty.Resist_Matter] -= m_value;
			m_owner.AbilityBonus[(int)eProperty.Resist_Spirit] -= m_value;
			if (m_owner is GamePlayer)
			{
				(m_owner as GamePlayer).Out.SendCharResistsUpdate();
				GameEventMgr.RemoveHandler(m_owner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerLeftWorld));
			}
			m_owner.TempProperties.removeProperty(RealmAbilities.BarrierOfFortitudeAbility.BofBaSb);
		}


		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return "Bedazzeling Aura";
			}
		}

		/// <summary>
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3029;
			}
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override IList DelveInfo
		{
			get
			{
				IList delveInfoList = new ArrayList(10);
				delveInfoList.Add("Grants the group increased resistance to magical damage (Does not stack with Soldier's Barricade or Barrier of Fortitude).");
				delveInfoList.Add(" ");
				delveInfoList.Add("Value: " + m_value + "%");

				int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList;
			}
		}
	}
}
