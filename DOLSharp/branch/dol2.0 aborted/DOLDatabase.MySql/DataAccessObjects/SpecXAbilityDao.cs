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
	public class SpecXAbilityDao : ISpecXAbilityDao
	{
		protected static readonly string c_rowFields = "`SpecXAbilityId`,`AbilityKey`,`AbilityLevel`,`Spec`,`SpecLevel`";
		protected readonly MySqlState m_state;

		public virtual SpecXAbilityEntity Find(int id)
		{
			SpecXAbilityEntity result = new SpecXAbilityEntity();
			string command = "SELECT " + c_rowFields + " FROM `specxability` WHERE `SpecXAbilityId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref SpecXAbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `specxability` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AbilityKey.ToString()) + "','" + m_state.EscapeString(obj.AbilityLevel.ToString()) + "','" + m_state.EscapeString(obj.Spec.ToString()) + "','" + m_state.EscapeString(obj.SpecLevel.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(SpecXAbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `specxability` SET `SpecXAbilityId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AbilityKey`='" + m_state.EscapeString(obj.AbilityKey.ToString()) + "', `AbilityLevel`='" + m_state.EscapeString(obj.AbilityLevel.ToString()) + "', `Spec`='" + m_state.EscapeString(obj.Spec.ToString()) + "', `SpecLevel`='" + m_state.EscapeString(obj.SpecLevel.ToString()) + "' WHERE `SpecXAbilityId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(SpecXAbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `specxability` WHERE `SpecXAbilityId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<SpecXAbilityEntity> SelectAll()
		{
			SpecXAbilityEntity entity;
			List<SpecXAbilityEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `specxability`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<SpecXAbilityEntity>();
					while (reader.Read())
					{
						entity = new SpecXAbilityEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `specxability`");
		}

		protected virtual void FillEntityWithRow(ref SpecXAbilityEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AbilityKey = reader.GetString(1);
			entity.AbilityLevel = reader.GetInt32(2);
			entity.Spec = reader.GetString(3);
			entity.SpecLevel = reader.GetInt32(4);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpecXAbilityEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `specxability` ("
				+"`SpecXAbilityId` int NOT NULL auto_increment,"
				+"`AbilityKey` char(255) character set latin1 NOT NULL,"
				+"`AbilityLevel` int NOT NULL,"
				+"`Spec` char(255) character set latin1 NOT NULL,"
				+"`SpecLevel` int NOT NULL"
				+", primary key `SpecXAbilityId` (`SpecXAbilityId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `specxability`");
			return null;
		}

		public SpecXAbilityDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
