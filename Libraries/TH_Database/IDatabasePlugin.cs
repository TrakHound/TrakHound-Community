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

        DataTable[] Table_Get(object settings, string[] tablenames, string[] filterExpressions, string[] columns);


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

}
