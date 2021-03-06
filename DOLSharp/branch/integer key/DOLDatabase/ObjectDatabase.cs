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
using System.Reflection;
using System.Data;
using System.Text;
using System.Threading;
using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.Cache;
using DOL.Database.UniqueID;
using log4net;
using MySql.Data.Types;
using DataTable = System.Data.DataTable;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Collections.Generic;

namespace DOL.Database
{
	/// <summary>
	/// Database to to full Dokumentation
	/// </summary>
	public class ObjectDatabase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

		private Dictionary<string, DataTableHandler> tableDatasets;
		private DataConnection connection;

		public ObjectDatabase(DataConnection Connection)
		{
			tableDatasets = new Dictionary<string, DataTableHandler>();
			connection = Connection;
		}

		public string[] GetTableNameList()
		{
			List<string> tableList = new List<string>();
			foreach (KeyValuePair<string, DataTableHandler> kvp in tableDatasets)
			{
				tableList.Add(kvp.Key);
			}

			return tableList.ToArray();
		}

		[Obsolete("Cache is disabled.")]
		public void ReloadDatabaseTables()
		{
			foreach (KeyValuePair<string, DataTableHandler> kvp in tableDatasets)
			{
				ReloadCache(kvp.Key);
			}
		}

		[Obsolete("Cache is disabled.")]
		public void ReloadDatabaseTable(Type objectType)
		{
			string tableName = DataObject.GetTableName(objectType);
			ReloadCache(tableName);
		}

		public int GetObjectCount(Type objectType)
		{
			return GetObjectCount(objectType, "");
		}

		public int GetObjectCount(Type objectType, string where)
		{
			string tableName = DataObject.GetTableName(objectType);

			string query = "SELECT COUNT(*) FROM " + tableName;
			if (where != "")
				query += " WHERE " + where;
			object count = connection.ExecuteScalar(query);
			if (count is Int64)
				return (int)((Int64)count);
			return (int)count;
		}

		/// <summary>
		/// insert a new object into the db
		/// and save it if its autosave=true
		/// </summary>
		/// <param name="dataObject"></param>
		public void AddNewObject(DataObject dataObject)
		{
			try
			{
				string tableName = dataObject.TableName;

				if (dataObject.ObjectId != 0)
				{
					log.Warn("ObjectId is not equals to 0 for " + dataObject.TableName + "objectId #" + dataObject.ObjectId + " " + dataObject.ToString());
				}
				StringBuilder columns = new StringBuilder();
				StringBuilder values = new StringBuilder();

				MemberInfo[] objMembers = dataObject.GetType().GetMembers();
				bool hasRelations = false;
				string dateFormat = connection.GetDBDateFormat();

				columns.Append("`uid`");
				values.Append("NULL");

				for (int i = 0; i < objMembers.Length; i++)
				{
					if (!hasRelations)
					{
						object[] relAttrib = GetRelationAttributes(objMembers[i]);
						hasRelations = relAttrib.Length > 0;
					}
					object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);
					object[] attrib = objMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.DataElement), true);
					if (attrib.Length > 0 || keyAttrib.Length > 0)
					{
						object val = null;
						if (objMembers[i] is PropertyInfo)
						{
							val = ((PropertyInfo)objMembers[i]).GetValue(dataObject, null);
						}
						else if (objMembers[i] is FieldInfo)
						{
							val = ((FieldInfo)objMembers[i]).GetValue(dataObject);
						}

						columns.Append(", ");
						values.Append(", ");
						columns.Append("`" + objMembers[i].Name + "`");
						if (val is bool)
						{
							val = ((bool)val) ? (byte)1 : (byte)0;
						}
						else if (val is DateTime)
						{
							val = ((DateTime)val).ToString(dateFormat);
						}
						else if (val is float)
						{
							val = ((float)val).ToString(nfi);
						}
						else if (val is double)
						{
							val = ((double)val).ToString(nfi);
						}
						else if (val is string)
						{
							val = Escape(val.ToString());
						}
						values.Append('\'');
						values.Append(val);
						values.Append('\'');
					}
				}

				string sql = "INSERT INTO `" + tableName + "` (" + columns.ToString() + ") VALUES (" + values.ToString() + ")";
				if (log.IsDebugEnabled)
					log.Debug(sql);

				uint insertedId;

				int res = connection.ExecuteNonQuery(sql, out insertedId);
				if (res == 0)
				{
					if (log.IsErrorEnabled)
						log.Error("Error adding object into " + dataObject.TableName + " ID=" + dataObject.ObjectId + "Query = " + sql);
					return;
				}

				log.Debug("SELECT LAST_INSERT_ID(); return " + insertedId);
				dataObject.ObjectId = insertedId;
				log.Debug("dataObject.ObjectId set to  " + dataObject.ObjectId);

				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				PutObjectInCache(tableName, dataObject);
				dataObject.IsValid = true;

			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error while adding dataobject " + dataObject.TableName + " " + dataObject.ObjectId, e);
			}
		}

		/// <summary>
		/// saves an object to db in memory and when autosave is activated 
		/// it saves immediately persistent to database
		/// </summary>
		/// <param name="dataObject"></param>
		public void SaveObject(DataObject dataObject)
		{
			try
			{
				if (!dataObject.Dirty)
					return;
				string tableName = dataObject.TableName;

				StringBuilder sb = new StringBuilder("UPDATE `" + tableName + "` SET ");

				BindingInfo[] bindingInfo = GetBindingInfo(dataObject.GetType());
				bool hasRelations = false;
				bool first = true;
				string dateFormat = connection.GetDBDateFormat();

				for (int i = 0; i < bindingInfo.Length; i++)
				{
					BindingInfo bind = bindingInfo[i];
					if (!hasRelations)
					{
						hasRelations = bind.HasRelation;
					}
					if (!bind.HasRelation)
					{
						object val = null;
						if (bind.Member is PropertyInfo)
						{
							val = ((PropertyInfo)bind.Member).GetValue(dataObject, null);
						}
						else if (bind.Member is FieldInfo)
						{
							val = ((FieldInfo)bind.Member).GetValue(dataObject);
						}
						else
						{
							continue;
						}

						if (!first)
						{
							sb.Append(", ");
						}
						else
						{
							first = false;
						}

						if (val is bool)
						{
							val = ((bool)val) ? (byte)1 : (byte)0;
						}
						else if (val is DateTime)
						{
							val = ((DateTime)val).ToString(dateFormat);
						}
						else if (val is float)
						{
							val = ((float)val).ToString(nfi);
						}
						else if (val is double)
						{
							val = ((double)val).ToString(nfi);
						}
						else if (val is string)
						{
							val = Escape(val.ToString());
						}
						sb.Append("`" + bind.Member.Name + "` = ");
						sb.Append('\'');
						sb.Append(val);
						sb.Append('\'');
					}
				}

				sb.Append(" WHERE `uid` = '" + dataObject.ObjectId + "'");
				string sql = sb.ToString();
				if (log.IsDebugEnabled)
					log.Debug(sql);

				int res = connection.ExecuteNonQuery(sql);
				if (res == 0)
				{
					if (log.IsErrorEnabled)
						log.Error("Error modifying object " + dataObject.TableName + " uid = " + dataObject.ObjectId + " --- keyvalue changed?");
					return;
				}

				if (hasRelations)
				{
					SaveObjectRelations(dataObject);
				}

				dataObject.Dirty = false;
				dataObject.IsValid = true;

			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error while adding dataobject " + dataObject.TableName + " " + dataObject.ObjectId, e);
			}
		}

		public DataObject ReloadObject(DataObject dataObject)
		{
			try
			{
				if (dataObject == null)
					return null;
				DataObject ret = dataObject;

				DataRow row = FindRowByKey(ret);

				if (row == null)
					throw new DatabaseException("Reloading Databaseobject failed (Keyvalue Changed ?)!");

				FillObjectWithRow(ref ret, row, true);

				dataObject.Dirty = false;
				dataObject.IsValid = true;

				return ret;
			}
			catch (Exception e)
			{
				throw new DatabaseException("Reloading Databaseobject failed !", e);
			}
		}

		/// <summary>
		/// delete object from db and make it persist if autosave=true
		/// </summary>
		/// <param name="dataObject"></param>
		public void DeleteObject(DataObject dataObject)
		{
			string sql = "DELETE FROM `" + dataObject.TableName + "` WHERE `uid` = '" + dataObject.ObjectId + "'";
			if (log.IsDebugEnabled)
				log.Debug(sql);
			int res = connection.ExecuteNonQuery(sql);
			if (res == 0)
			{
				if (log.IsErrorEnabled)
					log.Error("Deleting " + dataObject.TableName + " object failed! uid = " + dataObject.ObjectId);
			}
		}

		public DataObject FindObjectByKey(Type objectType, object key)
		{
#warning TODO
			if (Activator.CreateInstance(objectType) is ItemTemplate)
			{
				//return FindObjectByIndex(objectType, key);
				return SelectObject(objectType, "Id_Nb = '" + connection.Escape(key.ToString()) + "'");
			}


			MemberInfo[] members = objectType.GetMembers();
			DataObject ret = (DataObject)Activator.CreateInstance(objectType);
			string tableName = ret.TableName;
			DataTableHandler dth = tableDatasets[tableName];
			string whereClause = null;

			if (dth.UsesPreCaching)
			{
				DataObject obj = dth.GetPreCachedObject(key);
				if (obj != null)
					return obj;
			}

			// Escape PK value
			key = Escape(key.ToString());

			for (int i = 0; i < members.Length; i++)
			{
				object[] keyAttrib = members[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);
				if (keyAttrib.Length > 0)
				{
					whereClause = "`" + members[i].Name + "` = '" + key.ToString() + "'";
					break;
				}
			}
			if (whereClause == null)
			{
				whereClause = "`uid` = '" + key.ToString() + "'";
			}
			DataObject[] objs = SelectObjects(objectType, whereClause);
			if (objs.Length > 0)
			{
				dth.SetPreCachedObject(key, objs[0]);
				return objs[0];
			}
			else
				return null;
		}

		private BindingInfo[] GetBindingInfo(Type objectType)
		{
			BindingInfo[] bindingInfos = (BindingInfo[])m_bindingInfos[objectType];
			if (bindingInfos == null)
			{
				ArrayList list = new ArrayList();
				MemberInfo[] objMembers = objectType.GetMembers();
				for (int i = 0; i < objMembers.Length; i++)
				{
					object[] keyAttrib = objMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);
					object[] attrib = objMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.DataElement), true);
					object[] relAttrib = GetRelationAttributes(objMembers[i]);

					if (attrib.Length > 0 || keyAttrib.Length > 0 || relAttrib.Length > 0)
					{
						BindingInfo info = new BindingInfo(objMembers[i], keyAttrib.Length > 0, relAttrib.Length > 0, (attrib.Length > 0) ? (DataElement)attrib[0] : null);
						list.Add(info);
					}
				}
				bindingInfos = (BindingInfo[])list.ToArray(typeof(BindingInfo));
				m_bindingInfos[objectType] = bindingInfos;
			}
			return bindingInfos;
		}

		/// <summary>
		/// Selects a single object, if more than
		/// one exist, the first is returned
		/// </summary>
		/// <param name="objectType">the type of the object</param>
		/// <param name="statement">the select statement</param>
		/// <returns>the object or null if none found</returns>
		public DataObject SelectObject(Type objectType, string statement)
		{
			DataObject[] objs = SelectObjects(objectType, statement);
			if (objs.Length > 0)
				return objs[0];
			return null;
		}

		public DataObject[] SelectObjects(Type objectType, string whereClause)
		{
			string tableName = DataObject.GetTableName(objectType);
			List<DataObject> dataObjects = new List<DataObject>(500);

			// build sql command
			StringBuilder sb = new StringBuilder("SELECT `uid`, ");
			bool first = true;
			BindingInfo[] bindingInfo = GetBindingInfo(objectType);
			for (int i = 0; i < bindingInfo.Length; i++)
			{
				if (!bindingInfo[i].HasRelation)
				{
					if (!first)
					{
						sb.Append(", ");
					}
					else
					{
						first = false;
					}
					sb.Append("`" + bindingInfo[i].Member.Name + "`");
				}
			}
			sb.Append(" FROM `" + tableName + "`");
			if (whereClause != null && whereClause.Trim().Length > 0)
			{
				sb.Append(" WHERE " + whereClause);
			}
			string sql = sb.ToString();

			if (log.IsDebugEnabled)
				log.Debug(sql);

			// read data and fill objects
			connection.ExecuteSelect(sql, delegate(MySqlDataReader reader)
				{
					object[] data = new object[reader.FieldCount];
					while (reader.Read())
					{
						reader.GetValues(data);
						uint id = (uint)data[0];

						DataObject cache = GetObjectInCache(tableName, id);

						if (cache != null)
						{
							dataObjects.Add(cache);
						}
						else
						{ // fill new data object
							DataObject obj = (DataObject)(Activator.CreateInstance(objectType));
							obj.ObjectId = id;

							bool hasRelations = false;
							int field = 1; // we can use hard index access because we iterate the same order here
							for (int i = 0; i < bindingInfo.Length; i++)
							{
								BindingInfo bind = bindingInfo[i];
								if (!hasRelations)
								{
									hasRelations = bind.HasRelation;
								}

								if (!bind.HasRelation)
								{
									object val = data[field++];
									if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
									{
										if (bind.Member is PropertyInfo)
										{
											Type type = ((PropertyInfo)bind.Member).PropertyType;

											try
											{
												if (type == typeof(bool))
												{
													// special handling for bool
													((PropertyInfo)bind.Member).SetValue(obj, (val.ToString() == "0") ? false : true, null);
												}
												else if (type == typeof(DateTime))
												{
													// special handling for datetime
													if (val is MySqlDateTime)
													{
														((PropertyInfo)bind.Member).SetValue(obj, ((MySqlDateTime)val).GetDateTime(), null);
													}
													else
													{
														((PropertyInfo)bind.Member).SetValue(obj, ((IConvertible)val).ToDateTime(null), null);
													}
												}
												else
												{
													((PropertyInfo)bind.Member).SetValue(obj, val, null);
												}
											}
											catch (Exception e)
											{
												if (log.IsErrorEnabled)
													log.Error(tableName + ": " + bind.Member.Name + " = " + val.GetType().FullName + " doesnt fit to " + bind.Member.DeclaringType.FullName, e);
												continue;
											}
										}
										else if (bind.Member is FieldInfo)
										{
											((FieldInfo)bind.Member).SetValue(obj, val);
										}
									}
								}
							}

							dataObjects.Add(obj);
							obj.Dirty = false;
							if (hasRelations)
							{
								FillLazyObjectRelations(obj, true);
							}
							PutObjectInCache(tableName, obj);
							obj.IsValid = true;
						}
					}
				}
			);
			return dataObjects.ToArray();
		}

		public DataObject[] SelectAllObjects(Type objectType)
		{
			return SelectObjects(objectType, null);
		}

#warning TODO find a better handling system
		public void RegisterDataObject(Type dataObjectType)
		{
			bool primary = false;
			bool relations = false;
			MemberInfo primaryIndexMember = null;

			string TableName = DataObject.GetTableName(dataObjectType);
			DataSet ds = new DataSet();
			DataTable table = new DataTable(TableName);
			table.Columns.Add("uid", typeof(string));
			MemberInfo[] myMembers = dataObjectType.GetMembers();

			for (int i = 0; i < myMembers.Length; i++)
			{

				object[] myAttributes = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);

				if (myAttributes.Length > 0)
				{
					primary = true;
					if (myMembers[i] is PropertyInfo)
						table.Columns.Add(myMembers[i].Name, ((PropertyInfo)myMembers[i]).PropertyType);
					else
						table.Columns.Add(myMembers[i].Name, ((FieldInfo)myMembers[i]).FieldType);

					DataColumn[] index = new DataColumn[1];
					index[0] = table.Columns[myMembers[i].Name];
					primaryIndexMember = myMembers[i];
					table.PrimaryKey = index;
					continue;
				}

				myAttributes = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.DataElement), true);

				if (myAttributes.Length > 0)
				{

					if (myMembers[i] is PropertyInfo)
					{
						table.Columns.Add(myMembers[i].Name, ((PropertyInfo)myMembers[i]).PropertyType);
					}
					else
					{
						table.Columns.Add(myMembers[i].Name, ((FieldInfo)myMembers[i]).FieldType);
					}

					table.Columns[myMembers[i].Name].AllowDBNull = ((Attributes.DataElement)myAttributes[0]).AllowDbNull;
					if (((Attributes.DataElement)myAttributes[0]).Unique)
					{
						table.Constraints.Add(new UniqueConstraint("UNIQUE_" + myMembers[i].Name, table.Columns[myMembers[i].Name]));
					}
					if (((Attributes.DataElement)myAttributes[0]).Index)
					{
						table.Columns[myMembers[i].Name].ExtendedProperties.Add("INDEX", true);
					}

					myAttributes = GetRelationAttributes(myMembers[i]);

					if (myAttributes.Length > 0)
					{
						relations = true;
					}
				}
			}

			if (primary == false)
			{
				DataColumn[] index = new DataColumn[1];
				index[0] = table.Columns["uid"];
				table.PrimaryKey = index;
			}

			connection.CheckOrCreateTable(table);

			ds.DataSetName = TableName;
			ds.EnforceConstraints = true;
			ds.CaseSensitive = false;
			ds.Tables.Add(table);

			DataTableHandler dth = new DataTableHandler(ds);
			dth.HasRelations = relations;
			dth.UsesPreCaching = DataObject.GetPreCachedFlag(dataObjectType);

			tableDatasets.Add(TableName, dth);

			if (dth.UsesPreCaching)
			{
				if (log.IsDebugEnabled)
					log.Debug("Precaching of " + table.TableName + "...");

				DataObject[] objects = SelectObjects(dataObjectType, null);
				object key;
				for (int i = 0; i < objects.Length; i++)
				{
					key = null;
					if (primaryIndexMember == null)
					{
						key = objects[i].ObjectId;
					}
					else
					{
						if (primaryIndexMember is PropertyInfo)
						{
							key = ((PropertyInfo)primaryIndexMember).GetValue(objects[i], null);
						}
						else if (primaryIndexMember is FieldInfo)
						{
							key = ((FieldInfo)primaryIndexMember).GetValue(objects[i]);
						}
					}
					if (key != null)
					{
						dth.SetPreCachedObject(key, objects[i]);
					}
					else
					{
						if (log.IsErrorEnabled)
							log.Error("Primary key is null! " + ((primaryIndexMember != null) ? primaryIndexMember.Name : ""));
					}
				}

				if (log.IsDebugEnabled)
					log.Debug("Precaching of " + table.TableName + " finished!");
			}
		}

		public DataSet GetDataSet(string TableName)
		{
			DataTableHandler handler = tableDatasets[TableName];
			return handler.DataSet;
		}

		private void FillObjectWithRow(ref DataObject DataObject, DataRow row, bool reload)
		{
			bool relation = false;

			string tableName = DataObject.TableName;
			Type myType = DataObject.GetType();
			uint id = (uint)row["uid"];

			DataObject cacheObj = GetObjectInCache(tableName, id);

			if (cacheObj != null)
			{
				DataObject = cacheObj;
				if (reload == false)
					return;
			}

			MemberInfo[] myMembers = myType.GetMembers();
			DataObject.ObjectId = id;

			for (int i = 0; i < myMembers.Length; i++)
			{
				object[] myAttributes = GetRelationAttributes(myMembers[i]);

				if (myAttributes.Length > 0)
				{
					relation = true;
				}
				else
				{
					object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);
					myAttributes = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.DataElement), true);
					if (myAttributes.Length > 0 || keyAttrib.Length > 0)
					{
						object val = row[myMembers[i].Name];
						if (val != null && !val.GetType().IsInstanceOfType(DBNull.Value))
						{
							if (myMembers[i] is PropertyInfo)
							{
								((PropertyInfo)myMembers[i]).SetValue(DataObject, val, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								((FieldInfo)myMembers[i]).SetValue(DataObject, val);
							}
						}
					}
				}
			}

			DataObject.Dirty = false;


			if (relation == true)
			{
				FillLazyObjectRelations(DataObject, true);
			}

			if (reload == false)
			{
				PutObjectInCache(tableName, DataObject);
			}

			DataObject.IsValid = true;
		}

#warning TODO remove this
		/*private DataObject GetObjectInPreCache(string TableName, object key)
		{
			DataTableHandler handler = tableDatasets[TableName] as DataTableHandler;
			return handler.GetPreCachedObject(key);
		}

		private void PutObjectInPreCache(string TableName, object key, DataObject obj)
		{
			DataTableHandler handler = tableDatasets[TableName] as DataTableHandler;
			handler.SetPreCachedObject(key, obj);
		}*/

		private void DeleteObjectInPreCache(string TableName, DataObject obj)
		{
			DataTableHandler handler = tableDatasets[TableName];
			handler.SetCacheObject(obj.ObjectId, null);
		}

		[Obsolete("Cache is disabled")]
		private DataObject GetObjectInCache(string TableName, uint id)
		{
			//			DataTableHandler handler = tableDatasets[TableName] as DataTableHandler;
			//			return handler.GetCacheObject(id);
			return null;
		}

		[Obsolete("Cache is disabled")]
		private void PutObjectInCache(string TableName, DataObject obj)
		{
			//			DataTableHandler handler = tableDatasets[TableName] as DataTableHandler;
			//			handler.SetCacheObject(obj.ObjectId, obj);
		}

		[Obsolete("Cache is disabled")]
		private void DeleteObjectInCache(string TableName, DataObject obj)
		{
			//			DataTableHandler handler = tableDatasets[TableName] as DataTableHandler;
			//			handler.SetCacheObject(obj.ObjectId, null);
		}

		private void FillRowWithObject(DataObject DataObject, DataRow row)
		{
			bool relation = false;

			Type myType = DataObject.GetType();

			row["uid"] = DataObject.ObjectId;

			MemberInfo[] myMembers = myType.GetMembers();

			for (int i = 0; i < myMembers.Length; i++)
			{
				object[] myAttributes = GetRelationAttributes(myMembers[i]);
				object val = null;

				if (myAttributes.Length > 0)
				{
					relation = true;
				}
				else
				{
					myAttributes = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.DataElement), true);
					object[] keyAttrib = myMembers[i].GetCustomAttributes(typeof(DOL.Database.Attributes.PrimaryKey), true);

					if (myAttributes.Length > 0 || keyAttrib.Length > 0)
					{
						if (myMembers[i] is PropertyInfo)
						{
							val = ((PropertyInfo)myMembers[i]).GetValue(DataObject, null);
						}
						if (myMembers[i] is FieldInfo)
						{
							val = ((FieldInfo)myMembers[i]).GetValue(DataObject);
						}
						if (val != null)
						{
							row[myMembers[i].Name] = val;
						}
					}
				}
			}
			if (relation == true)
			{
				SaveObjectRelations(DataObject);
			}
		}

		private DataRow FindRowByKey(DataObject DataObject)
		{
			DataRow row;

			String tableName = DataObject.TableName;


			DataTable table = GetDataSet(tableName).Tables[tableName];

			Type myType = DataObject.GetType();

			string key = table.PrimaryKey[0].ColumnName;

			if (key.Equals("uid"))
				row = table.Rows.Find(DataObject.ObjectId);
			else
			{
				MemberInfo[] keymember = myType.GetMember(key);

				object val = null;

				if (keymember[0] is PropertyInfo)
					val = ((PropertyInfo)keymember[0]).GetValue(DataObject, null);
				if (keymember[0] is FieldInfo)
					val = ((FieldInfo)keymember[0]).GetValue(DataObject);

				if (val != null)
					row = table.Rows.Find(val);
				else
					return null;
			}

			return row;
		}

		public void FillObjectRelations(DataObject DataObject)
		{
			FillLazyObjectRelations(DataObject, false);
		}

		private void SaveObjectRelations(DataObject DataObject)
		{
			try
			{
				object val;

				Type myType = DataObject.GetType();

				MemberInfo[] myMembers = myType.GetMembers();

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
					if (myAttributes.Length > 0)
					{
						bool array = false;

						Type type;

						if (myMembers[i] is PropertyInfo)
							type = ((PropertyInfo)myMembers[i]).PropertyType;
						else
							type = ((FieldInfo)myMembers[i]).FieldType;

						if (type.HasElementType)
						{
							type = type.GetElementType();
							array = true;
						}

						val = null;

						if (array)
						{
							if (myMembers[i] is PropertyInfo)
							{
								val = ((PropertyInfo)myMembers[i]).GetValue(DataObject, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								val = ((FieldInfo)myMembers[i]).GetValue(DataObject);
							}
							if (val is Array)
							{
								Array a = val as Array;

								foreach (object o in a)
								{
									if (o is DataObject)
										SaveObject(o as DataObject);
								}
							}
							else
							{
								if (val is DataObject)
									SaveObject(val as DataObject);
							}

						}
						else
						{
							if (myMembers[i] is PropertyInfo)
								val = ((PropertyInfo)myMembers[i]).GetValue(DataObject, null);
							if (myMembers[i] is FieldInfo)
								val = ((FieldInfo)myMembers[i]).GetValue(DataObject);
							if (val != null && val is DataObject)
								SaveObject(val as DataObject);
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Saving Relations failed !", e);
			}
		}

		private void DeleteObjectRelations(DataObject DataObject)
		{
			try
			{
				object val;

				Type myType = DataObject.GetType();

				MemberInfo[] myMembers = myType.GetMembers();

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
					if (myAttributes.Length > 0)
					{
						if (myAttributes[0].AutoDelete == false)
							continue;

						bool array = false;

						Type type;

						if (myMembers[i] is PropertyInfo)
							type = ((PropertyInfo)myMembers[i]).PropertyType;
						else
							type = ((FieldInfo)myMembers[i]).FieldType;

						if (type.HasElementType)
						{
							type = type.GetElementType();
							array = true;
						}

						val = null;

						if (array)
						{
							if (myMembers[i] is PropertyInfo)
							{
								val = ((PropertyInfo)myMembers[i]).GetValue(DataObject, null);
							}
							if (myMembers[i] is FieldInfo)
							{
								val = ((FieldInfo)myMembers[i]).GetValue(DataObject);
							}
							if (val is Array)
							{
								Array a = val as Array;

								foreach (object o in a)
								{
									if (o is DataObject)
										DeleteObject(o as DataObject);
								}
							}
							else
							{
								if (val is DataObject)
									DeleteObject(val as DataObject);
							}

						}
						else
						{
							if (myMembers[i] is PropertyInfo)
								val = ((PropertyInfo)myMembers[i]).GetValue(DataObject, null);
							if (myMembers[i] is FieldInfo)
								val = ((FieldInfo)myMembers[i]).GetValue(DataObject);
							if (val != null && val is DataObject)
								DeleteObject(val as DataObject);
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Resolving Relations failed !", e);
			}
		}

		protected readonly Hashtable m_memberInfoCache = new Hashtable();
		protected readonly Hashtable m_constructorByFieldType = new Hashtable();

		private void FillLazyObjectRelations(DataObject DataObject, bool Autoload)
		{
			try
			{
				Type myType = DataObject.GetType();

				MemberInfo[] myMembers = (MemberInfo[])m_memberInfoCache[myType];
				if (myMembers == null)
				{
					myMembers = myType.GetMembers();
					m_memberInfoCache[myType] = myMembers;
				}

				for (int i = 0; i < myMembers.Length; i++)
				{
					Relation[] myAttributes = GetRelationAttributes(myMembers[i]);
					if (myAttributes.Length > 0)
					{
						bool array = false;

						Relation rel = myAttributes[0];

						if (
							(rel.AutoLoad == false) &&
								(Autoload == true)
							)
							continue;

						DataObject[] Elements = null;

						string local = myAttributes[0].LocalField;
						string remote = myAttributes[0].RemoteField;

						Type type;

						if (myMembers[i] is PropertyInfo)
							type = ((PropertyInfo)myMembers[i]).PropertyType;
						else
							type = ((FieldInfo)myMembers[i]).FieldType;

						if (type.HasElementType)
						{
							type = type.GetElementType();
							array = true;
						}

						PropertyInfo prop = myType.GetProperty(local);
						FieldInfo field = myType.GetField(local);

						object val = 0;

						if (prop != null)
							val = prop.GetValue(DataObject, null);
						if (field != null)
							val = field.GetValue(DataObject);

						if (val != null)
							Elements = SelectObjects(type, remote + " = '" + Escape(val.ToString()) + "'");

						if (
							(Elements != null) &&
								(Elements.Length > 0)
							)
						{
							if (array)
							{
								if (myMembers[i] is PropertyInfo)
								{
									((PropertyInfo)myMembers[i]).SetValue(DataObject, Elements, null);
								}
								if (myMembers[i] is FieldInfo)
								{
									FieldInfo currentField = (FieldInfo)myMembers[i];
									ConstructorInfo constructor = (ConstructorInfo)m_constructorByFieldType[currentField.FieldType];
									if (constructor == null)
									{
										constructor = currentField.FieldType.GetConstructor(new Type[] { typeof(int) });
										m_constructorByFieldType[currentField.FieldType] = constructor;
									}

									object[] args = { Elements.Length };
									object t = constructor.Invoke(args);
									object[] test = (object[])t;

									for (int m = 0; m < Elements.Length; m++)
										test[m] = Elements[m];

									currentField.SetValue(DataObject, test);
								}
							}
							else
							{
								if (myMembers[i] is PropertyInfo)
									((PropertyInfo)myMembers[i]).SetValue(DataObject, Elements[0], null);
								if (myMembers[i] is FieldInfo)
									((FieldInfo)myMembers[i]).SetValue(DataObject, Elements[0]);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException("Resolving Relations for " + DataObject.TableName + " failed!", e);
			}
		}

		private void ReloadCache(string TableName)
		{
			DataTableHandler handler = tableDatasets[TableName];

			ICache cache = handler.Cache;

			foreach (object o in cache.Keys)
			{
				ReloadObject(cache[o] as DataObject);
			}
		}

		public string Escape(string toEscape)
		{
			return connection.Escape(toEscape);
		}

		protected readonly Hashtable m_bindingInfos = new Hashtable();

		private class BindingInfo
		{
			public MemberInfo Member;
			public bool PrimaryKey;
			public DataElement DataElementAttribute;
			public bool HasRelation;

			public BindingInfo(MemberInfo member, bool primaryKey, bool hasRelation, DataElement attrib)
			{
				this.Member = member;
				this.PrimaryKey = primaryKey;
				this.HasRelation = hasRelation;
				this.DataElementAttribute = attrib;
			}
		}

		protected readonly Hashtable m_relationAttributes = new Hashtable();

		protected Relation[] GetRelationAttributes(MemberInfo info)
		{
			Relation[] rel = (Relation[])m_relationAttributes[info];
			if (rel != null)
				return rel;

			rel = (Relation[])info.GetCustomAttributes(typeof(DOL.Database.Attributes.Relation), true);
			m_relationAttributes[info] = rel;

			return rel;
		}
	}
}
