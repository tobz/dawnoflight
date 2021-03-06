/*
 * (c) 2008 Julian Bangert 
 * This file is part of
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
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;



namespace DOL.Database2
{        
    //TODO: Rework with GENERICS
    public class DatabaseLayer: System.Collections.Generic.IEnumerable<DatabaseObject> // Some c# compilers need that apparently xD
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#region Singleton
		/// <summary>
        /// The Singleton Instance
        /// </summary>
        private static volatile DatabaseLayer m_instance = null;
        /// <summary>
        /// Singleton Lock Object
        /// </summary>
        private static object syncRoot = new Object();
        private DatabaseLayer() { }
        public static DatabaseLayer Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_instance == null)
                            m_instance = new DatabaseLayer();
                    }
                }

                return m_instance;
            }
        } 
	#endregion
        IFormatter m_formatter = new BinaryFormatter();
        private DatabaseProvider m_provider = null;
        private readonly Dictionary<UInt64,DatabaseObject> DatabaseObjects = new Dictionary<UInt64,DatabaseObject>();
        #region RestoreWorldState
        public void RestoreWorldState()
        {
            Queue <DatabaseObjectInformation> DatabaseInformation =  m_provider.GetAllObjects();
            IFormatter formatter = new BinaryFormatter();
            foreach(DatabaseObjectInformation objinf in DatabaseInformation)
            {
                Type ObjectType = null ;
                foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    ObjectType = asm.GetType(objinf.TypeName);
                    if(ObjectType != null)
                        break;
                }
                if(ObjectType == null)
                {
                    log.Error("Type "+objinf.TypeName+" referenced by ObjectID "+objinf.UniqueID+" was not found");
                }
                else
                {
                    DatabaseObject Object;
                    try
                    {
                        Object = (DatabaseObject)formatter.Deserialize(objinf.Data);
                        DatabaseObjects.Add(Object.ID, Object);
                    }
                    catch(SerializationException e)
                    {
                        log.Error("Could not deserialize "+objinf.TypeName+" with ObjectID "+objinf.UniqueID,e);
                    }                    
                }
            }
        }
        #endregion
        #region SaveWorldState,SaveObject

        public void SaveWorldState()
        {
            foreach (DatabaseObject obj in DatabaseObjects.Values)
            {
                SaveObject(obj);
            }
        }
        /// <summary>
        /// Saves a DatabaseObject
        /// </summary>
        /// <param name="Object">DatabaseObject to save</param>
        public void SaveObject(DatabaseObject Object)
        {
            try
            {                
                //TODO: Tuning possible here , keep the MemoryStream , but it would have to be cleared
                MemoryStream memstream = new MemoryStream();
                m_formatter.Serialize(memstream, Object); // The object is converted into binary Data from the database
                m_provider.InsertOrUpdateObjectData(Object.ID, Object.GetType(), memstream); // We save the data
            }
            catch (SerializationException e)
            {
                if (log.IsErrorEnabled)
                    log.Error("Could not serialize"+typeof(Object).FullName+" " + Object.ID, e);
            }
        }
#endregion
        #region Select
        public DatabaseObject SelectObject(Type type, string MemberName, object Value)
        {
            FieldInfo field = type.GetField(MemberName);
            PropertyInfo property = type.GetProperty(MemberName);
            if (property != null)
            {
                return SelectObject(property, Value);
            }
            else if (field != null)
            {
                return SelectObject(field, Value);
            }
            else
            {
                throw new PropertyFieldNotFoundException(MemberName, type);
            }
        }
        public DatabaseObject SelectObject(FieldInfo field, object Value)
        {
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (field.DeclaringType == dbo.GetType())
                {
                    if (field.GetValue(dbo) == Value)
                    {
                        return dbo;
                    }
                }
            }
            return null;
        }
        public DatabaseObject SelectObject(PropertyInfo property, object Value)
        {
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (property.DeclaringType == dbo.GetType())
                {
                    if (property.GetValue(dbo,null) == Value)
                    {
                        return dbo;
                    }
                }
            }
            return null;
        }
        public List<T> SelectObjects<T>() where T: DatabaseObject
        {
            List<T> list = new List<T>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if(dbo.GetType() == typeof(T))
                    list.Add(dbo as T);
            }
            return list;
        }
        public List<T> SelectObjects<T>(string MemberName,object value) where T : DatabaseObject
        {
            FieldInfo field = typeof(T).GetField(MemberName);
            PropertyInfo property = typeof(T).GetProperty(MemberName);
            if (property != null)
            {
                return SelectObjects<T>(property, value);
            }
            else if (field != null)
            {
                return SelectObjects<T>(field, value);
            }
            throw new PropertyFieldNotFoundException(MemberName, typeof(T));
        }
        public List<T> SelectObjects<T>(PropertyInfo info, object value) where T : DatabaseObject
        {
            List<T> list = new List<T>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if(dbo.GetType() == typeof(T))
                    if(info.GetValue(dbo,null) == value)
                        list.Add(dbo as T);
            }
            return list;
        }
        public List<T> SelectObjects<T>(FieldInfo info, object value) where T : DatabaseObject
        {
            List<T> list = new List<T>();
            foreach (DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (dbo.GetType() == typeof(T) && info.GetValue(dbo) == value)
                    list.Add(dbo as T);
            }
            return list;
        }
        #endregion
        public bool Contains(UInt64 Key)
        {
            return DatabaseObjects.ContainsKey(Key);
        }
        //A lot of little helpers for common Select Statements !
        #region Select Helpers 
 
        public DatabaseObject GetDatabaseObjectFromID(UInt64 ID)
        {
            if (ID == 0)
                return null;
            //TODO: Tune that with The native field exceptions and map that to the DOL specific exception
            if (!DatabaseObjects.ContainsKey(ID))
                return null;
            return DatabaseObjects[ID];
        }
        public DatabaseObject GetDatabaseObjectFromIDnb(Type t,string id_nb)
        {
            return SelectObject(t, "Id_nb", id_nb);
        }
        #endregion
        public UInt64 GetNewUniqueID()
        {
            return m_provider.GetNewUniqueID();
        }
        public UInt64 GetObjectCount(Type t)//TODO: tune perhaps
        {
            UInt64 retval = 0;
            foreach(DatabaseObject dbo in DatabaseObjects.Values)
            {
                if (dbo.GetType() == t)
                    retval++;
            }
            return retval;
        }
        /// <summary>
        /// Compatibility Layer part 1
        /// </summary>
        /// <param name="Object"></param>
        [Obsolete]
        public void AddNewObject(DatabaseObject Object) // Automatically happens
        {
        }
        public void DeleteObject(DatabaseObject Object)
        {
            DatabaseObjects.Remove(Object.ID);
            m_provider.DeleteObject(Object.ID);
        }
        public void SetProvider(DatabaseProvider provider,string ConnectionString)
        {
            if (m_provider != null)
            {
                // Unload old one
                m_provider.CloseConnection();
            }
            m_provider = provider;
            m_provider.ConnectionString = ConnectionString;
            m_provider.OpenConnection();
        }

        public void RegisterDatabaseObject(DatabaseObject dbo)
        {
            if (!DatabaseObjects.ContainsKey(dbo.ID))
                DatabaseObjects.Add(dbo.ID, dbo);
        }
       
        #region IEnumerable Members

        IEnumerator<DatabaseObject> IEnumerable<DatabaseObject>.GetEnumerator()
        {
            return DatabaseObjects.Values.GetEnumerator() as IEnumerator<DatabaseObject>;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return DatabaseObjects.Values.GetEnumerator() ;
        }
        #endregion
    }
}
