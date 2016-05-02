// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TH_Global
{
    public static class TableNames
    {

        // Device_Server ----------------------------------
        public const string AgentInfo = "agent_info";
        // ------------------------------------------------

        // Variables --------------------------------------
        public const string Variables = "variables";
        // ------------------------------------------------

        // Instance Table ---------------------------------
        public const string Instance = "instance";
        // ------------------------------------------------

        // ShiftTable -------------------------------------
        public const string Shifts = "shifts";
        public const string ShiftSegments = "shiftsegments";
        // ------------------------------------------------

        // GeneratedData ----------------------------------
        public const string SnapShots = "snapshots";
        public const string GenEventValues = "geneventvalues";
        public const string Gen_Events_TablePrefix = "gen_event_";
        // ------------------------------------------------

        // Cycles -----------------------------------------
        public const string Cycles = "cycles";
        public const string Cycles_Setup = "cycles_setup";
        // ------------------------------------------------

        // OEE --------------------------------------------
        public const string OEE = "oee"; // DEPRECATED 4-25-16
        public const string OEE_Cycles = "oee_cycles";
        public const string OEE_Segments = "oee_segments";
        public const string OEE_Shifts = "oee_shifts";
        // ------------------------------------------------

        // Parts --------------------------------------------
        public const string Parts = "parts";
        // ------------------------------------------------

        // Status --------------------------------------------
        public const string Status = "status";
        // ------------------------------------------------

    }

}
