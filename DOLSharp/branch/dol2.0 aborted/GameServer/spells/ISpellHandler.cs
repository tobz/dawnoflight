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
using DOL.GS.Effects;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISpellHandler
	{
		/// <summary>
		/// Called when a spell is casted
		/// </summary>
		void CastSpell();

		/// <summary>
		/// Starts the spell, without displaying cast message etc.
		/// Should be used for StyleEffects, ...
		/// </summary>
		void StartSpell(GameLivingBase target);

		/// <summary>
		/// Whenever the current casting sequence is to be interrupted
		/// this callback is called
		/// </summary>
		void InterruptCasting();

		/// <summary>
		/// Has to be called when the caster moves
		/// </summary>
		void CasterMoves();

		/// <summary>
		/// Has to be called when the caster is attacked by enemy
		/// for interrupt checks
		/// <param name="attacker">attacker that interrupts the cast sequence</param>
		/// <returns>true if casting was interrupted</returns>
		/// </summary>
		bool CasterIsAttacked(GameLiving attacker);

		/// <summary>
		/// Returns true when spell is in casting phase
		/// </summary>
		bool IsCasting { get; }

		/// <summary>
		/// Gets wether this spell has positive or negative impact on targets
		/// important to determine wether the spell can be canceled by a player
		/// </summary>
		/// <returns></returns>
		bool HasPositiveEffect { get; }

		/// <summary>
		/// Determines wether new spell is better than existing one
		/// important for overwriting
		/// </summary>
		/// <param name="oldeffect"></param>
		/// <param name="neweffect"></param>
		/// <returns>true if new spell is better version</returns>
		bool IsNewEffectBetter(GameSpellEffect oldeffect, GameSpellEffect neweffect);

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable do not stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		bool IsOverwritable(GameSpellEffect compare);

		void OnEffectStart(GameSpellEffect effect);
		void OnEffectPulse(GameSpellEffect effect);

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		int OnEffectExpires(GameSpellEffect effect, bool noMessages);

		/// <summary>
		/// When spell pulses
		/// </summary>
		/// <param name="effect">The effect doing the pulses</param>
		void OnSpellPulse(PulsingSpellEffect effect);

		GameLiving Caster { get; }
		Spell Spell { get; }
		SpellLine SpellLine { get; }
		IList DelveInfo{ get; }

		/// <summary>
		/// Event raised when casting sequence is completed and execution of spell can start
		/// </summary>
		event CastingCompleteCallback CastingCompleteEvent;

		/// <summary>
		/// Event raised when spell is done or canceled
		/// </summary>
		//event SpellEndsCallback SpellEndsEvent;
	}

	/// <summary>
	/// Callback when spell handler has done its cast work
	/// </summary>
	public delegate void CastingCompleteCallback(ISpellHandler handler);

	/// <summary>
	/// Callback when spell handler is completely done and duration spell expired
	/// or concentration spell was canceled
	/// </summary>
	public delegate void SpellEndsCallback(ISpellHandler handler);
}
