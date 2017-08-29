using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Globalization;
using GM.DB.Clients;
using GM.DB.Entities;
using GM.DB.Entities.Attributes;
using GM.DB.Entities.Descriptors;

namespace GM.DB.Business
{
    public class BusinessObjectManager
    {
        Dictionary<Type, EntityDescriptor> objectdescriptorlookup = new Dictionary<Type, EntityDescriptor>();

        readonly IDBClient dbclient;

        public BusinessObjectManager(IDBClient dbclient)
        {
            this.dbclient = dbclient;
        }

        public IDBClient DBClient {
            get { return dbclient; }
        }

        public void InitObjects<T>() {
            EntityDescriptor bod = GetDescriptor(typeof(T));

            if(!dbclient.DBInfo.CheckIfTableExists(dbclient, bod.TableName))
                Create<T>();
        }

        long Insert<T>(T obj) {
            EntityDescriptor bod = GetDescriptor(typeof(T));

            StringBuilder columnbuilder = new StringBuilder();
            StringBuilder valuebuilder = new StringBuilder();

            string column;
            object value;
            List<object> values = new List<object>();
            foreach(KeyValuePair<string, ColumnDescriptor> kvp in bod.Columns) {
                if(!kvp.Value.PrimaryKey) {
                    column = kvp.Key.Replace("'", "");
                    if(columnbuilder.Length > 0) columnbuilder.Append(',');
                    columnbuilder.Append(column);

                    value = kvp.Value.Property.GetValue(obj, null);
                    if(valuebuilder.Length > 0) valuebuilder.Append(',');
                    valuebuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", dbclient.DBInfo.Parameter, values.Count + 1);
                    values.Add(value);
                }
            }

            string command=string.Format("INSERT INTO {0} ({1}) VALUES ({2})", bod.TableName, columnbuilder.ToString(), valuebuilder.ToString());

            object id = dbclient.Scalar(command + ";\nSELECT " + dbclient.DBInfo.LastValue, values.ToArray());
            if(id is long) return (long)id;
            else return 0;
        }

        DBCommand Update<T>(T obj) {
            EntityDescriptor bod = GetDescriptor(typeof(T));

            StringBuilder sb = new StringBuilder();
            List<object> parameters = new List<object>();

            string column;
            string value;
            object rawvalue;
            ColumnDescriptor keycolumn = null;
            foreach(KeyValuePair<string, ColumnDescriptor> kvp in bod.Columns) {
                if(!kvp.Value.PrimaryKey) {
                    column = kvp.Key.Replace("'", "");
                    rawvalue = kvp.Value.Property.GetValue(obj, null);
                    if(rawvalue == null) continue;

                    parameters.Add(rawvalue);
                    if(sb.Length == 0) sb.AppendFormat(CultureInfo.InvariantCulture, "UPDATE {0} SET {3}{1}{3} = {4}{2}", bod.TableName, column, parameters.Count, dbclient.DBInfo.ColumnIndicator, dbclient.DBInfo.Parameter);
                    else sb.AppendFormat(CultureInfo.InvariantCulture, ", {2}{0}{2} = {3}{1}", column, parameters.Count, dbclient.DBInfo.ColumnIndicator, dbclient.DBInfo.Parameter);
                }
                else keycolumn = kvp.Value;
            }
            
            if(keycolumn == null) return null;

            sb.AppendFormat(" WHERE {0}={1}", keycolumn.Name, keycolumn.Property.GetValue(obj, null).ToString());
            return new DBCommand(sb.ToString(), parameters.ToArray());
        }

        public void Save<T>(T obj) {
            EntityDescriptor bod = GetDescriptor(typeof(T));

            if(bod == null)
                throw new ArgumentException("Object has no business object descriptor");
            if(bod.PrimaryKeyColumn == null)
                throw new ArgumentException("Object needs primary key to be used with the business object manager");


            if((long)bod.PrimaryKeyColumn.Property.GetValue(obj, null) == 0) {
                long id = Insert<T>(obj);
                bod.PrimaryKeyColumn.Property.SetValue(obj, id, null);
            }
            else {
                DBCommand updatecommand = Update<T>(obj);
                dbclient.NonQuery(updatecommand.Text, updatecommand.Arguments);
            }

        }

        public string UpdateIncrement<T>(T obj, string keycolumn, params string[] exceptions) {
            EntityDescriptor bod = GetDescriptor(typeof(T));

            HashSet<string> exceptionlookup = new HashSet<string>(exceptions);
            exceptionlookup.Add(keycolumn);

            StringBuilder sb = new StringBuilder();

            string column;
            string value;

            foreach(KeyValuePair<string, ColumnDescriptor> kvp in bod.Columns) {
                if(!exceptionlookup.Contains(kvp.Key) && kvp.Value.Property.PropertyType.IsValueType) {
                    column = kvp.Key.Replace("'", "");
                    value = kvp.Value.Property.GetValue(obj, null).ToString();
                    if(value != "0") {
                        if(sb.Length == 0) sb.AppendFormat(CultureInfo.InvariantCulture, "UPDATE {0} SET {3}{1}{3} = ({3}{1}{3}+{2})", bod.TableName, column, value, dbclient.DBInfo.ColumnIndicator);
                        else sb.AppendFormat(CultureInfo.InvariantCulture, ", {2}{0}{2} = ({2}{0}{2}+{1})", column, value, dbclient.DBInfo.ColumnIndicator);
                    }
                }
            }

            ColumnDescriptor pi = bod.GetProperty(keycolumn);
            if(pi == null) return null;

            sb.AppendFormat(" WHERE {0}={1}", keycolumn, pi.Property.GetValue(obj, null).ToString());
            return sb.ToString();
        }

        public void Create<T>() {
            EntityDescriptor descriptor = GetDescriptor(typeof(T));

            if (dbclient.DBInfo.CheckIfTableExists(dbclient, descriptor.TableName))
                // table already exists
                return;

            StringBuilder commandbuilder = new StringBuilder("CREATE TABLE ");
            commandbuilder.Append(descriptor.TableName).Append(" (");

            bool firstindicator = true;
            foreach(KeyValuePair<string, ColumnDescriptor> kvp in descriptor.Columns) {
                if(firstindicator) firstindicator = false;
                else commandbuilder.Append(", ");

                commandbuilder.Append(dbclient.DBInfo.MaskColumn(kvp.Key)).Append(" ").Append(dbclient.DBInfo.GetDBType(kvp.Value.Property.PropertyType));

                if(kvp.Value.PrimaryKey)
                    commandbuilder.Append(" PRIMARY KEY");
                if(kvp.Value.AutoIncrement)
                    commandbuilder.Append(" ").Append(dbclient.DBInfo.AutoIncrement);
                if(kvp.Value.Unique)
                    commandbuilder.Append(" UNIQUE");
                if(kvp.Value.NotNull)
                    commandbuilder.Append(" NOT NULL");

                if(kvp.Value.DefaultValue != null)
                    commandbuilder.Append(" DEFAULT ").Append(kvp.Value.DefaultValue.ToString());
            }

            commandbuilder.Append(")");

            if (!string.IsNullOrEmpty(dbclient.DBInfo.CreateSuffix))
                commandbuilder.Append(" ").Append(dbclient.DBInfo.CreateSuffix);

            dbclient.NonQuery(commandbuilder.ToString());

            foreach(IndexDescriptor indexdescriptor in descriptor.Indices) {
                commandbuilder.Length = 0;
                commandbuilder.Append("CREATE INDEX ").Append("idx_").Append(descriptor.TableName).Append("_").Append(indexdescriptor.Name).Append(" ON ").Append(descriptor.TableName).Append(" (");
                firstindicator=true;
                foreach(string column in indexdescriptor.Columns)
                {
                    if(firstindicator) firstindicator = false;
                    else commandbuilder.Append(", ");
                    commandbuilder.Append(column);
                }
                commandbuilder.Append(")");

                dbclient.NonQuery(commandbuilder.ToString());
            }
        }

        public T LoadSingle<T>(string query, params object[] parameters)
            where T : class
        {
            T[] objects = Load<T>(query, parameters);
            if (objects.Length == 0) return null;
            return objects[0];
        }

        public T[] Load<T>(string query, params object[] parameters)
        {
            DataTable result = dbclient.Query(query, parameters);
            if(result != null)
                return CreateObjects<T>(result);
            else return new T[] { };
        }

        T[] CreateObjects<T>(DataTable dt)
        {
            if(dt == null) return null;

            EntityDescriptor bod = GetDescriptor(typeof(T));
            T obj;
            ColumnDescriptor pi;
            List<T> objects = new List<T>();
            object dbvalue;
            foreach(DataRow row in dt.Rows)
            {
                obj = (T)Activator.CreateInstance(typeof(T), true);
                foreach(DataColumn column in dt.Columns)
                {
                    pi = bod.GetProperty(column.ColumnName);
                    if (pi == null) continue;
                    dbvalue = row[column.ColumnName];
                    if(pi.Property.PropertyType.IsEnum)
                    {
                        try
                        {
                            int index = Convert.ToInt32(dbvalue);
                            object value = Enum.ToObject(pi.Property.PropertyType, index);
                            pi.Property.SetValue(obj, value, null);
                        }
                        catch (System.Exception ex)
                        {
                        	
                        }
                    }
                    else if(dbvalue.GetType()==pi.Property.PropertyType)
                        pi.Property.SetValue(obj, dbvalue, null);
                    else if(dbvalue is DBNull)
                    {
                        if (!pi.Property.PropertyType.IsValueType)
                            pi.Property.SetValue(obj, null, null);
                    }
                    else{
                        try
                        {
                            object pvalue = Convert.ChangeType(dbvalue, pi.Property.PropertyType);
                            pi.Property.SetValue(obj, pvalue, null);
                        }
                        catch (System.Exception ex)
                        {
                        	
                        }
                    }
                }
                objects.Add(obj);
            }
            return objects.ToArray();
        }
    }
}
