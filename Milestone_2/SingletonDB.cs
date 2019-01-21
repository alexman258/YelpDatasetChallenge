using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Npgsql;
using System.Runtime.Remoting;
using System.Reflection;
using System.Collections;

namespace Milestone_2
{
    public sealed class SingletonDB
    {
        // Constructor
        private SingletonDB()
        {
            connection = new NpgsqlConnection(BuildConnString());
        }

        private static NpgsqlConnection connection;
        private static SingletonDB instance = null;

        public static SingletonDB GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SingletonDB();
                }
                return instance;
            }
        }

        // Returns the information needed to access my DB
        private string BuildConnString()
        {
            return "Host = localhost; Username = postgres; Password = sappword321; Database = YelpApp";
        }


        public Dictionary<string, dynamic> RunQuery(string query)
        {
            connection.Open();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var type = reader.GetValue(i);
                        data.Add(reader.GetName(i), CreateList(type.GetType()));

                        var value = reader.GetValue(i);
                        IList propertyValue = (IList)data[reader.GetName(i)];
                        propertyValue.Add(value);

                        data[reader.GetName(i)] = propertyValue;
                    }
                    
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);

                            IList propertyValue = (IList)data[reader.GetName(i)];
                            propertyValue.Add(value);

                            data[reader.GetName(i)] = propertyValue;
                        }
                    }
                }
            }

            connection.Close();
            return data;
        }

        public IList CreateList(Type type)
        {
            Type genericList = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericList);
        }




        public Dictionary<string, dynamic> RunQuerry_Original(string query)
        {
            connection.Open();
            Dictionary<string, dynamic> table = new Dictionary<string, dynamic>();
            TypeConverter tc = new TypeConverter();

            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {                      
                        ColumnType(reader.GetDataTypeName(i)[0], table, reader.GetName(i));
                    }

                    while (reader.Read())
                    {
                        var boggie = reader.GetValue(0);

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dynamic list = table[reader.GetName(i)];
                            string returnType = list.GetType().GetGenericArguments()[0].ToString();

                            table[reader.GetName(i)].Add(ConvertToSpecificType(returnType, reader.GetValue(i).ToString()));
                        }
                    }

                }
            }

            connection.Close();
            return table;
        }

        private void ColumnType(char type, Dictionary<string, dynamic> table, string colName)
        {
            if (type == 'v' || type == 't' || type == 'b')
            {
                table.Add(colName, new List<string>());
            }
            else if (type == 'i')
            {
                table.Add(colName, new List<int>());
            }
            else if (type == 'f')
            {
                table.Add(colName, new List<double>());
            }
            else if (type == 'b')
            {
                table.Add(colName, new List<bool>());
            }
            else
            {
                // Shouldn't get here
                throw new ArgumentException("Invalid type");
            } 
        }

        public dynamic ConvertToSpecificType(string type, string value)
        {
            if (type == "System.String")
            {
                return value.ToString();
            }
            else if (type == "System.Double")
            {
                return Convert.ToDouble(value);
            }
            else if (type == "System.Int")
            {
                return Convert.ToInt32(value);
            }
            else
            {
                return -1;
            }
        }

        

        
    }
}
