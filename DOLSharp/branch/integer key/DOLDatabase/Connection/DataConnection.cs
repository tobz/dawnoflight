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
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Text;
using log4net;
using MySql.Data.MySqlClient;

namespace DOL.Database.Connection
{
	/// <summary>
	/// Called after mysql query.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public delegate void QueryCallback(MySqlDataReader reader);

	/// <summary>
	/// Class for Handling the Connection to the ADO.Net Layer of the Databases.
	/// Funktions for loading and storing the complete Dataset are in there.
	/// </summary>
	public class DataConnection
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string connString;
		private ConnectionType connType;

		/// <summary>
		/// Constructor to set up a Database
		/// </summary>
		/// <param name="connType">Connection-Type the Database should use</param>
		/// <param name="connString">Connection-String to indicate the Parameters of the Datasource.
		///     MYSQL = ADO.NET ConnectionString 
		/// </param>
		public DataConnection(ConnectionType connType, string connString)
		{
			this.connType = connType;
			this.connString = connString;
		}

		/// <summary>
		/// The connection type to DB (xml, mysql,...)
		/// </summary>
		public ConnectionType ConnectionType
		{
			get { return connType; }
		}

		/// <summary>
		/// escape the strange character from string
		/// </summary>
		/// <param name="s">the string</param>
		/// <returns>the string with escaped character</returns>
		public string Escape(string s)
		{
			s = s.Replace("\\", "\\\\");
			s = s.Replace("\"", "\\\"");
			s = s.Replace("'", "\\'");
			s = s.Replace("�", "\\�");

			return s;
		}

		private readonly Queue<MySqlConnection> m_connectionPool = new Queue<MySqlConnection>();

		/// <summary>
		/// Gets connection from connection pool.
		/// </summary>
		/// <param name="isNewConnection">Set to <code>true</code> if new connection is created.</param>
		/// <returns>Connection.</returns>
		private MySqlConnection GetMySqlConnection(out bool isNewConnection)
		{
			// Get connection from pool
			MySqlConnection conn = null;
			lock (m_connectionPool)
			{
				if (m_connectionPool.Count > 0)
				{
					conn = m_connectionPool.Dequeue();
				}
			}

			if (conn != null)
			{
				isNewConnection = false;
			}
			else
			{
				isNewConnection = true;
				long start1 = Environment.TickCount;
				conn = new MySqlConnection(connString);
				conn.Open();
				if (Environment.TickCount - start1 > 1000)
				{
					if (log.IsWarnEnabled)
						log.Warn("Gaining SQL connection took " + (Environment.TickCount - start1) + "ms");
				}
				log.Info("New DB connection created");
			}
			return conn;
		}

		/// <summary>
		/// Releases the connection to connection pool.
		/// </summary>
		/// <param name="conn">The connection to relase.</param>
		private void ReleaseConnection(MySqlConnection conn)
		{
			lock (m_connectionPool)
			{
				m_connectionPool.Enqueue(conn);
			}
		}

		/// <summary>
		/// Execute a non query like update or delete
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns>number of rows affected</returns>
		public int ExecuteNonQuery(string sqlcommand)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("SQL: " + sqlcommand);
			}

			int affected = 0;
			bool repeat = false;
			do
			{
				bool isNewConnection;
				MySqlConnection conn = GetMySqlConnection(out isNewConnection);
				MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);

				try
				{
					long start = Environment.TickCount;
					affected = cmd.ExecuteNonQuery();

					if (log.IsDebugEnabled)
						log.Debug("SQL NonQuery exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL NonQuery took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand);

					ReleaseConnection(conn);

					repeat = false;
				}
				catch (Exception e)
				{
					conn.Close();

					if (!HandleException(e) || isNewConnection)
					{
						throw;
					}
					repeat = true;
				}
			} while (repeat);

			return affected;
		}

		/// <summary>
		/// Execute a non query like update or delete
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns>number of rows affected</returns>
		public int ExecuteNonQuery(string sqlcommand, out uint insertId)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("SQL: " + sqlcommand);
			}

			int affected = 0;
			bool repeat = false;
			do
			{
				bool isNewConnection;
				MySqlConnection conn = GetMySqlConnection(out isNewConnection);
				MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);
				MySqlCommand lastId = new MySqlCommand("SELECT LAST_INSERT_ID();", conn);

				try
				{
					long start = Environment.TickCount;
					affected = cmd.ExecuteNonQuery();
					insertId = (uint)(long)lastId.ExecuteScalar();
					//log.Debug("INSERTED ID = " + insertId);
					if (log.IsDebugEnabled)
						log.Debug("SQL NonQuery exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL NonQuery took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand);

					ReleaseConnection(conn);

					repeat = false;
				}
				catch (Exception e)
				{
					conn.Close();
					insertId = 0;
					if (!HandleException(e) || isNewConnection)
					{
						throw;
					}
					repeat = true;
				}
			} while (repeat);

			return affected;
		}

		/// <summary>
		/// Handles the exception.
		/// </summary>
		/// <param name="e">The exception.</param>
		/// <returns><code>true</code> if operation should be repeated, <code>false</code> otherwise.</returns>
		private static bool HandleException(Exception e)
		{
			bool ret = false;
			SocketException socketException = e.InnerException == null ? null : e.InnerException.InnerException as SocketException;
			if (socketException == null)
			{
				socketException = e.InnerException as SocketException;
			}

			if (socketException != null)
			{
				// Handle socket exception. Error codes:
				// http://msdn2.microsoft.com/en-us/library/ms740668.aspx
				// 10052 = Network dropped connection on reset.
				// 10053 = Software caused connection abort.
				// 10054 = Connection reset by peer.
				// 10057 = Socket is not connected.
				// 10058 = Cannot send after socket shutdown.
				switch (socketException.ErrorCode)
				{
					case 10052:
					case 10053:
					case 10054:
					case 10057:
					case 10058:
						{
							ret = true;
							break;
						}
				}
				log.WarnFormat("Socket exception: ({0}) {1}; repeat: {2}", socketException.ErrorCode, socketException.Message, ret);
			}
			return ret;
		}

		/// <summary>
		/// Execute select on sql database
		/// Close returned datareader when done or use using(reader) { ... }
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		public void ExecuteSelect(string sqlcommand, QueryCallback callback)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("SQL: " + sqlcommand);
			}
			bool repeat = false;
			MySqlConnection conn = null;
			do
			{
				bool isNewConnection = true;
				try
				{
					conn = GetMySqlConnection(out isNewConnection);

					long start = Environment.TickCount;

					MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);
					MySqlDataReader reader = cmd.ExecuteReader( /*CommandBehavior.CloseConnection*/);
					callback(reader);

					reader.Close();

					if (log.IsDebugEnabled)
						log.Debug("SQL Select exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL Select took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand);

					ReleaseConnection(conn);

					repeat = false;
				}
				catch (Exception e)
				{
					if (conn != null)
					{
						conn.Close();
					}
					if (!HandleException(e) || isNewConnection)
					{
						if (!sqlcommand.Contains("DESCRIBE"))
						{
							if (log.IsErrorEnabled)
								log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
						}
						//if (log.IsErrorEnabled)
						//    log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
						throw;
					}
					repeat = true;
				}
			} while (repeat);

			return;
		}

		/// <summary>
		/// Execute scalar on sql database
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		public object ExecuteScalar(string sqlcommand)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("SQL: " + sqlcommand);
			}

			object obj = null;
			bool repeat = false;
			MySqlConnection conn = null;
			do
			{
				bool isNewConnection = true;
				try
				{
					conn = GetMySqlConnection(out isNewConnection);
					MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);

					long start = Environment.TickCount;
					obj = cmd.ExecuteScalar();

					ReleaseConnection(conn);

					if (log.IsDebugEnabled)
						log.Debug("SQL Select exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL Select took " + (Environment.TickCount - start) + "ms!\n" + sqlcommand);

					repeat = false;
				}
				catch (Exception e)
				{
					if (conn != null)
					{
						conn.Close();
					}
					if (!HandleException(e) || isNewConnection)
					{
						if (log.IsErrorEnabled)
							log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
						throw;
					}
					repeat = true;
				}
			} while (repeat);

			return obj;
		}

		/// <summary>
		/// Create the table in mysql
		/// </summary>
		/// <param name="table">the table to create</param>
#warning This do not provide the optimized mysql field value
		public void CheckOrCreateTable(DataTable table)
		{
			ArrayList currentTableColumns = new ArrayList();
			try
			{
				ExecuteSelect("DESCRIBE `" + table.TableName + "`", delegate(MySqlDataReader reader)
				{
					while (reader.Read())
					{
						currentTableColumns.Add(reader.GetString(0).ToLower());
						log.Debug(reader.GetString(0).ToLower());
					}
					if (log.IsDebugEnabled)
						log.Debug(currentTableColumns.Count + " in table");
				});
			}
			catch (Exception e)
			{
				//if (log.IsDebugEnabled)
				//    log.Debug(e.ToString());

				if (log.IsWarnEnabled)
					log.Warn("Table " + table.TableName + " doesn't exist, creating it...");
			}

			StringBuilder sb = new StringBuilder();
			Hashtable primaryKeys = new Hashtable();
			for (int i = 0; i < table.PrimaryKey.Length; i++)
			{
				primaryKeys[table.PrimaryKey[i].ColumnName] = table.PrimaryKey[i];
			}

			ArrayList columnDefs = new ArrayList();
			ArrayList alterAddColumnDefs = new ArrayList();
			for (int i = 0; i < table.Columns.Count; i++)
			{
				Type systype = table.Columns[i].DataType;

				string column = "";

				column += "`" + table.Columns[i].ColumnName + "` ";
				if (systype == typeof(System.Char))
				{
					column += "SMALLINT UNSIGNED";
				}
				else if (systype == typeof(DateTime))
				{
					column += "DATETIME";
				}
				else if (systype == typeof(System.SByte))
				{
					column += "TINYINT";
				}
				else if (systype == typeof(System.Int16))
				{
					column += "SMALLINT";
				}
				else if (systype == typeof(System.Int32))
				{
					column += "INT";
				}
				else if (systype == typeof(System.Int64))
				{
					column += "BIGINT";
				}
				else if (systype == typeof(System.Byte))
				{
					column += "TINYINT UNSIGNED";
				}
				else if (systype == typeof(System.UInt16))
				{
					column += "SMALLINT UNSIGNED";
				}
				else if (systype == typeof(System.UInt32))
				{
					column += "INT UNSIGNED";
				}
				else if (systype == typeof(System.UInt64))
				{
					column += "BIGINT UNSIGNED";
				}
				else if (systype == typeof(System.Single))
				{
					column += "FLOAT";
				}
				else if (systype == typeof(System.Double))
				{
					column += "DOUBLE";
				}
				else if (systype == typeof(System.Boolean))
				{
					column += "TINYINT(1)";
				}
				//else if (systype == typeof(System.String))
				//{
				//    if (primaryKeys[table.Columns[i].ColumnName] != null ||
				//        table.Columns[i].ExtendedProperties.ContainsKey("INDEX") ||
				//        table.Columns[i].Unique)
				//    {
				//        column += "VARCHAR(255)";
				//    }
				//    else
				//    {
				//        column += "TEXT";
				//    }
				//}
				else if (systype == typeof(System.String))
				{
					if (primaryKeys[table.Columns[i].ColumnName] != null)
					{
						//column += "VARCHAR(255)";
						column += "INT UNSIGNED auto_increment";
					}
					else
					{
						if (table.Columns[i].ExtendedProperties.ContainsKey("INDEX") ||
							table.Columns[i].Unique)

							column += "VARCHAR(255)";

						else
							column += "TEXT";
					}
				}
				else
				{
					column += "BLOB";
				}
				if (!table.Columns[i].AllowDBNull)
				{
					column += " NOT NULL";
				}

				columnDefs.Add(column);

				// if the column doesnt exist but the table, then alter table
				if (currentTableColumns.Count > 0 && !currentTableColumns.Contains(table.Columns[i].ColumnName.ToLower()))
				{
					log.Debug("added for alteration " + table.Columns[i].ColumnName.ToLower());
					alterAddColumnDefs.Add(column);
				}
			}

			string columndef = string.Join(", ", (string[])columnDefs.ToArray(typeof(string)));

			// create primary keys
			if (table.PrimaryKey.Length > 0)
			{
				columndef += ", PRIMARY KEY (";
				bool first = true;
				for (int i = 0; i < table.PrimaryKey.Length; i++)
				{
					if (!first)
					{
						columndef += ", ";
					}
					else
					{
						first = false;
					}
					columndef += "`" + table.PrimaryKey[i].ColumnName + "`";
				}
				columndef += ")";
			}

			// unique indexes				
			for (int i = 0; i < table.Columns.Count; i++)
			{
				if (table.Columns[i].Unique && primaryKeys[table.Columns[i].ColumnName] == null)
				{
					columndef += ", UNIQUE INDEX (`" + table.Columns[i].ColumnName + "`)";
				}
			}

			// indexes
			for (int i = 0; i < table.Columns.Count; i++)
			{
				if (table.Columns[i].ExtendedProperties.ContainsKey("INDEX")
					&& primaryKeys[table.Columns[i].ColumnName] == null
					&& !table.Columns[i].Unique)
				{
					columndef += ", INDEX (`" + table.Columns[i].ColumnName + "`)";
				}
			}
			sb.Append("CREATE TABLE IF NOT EXISTS `" + table.TableName + "` (" + columndef + ")");

			try
			{
				if (log.IsDebugEnabled)
					log.Debug(sb.ToString());
				ExecuteNonQuery(sb.ToString());
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error while creating table " + table.TableName, e);
			}


			// alter table if needed
			if (alterAddColumnDefs.Count > 0)
			{
				columndef = string.Join(", ", (string[])alterAddColumnDefs.ToArray(typeof(string)));
				string alterTable = "ALTER TABLE `" + table.TableName + "` ADD (" + columndef + ")";
				try
				{
					if (log.IsDebugEnabled)
						log.Debug(alterTable);
					ExecuteNonQuery(alterTable);
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error while altering table table " + table.TableName, e);
				}
			}
		}

		/// <summary>
		/// Gets the format for date times
		/// </summary>
		/// <returns></returns>
		public string GetDBDateFormat()
		{
			return "yyyy-MM-dd HH:mm:ss";
		}
	}
}
