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
	public class GameNpcInventoryDao : IGameNpcInventoryDao
	{
		protected static readonly string c_rowFields = "`InventoryId`,`IsCloakHoodUp`";
		protected readonly MySqlState m_state;

		public virtual GameNpcInventoryEntity Find(int id)
		{
			GameNpcInventoryEntity result = new GameNpcInventoryEntity();
			string command = "SELECT " + c_rowFields + " FROM `gamenpcinventory` WHERE `InventoryId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `gamenpcinventory` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.IsCloakHoodUp.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `gamenpcinventory` SET `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `IsCloakHoodUp`='" + m_state.EscapeString(obj.IsCloakHoodUp.ToString()) + "' WHERE `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `gamenpcinventory` WHERE `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<GameNpcInventoryEntity> SelectAll()
		{
			GameNpcInventoryEntity entity;
			List<GameNpcInventoryEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `gamenpcinventory`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GameNpcInventoryEntity>();
					while (reader.Read())
					{
						entity = new GameNpcInventoryEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `gamenpcinventory`");
		}

		protected virtual void FillEntityWithRow(ref GameNpcInventoryEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.IsCloakHoodUp = reader.GetBoolean(1);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GameNpcInventoryEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gamenpcinventory` ("
				+"`InventoryId` int NOT NULL auto_increment,"
				+"`IsCloakHoodUp` bit NOT NULL"
				+", primary key `InventoryId` (`InventoryId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `gamenpcinventory`");
			return null;
		}

		public GameNpcInventoryDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
