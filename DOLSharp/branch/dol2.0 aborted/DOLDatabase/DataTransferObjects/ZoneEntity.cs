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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public class ZoneEntity
	{
		private int m_id;
		private string m_description;
		private int m_region;
		private int m_xOffset;
		private int m_yOffset;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}
		public int Region
		{
			get { return m_region; }
			set { m_region = value; }
		}
		public int XOffset
		{
			get { return m_xOffset; }
			set { m_xOffset = value; }
		}
		public int YOffset
		{
			get { return m_yOffset; }
			set { m_yOffset = value; }
		}
	}
}
