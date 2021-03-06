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
	public class MerchantPageDao : IMerchantPageDao
	{
		protected static readonly string c_rowFields = "`MerchantPageId`,`Currency`,`MerchantWindowId`,`Position`";
		protected readonly MySqlState m_state;

		public virtual MerchantPageEntity Find(int id)
		{
			MerchantPageEntity result = new MerchantPageEntity();
			string command = "SELECT " + c_rowFields + " FROM `merchantpage` WHERE `MerchantPageId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref MerchantPageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `merchantpage` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Currency.ToString()) + "','" + m_state.EscapeString(obj.MerchantWindow.ToString()) + "','" + m_state.EscapeString(obj.Position.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(MerchantPageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `merchantpage` SET `MerchantPageId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Currency`='" + m_state.EscapeString(obj.Currency.ToString()) + "', `MerchantWindowId`='" + m_state.EscapeString(obj.MerchantWindow.ToString()) + "', `Position`='" + m_state.EscapeString(obj.Position.ToString()) + "' WHERE `MerchantPageId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(MerchantPageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `merchantpage` WHERE `MerchantPageId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<MerchantPageEntity> SelectAll()
		{
			MerchantPageEntity entity;
			List<MerchantPageEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `merchantpage`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<MerchantPageEntity>();
					while (reader.Read())
					{
						entity = new MerchantPageEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `merchantpage`");
		}

		protected virtual void FillEntityWithRow(ref MerchantPageEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Currency = reader.GetByte(1);
			entity.MerchantWindow = reader.GetInt32(2);
			entity.Position = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(MerchantPageEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `merchantpage` ("
				+"`MerchantPageId` int NOT NULL auto_increment,"
				+"`Currency` tinyint unsigned NOT NULL,"
				+"`MerchantWindowId` int,"
				+"`Position` int NOT NULL"
				+", primary key `MerchantPageId` (`MerchantPageId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `merchantpage`");
			return null;
		}

		public MerchantPageDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
