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
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;
using DOL.GS.Scripts;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Uninterruptable")]
	public class UninterruptableSpellHandler : PrimerSpellHandler
	{
		/// <summary>
		/// Calculates the power to cast the spell
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateNeededPower(GameLiving target)
		{
			double power = (Caster.MaxMana*m_spell.Value);
			return (int) power;
		}

		public override bool CasterIsAttacked(GameLiving attacker)
		{
			return false;
		}

		// constructor
		public UninterruptableSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
