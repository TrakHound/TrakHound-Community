// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;

using TrakHound.Configurations;

namespace TrakHound.Plugins.Database
{
    /// <summary>
    /// This is the interface for writing Plugins for Database Connection. 
    /// All Plugins MUST contain the following properties and methods.
    /// </summary>
    [InheritedExport(typeof(IDatabasePlugin))]
    public interface IDatabasePlugin
    {
        /// <summary>
        /// Name of the Plugin (ex. MySQL Database Plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Name of the Database Type (ex. MySQL)
        /// Use the underscore character ('_') instead of spaces (ex. 'SQL_Server')
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Configuration Page Type (ex. typeof(TH_MySQL.ConfigurationPage.Page))
        /// </summary>
        //Type Config_Page { get; }


        /// <summary>
        /// Method to create the 'Configuration Button' used in the TH_DeviceManager.Database.Page usercontrol.
        /// Typically contains the Database Name, IP address, and any other basic info to 
        /// quickly identify which database is which
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        //object CreateConfigurationButton(DataTable dt);

        /// <summary>
        /// Method to intialize the plugin using the Database_Configuration object passed as an argument.
        /// This is where the configuration can be read and processed by the plugin
        /// </summary>
        /// <param name="config"></param>
        void Initialize(Database_Configuration config);

        /// <summary>
        /// Method to determine if the database is able to be connected to.
        /// This can also include checks for user permissions, etc.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Ping(object settings, out string msg);



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
