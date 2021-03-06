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

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Resistance Property calculator
	/// 
	/// BuffBonusCategory1 is used for all capped buffs
	/// BuffBonusCategory2 external used for only damagemodifiing resists (1.65 Category2 Resists)
	/// BuffBonusCategory3 is used for all debuffs (positive values expected here)
	/// BuffBonusCategory4 is used for all uncapped modifications
	///                    category 4 kicks in at last
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.Resist_First, eProperty.Resist_Last)]
	public class ResistCalculator : PropertyCalculator
	{
		public ResistCalculator() { }

		public override int CalcValue(GameLiving living, eProperty property)
		{
			int intProperty = (int)property;
			int itemBonus = living.ItemBonus[intProperty];
			int abilityBonus = living.AbilityBonus[intProperty];
			int buffBonus = living.BuffBonusCategory1[intProperty];
			int debuff = living.BuffBonusCategory3[intProperty];
			if (debuff < 0)
			{
				debuff = -debuff;
			}
			int res = 0;

			if (living is GamePlayer)
			{
				GamePlayer player = (GamePlayer)living;
				res += SkillBase.GetRaceResist((eRace)player.Race, (eResist)property);
				int cap = living.Level / 2 + 1;

				if (itemBonus > cap)
				{
					itemBonus = cap;
				}

				if (buffBonus > cap)
				{
					buffBonus = cap;
				}
			}

			//100% debuff effectiveness for resists buffs
			buffBonus = buffBonus + living.BuffBonusCategory4[intProperty] - debuff;
			//50% debuff effectiveness for item and racial bonuses
			if (buffBonus < 0)
				buffBonus /= 2;

			return res + itemBonus + buffBonus + abilityBonus;
		}
	}
}
