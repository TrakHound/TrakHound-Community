//  Copyright 2016 Feenux LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.


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
