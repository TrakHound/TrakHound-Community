using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;

using System.Data;

using TH_Configuration;

namespace TH_Database
{
    [InheritedExport(typeof(Database_Plugin))]
    public interface Database_Plugin
    {
        string Name { get; }

        string Type { get; }

        //object Configuration { get; set; }

        void Initialize(Database_Configuration config);

        bool Ping(object settings);


        // Database Functions -----------------------------------------------------------

        bool Database_Create(object settings);

        bool Database_Drop(object settings);

        // ------------------------------------------------------------------------------


        // Table ------------------------------------------------------------------------

        bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string primaryKey);

        bool Table_Drop(object settings, string tablename);

        bool Table_Drop(object settings, string[] tablenames);

        bool Table_Truncate(object settings, string tablename);

        DataTable Table_Get(object settings, string tablename);

        DataTable Table_Get(object settings, string tablename, string filterExpression);

        DataTable Table_Get(object settings, string tablename, string filterExpression, string columns);

        string[] Table_List(object settings);

        Int64 Table_GetRowCount(object settings, string tablename);

        Int64 Table_GetSize(object settings, string tablename);

        // ------------------------------------------------------------------------------


        // Column -----------------------------------------------------------------------

        List<string> Column_Get(object settings, string tablename);

        bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition);

        // ------------------------------------------------------------------------------


        // Row --------------------------------------------------------------------------

        bool Row_Insert(object settings, string tablename, object[] columns, object[] values, bool update);

        bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> values, bool update);

        bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, bool update);

        bool Row_Insert(object settings, string query);


        DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey);

        DataRow Row_Get(object settings, string tablename, string query);


        bool Row_Exists(object settings, string tablename, string filterString);

        // ------------------------------------------------------------------------------


        // Etc --------------------------------------------------------------------------

        object[] CustomCommand(object settings, string commandText);

        object GetValue(object settings, string tablename, string column, string filterExpression);

        DataTable GetGrants(object settings, string username);

        // ------------------------------------------------------------------------------

    }


    public enum DataType
    {
        // Numeric 0 - 100
        Short = 0,
        Long = 10,
        Double = 20,

        // Text 100 - 200
        SmallText = 100,
        MediumText = 110,
        LargeText = 120,

        // Date 200 - 300
        DateTime = 200
    }

    public class ColumnDefinition
    {
        public ColumnDefinition() { }

        public ColumnDefinition(string columnName, DataType dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        public ColumnDefinition(string columnName, DataType dataType, bool primaryKey)
        {
            ColumnName = columnName;
            DataType = dataType;
            PrimaryKey = primaryKey;
        }

        public ColumnDefinition(string columnName, DataType dataType, bool primaryKey, bool notNull)
        {
            ColumnName = columnName;
            DataType = dataType;
            PrimaryKey = primaryKey;
            NotNull = notNull;
        }


        public string ColumnName { get; set; }

        public DataType DataType { get; set; }

        public bool NotNull { get; set; }
        public string Default { get; set; }

        public bool RowId { get; set; }

        public bool PrimaryKey { get; set; }
        public bool Unique { get; set; }

    }


}
