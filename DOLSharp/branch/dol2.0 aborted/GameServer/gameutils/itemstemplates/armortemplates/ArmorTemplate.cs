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
using System.Collections;
using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a ArmorTemplate
	/// </summary> 
	public abstract class ArmorTemplate : VisibleEquipmentTemplate
	{
		#region Declaraction
		/// <summary>
		/// The armor factor of this template
		/// </summary>
		protected byte m_armorFactor;

		/// <summary>
		/// The armor level of this template
		/// </summary>
		protected eArmorLevel m_armorLevel;

		/// <summary>
		/// The model extension of this template (it is often the relief level of the item)
		/// </summary>
		protected byte m_modelExtension;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the armor factor of this template
		/// </summary>
		public byte ArmorFactor
		{
			get { return m_armorFactor; }
			set	{ m_armorFactor = value; }
		}

		/// <summary>
		/// Gets or sets the armor level of this template
		/// </summary>
		public eArmorLevel ArmorLevel
		{
			get { return m_armorLevel; }
			set	{ m_armorLevel = value; }
		}

		/// <summary>
		/// Gets or sets the model extension of this template (it is often the relief level of the item)
		/// </summary>
		public byte ModelExtension
		{
			get { return m_modelExtension; }
			set	{ m_modelExtension = value; }
		}

		#endregion

		/// <summary>
		/// Gets the object type of the template (for test use class type instead of this propriety)
		/// </summary>
		public override eObjectType ObjectType
		{
			get
			{
				switch(ArmorLevel)
				{
					case eArmorLevel.VeryLow :	
							return eObjectType.Cloth;
				
					case eArmorLevel.Low :	
							return eObjectType.Leather;
					
					case eArmorLevel.Medium :	
							if(Realm == eRealm.Hibernia)	return eObjectType.Reinforced;
							else							return eObjectType.Studded;
					
					case eArmorLevel.High :
							if(Realm == eRealm.Hibernia)	return eObjectType.Scale;
							else							return eObjectType.Chain;
							
					case eArmorLevel.VeryHigh : 
							return eObjectType.Plate;
				
					default : 
						return eObjectType.GenericArmor;
				}
			}
		}

		/// <summary>
		/// Gets the armor abs percent
		/// </summary>
		public virtual byte Absorbtion
		{
			get
			{
				switch(m_armorLevel)
				{
					case eArmorLevel.Low		: return 10; //leather (10% abs)
					case eArmorLevel.Medium		: return 19; //studded leather / reinforced  (19 % abs)
					case eArmorLevel.High		: return 27; //Chain / Scale (27% abs)
					case eArmorLevel.VeryHigh	: return 34; //Plate (34% abs)
					default						: return 0;  //cloth (0% abs) 
				}
			}
		}

		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList GetDelveInfo(GamePlayer player)
		{
			ArrayList list = (ArrayList) base.GetDelveInfo(player);

			list.Add(" ");
			list.Add(" ");
			list.Add("Armor Modifiers:");
			list.Add("- " + ArmorFactor + " Base Factor");
			
			double clampedAF = Math.Min((ArmorLevel == eArmorLevel.VeryLow ? 2 * player.Level : player.Level), ArmorFactor);
			
			list.Add("- " + (int)clampedAF + " Clamped Factor");
			list.Add("- " + Absorbtion + "% Absorption");
			list.Add("- " + Quality + "% Quality");
			list.Add("- " + (byte)Condition + "% Condition");
			list.Add(" ");
			list.Add("Effective Armor:");

			double EffecAF = clampedAF * Quality/100.0 * Condition/100 * (1 + Absorbtion/100.0 );

			list.Add("- " + (int)EffecAF + " Factor");
			
			return list;
		}
	}
}	