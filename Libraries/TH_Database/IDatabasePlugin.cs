using System;
using System.Collections.Generic;
using System.Windows.Media;

using System.ComponentModel.Composition;

using System.Data;

using TH_Configuration;

namespace TH_Database
{
    [InheritedExport(typeof(IDatabasePlugin))]
    public interface IDatabasePlugin
    {
        string Name { get; }

        string Type { get; }

        Type Config_Page { get; }

        object CreateConfigurationButton(DataTable dt);

        void Initialize(Database_Configuration config);

        bool Ping(object settings, out string msg);

        bool CheckPermissions(object settings, Application_Type type);

        // Database Functions -----------------------------------------------------------

        bool Database_Create(object settings);

        bool Database_Drop(object settings);

        // ------------------------------------------------------------------------------


        // Table ------------------------------------------------------------------------

        bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey);

        bool Table_Replace(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey);

        bool Table_Drop(object settings, string tablename);

        bool Table_Drop(object settings, string[] tablenames);

        bool Table_Truncate(object settings, string tablename);

        DataTable Table_Get(object settings, string tablename);

        DataTable Table_Get(object settings, string tablename, Int64 limit, Int64 offset);

        DataTable Table_Get(object settings, string tablename, string filterExpression);

        DataTable Table_Get(object settings, string tablename, string filterExpression, string columns);

        DataTable[] Table_Get(object settings, string[] tablenames);

        DataTable[] Table_Get(object settings, string[] tablenames, string[] filterExpressions);

        //DataTable[] Table_Get(object settings, string[] tablenames, string filterExpression);

        //DataTable[] Table_Get(object settings, string[] tablenames, string filterExpression, string columns);

        string[] Table_List(object settings);

        string[] Table_List(object settings, string filterExpression);

        Int64 Table_GetRowCount(object settings, string tablename);

        Int64 Table_GetSize(object settings, string tablename);

        // ------------------------------------------------------------------------------


        // Column -----------------------------------------------------------------------

        List<string> Column_Get(object settings, string tablename);

        bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition);

        // ------------------------------------------------------------------------------


        // Row --------------------------------------------------------------------------

        bool Row_Insert(object settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update);

        bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> values, string[] primaryKey, bool update);

        bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update);

        bool Row_Insert(object settings, string query);


        DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey);

        DataRow Row_Get(object settings, string tablename, string query);


        bool Row_Exists(object settings, string tablename, string filterString);

        // ------------------------------------------------------------------------------


        // Etc --------------------------------------------------------------------------

        string CustomCommand(object settings, string commandText);

        object GetValue(object settings, string tablename, string column, string filterExpression);

        DataTable GetGrants(object settings);

        // ------------------------------------------------------------------------------

    }


    public enum DataType
    {
        // Boolean 0 - 10
        Boolean = 0,

        // Numeric 10 - 100
        Short = 10,
        Long = 20,
        Double = 30,

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

    [InheritedExport(typeof(DatabaseConfigurationPage))]
    public interface DatabaseConfigurationPage
    {

        string PageName { get; }

        ImageSource Image { get; }

        event SettingChanged_Handler SettingChanged;

        string prefix { get; set; }
        //string ClientPrefix { get; set; }
        //string ServerPrefix { get; set; }

        void LoadConfiguration(DataTable dt);

        void SaveConfiguration(DataTable dt);

        Application_Type ApplicationType { get; set; }

        IDatabasePlugin Plugin { get; }

    }

    public enum Application_Type
    {
        Client = 0,
        Server = 1
    }

    public delegate void SettingChanged_Handler(string name, string oldVal, string newVal);

}
