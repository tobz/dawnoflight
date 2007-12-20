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
	[PlayerClassAttribute((int)eCharacterClass.Mystic, "Mystic", "Mystic")]
	public class ClassMystic : DOL.GS.CharacterClassSpec
	{
		public ClassMystic()
		{
			m_specializationMultiplier = 10;
			m_wsbase = 280;
			m_baseHP = 560;
			m_manaStat = eStat.INT;
		}

		public override eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		public override void OnLevelUp(GamePlayer player) 
		{
			base.OnLevelUp(player);

			if (this is ClassWarlock == false)
			{
				// Specializations
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Darkness));
				player.AddSpecialization(SkillBase.GetSpecialization(Specs.Suppression));

				// Spell lines
				player.AddSpellLine(SkillBase.GetSpellLine("Darkness"));
				player.AddSpellLine(SkillBase.GetSpellLine("Suppression"));
			}

			// Abilities
			player.AddAbility(SkillBase.GetAbility(Abilities.Sprint));
			player.AddAbility(SkillBase.GetAbility(Abilities.MidArmor, ArmorLevel.Cloth));
			player.AddAbility(SkillBase.GetAbility(Abilities.Weapon_Staves));
		}
	}
}
