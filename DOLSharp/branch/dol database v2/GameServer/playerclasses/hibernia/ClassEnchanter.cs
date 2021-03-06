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
using DOL.GS;

namespace DOL.GS.PlayerClass
{
	/// <summary>
	/// 
	/// </summary>
	[PlayerClassAttribute((int)eCharacterClass.Enchanter, "Enchanter", "Magician", "Enchantress")]
	public class ClassEnchanter : ClassMagician
	{
		public ClassEnchanter() : base() 
		{
			m_profession = "Path of Essence";
			m_specializationMultiplier = 10;
			m_primaryStat = eStat.INT;
			m_secondaryStat = eStat.DEX;
			m_tertiaryStat = eStat.QUI;
			m_manaStat = eStat.INT;
		}

		public override string GetTitle(int level) 
		{
			if (level>=50) return "Etheralist";
			if (level>=45) return "Possessor";
			if (level>=40) return "Enticer";
			if (level>=35) return "Phantom";
			if (level>=30) return "Mesmerizer";
			if (level>=25) return "Seductor";
			if (level>=20) return "Entrancer";
			if (level>=15) return "Glamourist";
			if (level>=10) return "Illusionist"; 
			if (level>=5) return "Charmer"; 
			return "None"; 
		}

		/// <summary>
		/// Update all skills and add new for current level
		/// </summary>
		/// <param name="player"></param>
		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);
			// Specializations
			player.AddSpecialization(SkillBase.GetSpecialization(Specs.Enchantments));
			
			// Spell lines
			player.AddSpellLine(SkillBase.GetSpellLine("Enchantment"));
			player.AddSpellLine(SkillBase.GetSpellLine("Empowering"));
			player.AddSpellLine(SkillBase.GetSpellLine("Bedazzling"));
			player.AddSpellLine(SkillBase.GetSpellLine("Enchantment Mastery"));

			if (player.Level >= 5) 
			{
				player.AddAbility(SkillBase.GetAbility(Abilities.Quickcast));
			}
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
