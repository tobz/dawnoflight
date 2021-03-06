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
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	public enum eCraftingToolType : byte
	{
		SmithHammer		= 0,
		SewingKit		= 1,
		PlaningTool		= 2,
		AlchemyKit		= 3,
		MortarAndPestle	= 4,
		SpellcraftKit	= 5,
		Forge			= 6,
		Lathe			= 7,
		AlchemyTable	= 8
	}

	/// <summary>
	/// Summary description for a CraftingToolTemplate
	/// </summary> 
	public class CraftingToolTemplate : GenericItemTemplate
	{
		#region Declaration
		/// <summary>
		/// The type of the crafting tool
		/// </summary>
		protected eCraftingToolType m_type;
		
		#endregion

		#region All get and set
		/// <summary>
		/// Gets or sets the type of this crafting tool
		/// </summary>
		public eCraftingToolType Type
		{
			get { return m_type; }
			set	{ m_type = value; }
		}
		#endregion

		/// <summary>
		/// Create a object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			CraftingTool item = new CraftingTool();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight;
			item.Value = m_value;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.QuestName = "";
			item.CrafterName = "";
			item.Type = m_type;
			return item;
		}
	}
}
