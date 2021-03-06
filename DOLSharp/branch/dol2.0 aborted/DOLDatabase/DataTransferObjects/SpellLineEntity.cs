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
	public class SpellLineEntity
	{
		private string m_keyName;
		private bool m_isBaseLine;
		private string m_name;
		private string m_spec;

		public string KeyName
		{
			get { return m_keyName; }
			set { m_keyName = value; }
		}
		public bool IsBaseLine
		{
			get { return m_isBaseLine; }
			set { m_isBaseLine = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public string Spec
		{
			get { return m_spec; }
			set { m_spec = value; }
		}
	}
}
