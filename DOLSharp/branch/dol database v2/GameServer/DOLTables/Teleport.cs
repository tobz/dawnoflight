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


namespace DOL.Database2
{
	/// <summary>
	/// Teleport location table.
	/// </summary>
	/// <author>Aredhel</author>
	[Serializable]//TableName = "Teleport")]
	public class Teleport : DatabaseObject
	{
		private bool m_autoSave;
		private String m_teleportID;
		private int m_realm;
		private int m_regionID;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;

		/// <summary>
		/// Create a new teleport location.
		/// </summary>
        public Teleport()
            : base()
		{
			m_autoSave = false;
			m_teleportID = "UNDEFINED";
			m_realm = 0;
			m_regionID = 0;
			m_x = 0;
			m_y = 0;
			m_z = 0;
			m_heading = 0;
		}


		/// <summary>
		/// ID for this teleport location.
		/// </summary>
		
		public String TeleportID
		{
			get { return m_teleportID; }
			set	{ m_teleportID = value;	}
		}

		/// <summary>
		/// Realm for this teleport location.
		/// </summary>
		
		public int Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}

		/// <summary>
		/// Realm for this teleport location.
		/// </summary>
		
		public int RegionID
		{
			get { return m_regionID; }
			set { m_regionID = value; }
		}

		/// <summary>
		/// X coordinate for teleport location.
		/// </summary>
		
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Y coordinate for teleport location.
		/// </summary>
		
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		/// <summary>
		/// Z coordinate for teleport location.
		/// </summary>
		
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

		/// <summary>
		/// Heading for teleport location.
		/// </summary>
		
		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
	}
}