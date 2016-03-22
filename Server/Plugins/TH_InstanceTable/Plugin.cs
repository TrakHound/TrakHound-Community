// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
    public partial class InstanceTable : IServerPlugin
    {
        #region "PlugIn"

        public string Name { get { return "TH_InstanceTable"; } }

        public void Initialize(Configuration configuration)
        {
            InstanceConfiguration ic = null;

            if (firstPass)
            {
                ic = InstanceConfiguration.Read(configuration.ConfigurationXML);
            }
            else ic = InstanceConfiguration.Get(configuration);

            if (ic != null)
            {
                if (ic.DataItems.Conditions || ic.DataItems.Events || ic.DataItems.Samples) AddMySQL = true;
                else AddMySQL = false;

                if (firstPass) configuration.CustomClasses.Add(ic);
            }

            firstPass = false;

            config = configuration;
        }

        public void Update_Probe(TH_MTConnect.Components.ReturnData returnData)
        {

            ColumnNames = GetVariablesFromProbeData(returnData);

            if (UseDatabases) if (AddMySQL) CreateInstanceTable(ColumnNames);

        }

        public void Update_Current(TH_MTConnect.Streams.ReturnData returnData)
        {
            CurrentData = returnData;

            InstanceData instanceData = ProcessSingleInstance(returnData);

            PreviousInstanceData_old = PreviousInstanceData_new;

            CurrentInstanceData cid = new CurrentInstanceData();
            cid.currentData = returnData;
            cid.data = instanceData;

            // Send InstanceData object to other Plugins --
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "CurrentInstanceData";
            de_d.data01 = config;
            de_d.data02 = cid;

            if (DataEvent != null) DataEvent(de_d);
            //--------------------------------------------         
        }

        public void Update_Sample(TH_MTConnect.Streams.ReturnData returnData)
        {
            List<InstanceData> instanceDatas = ProcessInstances(CurrentData, returnData);

            if (UseDatabases) if (AddMySQL) AddRowsToDatabase(ColumnNames, instanceDatas);

            PreviousInstanceData_old = PreviousInstanceData_new;

            // Send instanceDatas to other Plugins --------
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "InstanceData";
            de_d.data01 = config;
            de_d.data02 = instanceDatas;

            SendDataEvent(de_d);
        }

        void SendDataEvent(DataEvent_Data de_d)
        {
            if (DataEvent != null) DataEvent(de_d);
        }

        public void Update_DataEvent(DataEvent_Data de_data)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {


        }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion


    }
}
