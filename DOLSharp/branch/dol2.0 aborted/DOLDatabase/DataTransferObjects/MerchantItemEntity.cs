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
	public class MerchantItemEntity
	{
		private int m_id;
		private string m_itemTemplate;
		private int m_merchantPage;
		private int m_position;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string ItemTemplate
		{
			get { return m_itemTemplate; }
			set { m_itemTemplate = value; }
		}
		public int MerchantPage
		{
			get { return m_merchantPage; }
			set { m_merchantPage = value; }
		}
		public int Position
		{
			get { return m_position; }
			set { m_position = value; }
		}
	}
}
