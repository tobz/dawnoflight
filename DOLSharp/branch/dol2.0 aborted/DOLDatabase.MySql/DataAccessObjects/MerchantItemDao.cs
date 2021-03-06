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
using System.Collections.Generic;
using System.Data;
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;
using MySql.Data.MySqlClient;

namespace DOL.Database.MySql.DataAccessObjects
{
	public class MerchantItemDao : IMerchantItemDao
	{
		protected static readonly string c_rowFields = "`MerchantItemId`,`ItemTemplateId`,`MerchantPageId`,`Position`";
		protected readonly MySqlState m_state;

		public virtual MerchantItemEntity Find(int id)
		{
			MerchantItemEntity result = new MerchantItemEntity();
			string command = "SELECT " + c_rowFields + " FROM `merchantitem` WHERE `MerchantItemId`='" + m_state.EscapeString(id.ToString()) + "'";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						result = null;
					}
					else
					{
						FillEntityWithRow(ref result, reader);
					}
				}
			);

			return result;
		}

		public virtual void Create(ref MerchantItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `merchantitem` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "','" + m_state.EscapeString(obj.MerchantPage.ToString()) + "','" + m_state.EscapeString(obj.Position.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(MerchantItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `merchantitem` SET `MerchantItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `ItemTemplateId`='" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "', `MerchantPageId`='" + m_state.EscapeString(obj.MerchantPage.ToString()) + "', `Position`='" + m_state.EscapeString(obj.Position.ToString()) + "' WHERE `MerchantItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(MerchantItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `merchantitem` WHERE `MerchantItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<MerchantItemEntity> SelectAll()
		{
			MerchantItemEntity entity;
			List<MerchantItemEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `merchantitem`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<MerchantItemEntity>();
					while (reader.Read())
					{
						entity = new MerchantItemEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `merchantitem`");
		}

		protected virtual void FillEntityWithRow(ref MerchantItemEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.ItemTemplate = reader.GetString(1);
			entity.MerchantPage = reader.GetInt32(2);
			entity.Position = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(MerchantItemEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `merchantitem` ("
				+"`MerchantItemId` int NOT NULL auto_increment,"
				+"`ItemTemplateId` char(255) character set latin1,"
				+"`MerchantPageId` int,"
				+"`Position` int NOT NULL"
				+", primary key `MerchantItemId` (`MerchantItemId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `merchantitem`");
			return null;
		}

		public MerchantItemDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
