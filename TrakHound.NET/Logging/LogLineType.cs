// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


namespace TrakHound.Logging
{

    public enum LogLineType
    {
        /// <summary>
        /// Used for debugging only, Shows detailed information
        /// </summary>
        Debug,

        /// <summary>
        /// Used to show warninig information (ex. 'Could not find file' when method continues anyways)
        /// </summary>
        Warning,

        /// <summary>
        /// Used to show error information (ex. a feature will not work because of this error)
        /// </summary>
        Error,

        /// <summary>
        /// Used to show notification information (ex. a feature has started)
        /// </summary>
        Notification,

        /// <summary>
        /// Used to only write to the console
        /// </summary>
        Console,
    }

}
