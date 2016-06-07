// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TH_Plugins.Database
{
    public static class DatabasePlugin
    { 
        //public const string PLUGIN_EXTENSION = ".dplugin";

        public class PluginContainer : ReaderContainer
        {
            [ImportMany(typeof(TH_Plugins.Database.IDatabasePlugin))]
            public IEnumerable<Lazy<object>> Plugins { get; set; }
        }

        /// <summary>
        /// Add plugins to list making sure that plugins are not repeated in list
        /// </summary>
        /// <param name="newPlugins"></param>
        /// <param name="oldPlugins"></param>
        static void AddPlugins(List<IDatabasePlugin> newPlugins, List<IDatabasePlugin> oldPlugins)
        {
            foreach (var plugin in newPlugins)
            {
                if (oldPlugins.Find(x => x.Name == plugin.Name) == null) oldPlugins.Add(plugin);
            }
        }
   
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
}
