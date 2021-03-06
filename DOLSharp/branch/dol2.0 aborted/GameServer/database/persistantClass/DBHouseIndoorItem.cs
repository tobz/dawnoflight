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

namespace DOL.GS.Database
{
	public class DBHouseIndoorItem
	{
		private int m_id;	
		//important data
		private int m_housenumber;
		private int m_model;
		private int m_position;
		private int m_placemode;
		private int m_xpos;
		private int m_ypos;
		private string m_baseitemid;
		//"can-be-null" data (well, i dont know if size can be 0)
		private int m_color;
		private int m_rotation;
		private int m_size;

		public int HouseIndoorItemID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}
		
		public int HouseNumber
		{
			get
			{
				return m_housenumber;
			}
			set
			{
				m_housenumber = value;
			}
		}
		
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{
				m_model = value;
			}
		}
		
		public int Position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
			}
		}
		
		public int Placemode
		{
			get
			{
				return m_placemode;
			}
			set
			{
				m_placemode = value;
			}
		}
		
		public int X
		{
			get
			{
				return m_xpos;
			}
			set
			{
				m_xpos = value;
			}
		}
		
		public int Y
		{
			get
			{
				return m_ypos;
			}
			set
			{
				m_ypos = value;
			}
		}
		
		public string BaseItemID
		{
			get
			{
				return m_baseitemid;
			}
			set
			{
				m_baseitemid = value;
			}
		}
		
		public int Color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
			}
		}
		
		public int Rotation
		{
			get
			{
				return m_rotation;
			}
			set
			{
				m_rotation = value;
			}
		}

		public int Size
		{
			get
			{
				return m_size;
			}
			set
			{
				m_size = value;
			}
		}
	}
}
