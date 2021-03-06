﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatePr
{
    public class QueryInsertItem : QueryTemplateBase
    {
        protected override List<T> ExecuteQuery<T>(SqlCommand cmd, T item)
        {
            Type type_of_record = typeof(T);

            string tableName = "";

            var customAttributes = (MyTableNameAttribute[])type_of_record.GetCustomAttributes(typeof(MyTableNameAttribute), true);
            if (customAttributes.Length > 0)
            {
                tableName = customAttributes[0].TableName;
            }
            else
                throw new ArgumentException($"Poco {type_of_record.FullName} does not contain MyTableNameAttribute");

            string insert_names = GetPropertyNames(type_of_record);

            string values = GetPropertiesValues(item, type_of_record);

            cmd.CommandText = $"INSERT INTO {tableName} ({insert_names}) VALUES ({values})";

            cmd.ExecuteScalar();

            return null;
        }

        private static string GetPropertiesValues<T>(T item, Type type_of_record) where T : new()
        {
            string values = "";
            foreach (var oneProperty in type_of_record.GetProperties())
            {
                string columnName = oneProperty.Name;

                var customFieldAttributes = (MyFieldNameAttribute[])oneProperty.GetCustomAttributes(typeof(MyFieldNameAttribute), true);
                if (customFieldAttributes.Length > 0)
                {
                    columnName = customFieldAttributes[0].ColumnName;
                }

                // let's assume id is identity (auto-increment)
                if (columnName.ToUpper() != "ID")
                {
                    values += (values != "" ? "," : "") +
                                (oneProperty.PropertyType == typeof(string) ? "'" : "") +
                                oneProperty.GetValue(item) +
                                (oneProperty.PropertyType == typeof(string) ? "'" : "");
                }
            }

            return values;
        }

        private static string GetPropertyNames(Type type_of_record)
        {
            string insert_names = "";
            foreach (var oneProperty in type_of_record.GetProperties())
            {
                string columnName = oneProperty.Name;

                var customFieldAttributes = (MyFieldNameAttribute[])oneProperty.GetCustomAttributes(typeof(MyFieldNameAttribute), true);
                if (customFieldAttributes.Length > 0)
                {
                    columnName = customFieldAttributes[0].ColumnName;
                }

                if (columnName.ToUpper() != "ID")
                {
                    insert_names += (insert_names != "" ? "," : "") + columnName;
                }
            }

            return insert_names;
        }
    }
}
