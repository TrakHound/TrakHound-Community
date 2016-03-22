using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_MTConnect;
using TH_Plugins_Server;

namespace TH_InstanceTable
{
    public partial class InstanceTable
    {

        public string TableName = TableNames.Instance;
        string[] primaryKey = null;

        InstanceData previousDatabaseData = null;

        void CreateInstanceTable(List<string> variablesToRecord)
        {
            TableName = TablePrefix + TableNames.Instance;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("TIMESTAMP", DataType.DateTime));
            columns.Add(new ColumnDefinition("SEQUENCE", DataType.Long));
            columns.Add(new ColumnDefinition("AGENTINSTANCEID", DataType.Long));

            foreach (string variable in variablesToRecord) columns.Add(new ColumnDefinition(variable.ToUpper(), DataType.LargeText));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, TableName, ColArray, primaryKey);

        }

        void AddRowsToDatabase(List<string> columns, List<InstanceData> instanceDatas)
        {
            List<string> reqColumns = new List<string>();

            if (columns != null)
            {
                if (!columns.Contains("TIMESTAMP")) reqColumns.Add("TIMESTAMP");
                if (!columns.Contains("SEQUENCE")) reqColumns.Add("SEQUENCE");
                if (!columns.Contains("AGENTINSTANCEID")) reqColumns.Add("AGENTINSTANCEID");

                List<List<object>> rowValues = new List<List<object>>();

                InstanceData previousData = previousDatabaseData;

                foreach (InstanceData instanceData in instanceDatas)
                {
                    List<object> values = new List<object>();

                    values.Add(instanceData.timestamp);
                    values.Add(instanceData.sequence);
                    values.Add(instanceData.agentInstanceId);

                    bool changed = false;

                    string prev_output = "";
                    string output = "";

                    foreach (string column in columns)
                    {
                        InstanceData.Value value = instanceData.values.Find(x => x.id.ToUpper() == column.ToUpper());
                        InstanceData.Value prev_Value = null;

                        if (previousData != null) prev_Value = previousData.values.Find(x => x.id.ToUpper() == column.ToUpper());

                        string val = null;
                        string prev_val = null;

                        if (value != null) val = value.value;
                        if (prev_Value != null) prev_val = prev_Value.value;

                        prev_output += column + ":" + prev_val + "|";
                        output += column + ":" + val + "|";

                        if (val != prev_val) changed = true;

                        values.Add(val);
                    }

                    previousData = instanceData;

                    if (changed) rowValues.Add(values);
                }

                previousDatabaseData = previousData;

                List<string> columnsMySQL = new List<string>();

                columnsMySQL.AddRange(reqColumns);
                columnsMySQL.AddRange(columns);

                int interval = 50;
                int countLeft = rowValues.Count;

                while (countLeft > 0)
                {
                    IEnumerable<List<object>> ValuesToAdd = rowValues.Take(interval);

                    countLeft -= interval;

                    Row.Insert(config.Databases_Server, TableName, columnsMySQL.ToArray(), ValuesToAdd.ToList(), primaryKey, false);
                }
            }
        }

    }
}
