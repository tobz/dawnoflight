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
	public class CraftItemDataDao : ICraftItemDataDao
	{
		protected static readonly string c_rowFields = "`CraftItemDataId`,`CraftingLevel`,`CraftingSkill`,`TemplateToCraft`";
		protected readonly MySqlState m_state;

		public virtual CraftItemDataEntity Find(int id)
		{
			CraftItemDataEntity result = new CraftItemDataEntity();
			string command = "SELECT " + c_rowFields + " FROM `craftitemdata` WHERE `CraftItemDataId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref CraftItemDataEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `craftitemdata` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.CraftingLevel.ToString()) + "','" + m_state.EscapeString(obj.CraftingSkill.ToString()) + "','" + m_state.EscapeString(obj.TemplateToCraft.ToString()) + "');");
		}

		public virtual void Update(CraftItemDataEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `craftitemdata` SET `CraftItemDataId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `CraftingLevel`='" + m_state.EscapeString(obj.CraftingLevel.ToString()) + "', `CraftingSkill`='" + m_state.EscapeString(obj.CraftingSkill.ToString()) + "', `TemplateToCraft`='" + m_state.EscapeString(obj.TemplateToCraft.ToString()) + "' WHERE `CraftItemDataId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(CraftItemDataEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `craftitemdata` WHERE `CraftItemDataId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<CraftItemDataEntity> SelectAll()
		{
			CraftItemDataEntity entity;
			List<CraftItemDataEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `craftitemdata`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<CraftItemDataEntity>();
					while (reader.Read())
					{
						entity = new CraftItemDataEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `craftitemdata`");
		}

		protected virtual void FillEntityWithRow(ref CraftItemDataEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.CraftingLevel = reader.GetInt32(1);
			entity.CraftingSkill = reader.GetInt32(2);
			entity.TemplateToCraft = reader.GetString(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(CraftItemDataEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `craftitemdata` ("
				+"`CraftItemDataId` int NOT NULL,"
				+"`CraftingLevel` int NOT NULL,"
				+"`CraftingSkill` int NOT NULL,"
				+"`TemplateToCraft` char(255) character set latin1 NOT NULL"
				+", primary key `CraftItemDataId` (`CraftItemDataId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `craftitemdata`");
			return null;
		}

		public CraftItemDataDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
