// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Configurations.Converters;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Devices
    {

        // Example URL (GET)
        // -----------------------------------------------------

        // ../api/devices/get?token=01234&sender_id=56789   				  (Get all devices for the user)
        // OR
        // ../api/devices/get?token=01234&sender_id=56789&unique_id=ABCDEFG   (Get only the device with the specified 'unique_id')

        // -----------------------------------------------------

        // Example Post Data
        // -----------------------------------------------------

        // name = 'token'
        // value = 01234 (Session Token)

        // name = 'sender_id'
        // value = 56789 (Sender ID)

        // (Optional)
        // name = 'devices'
        // value =  [
        //	{ "unique_id": "ABCDEFG" },
        //  { "unique_id": "HIJKLMN" }
        //	]
        // -----------------------------------------------------

        /// <summary>
        /// Get a single DeviceConfiguration
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <param name="deviceUniqueId">The Unique ID of the device to return</param>
        /// <returns></returns>
        public static DeviceConfiguration Get(UserConfiguration userConfig, string deviceUniqueId)
        {
            if (!string.IsNullOrEmpty(deviceUniqueId))
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "devices/get/index.php").ToString();

                string format = "{0}?token={1}&sender_id={2}{3}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();
                string uniqueId = "unique_id=" + deviceUniqueId;

                url = string.Format(format, url, token, senderId, uniqueId);

                string response = HTTP.GET(url);
                if (response != null)
                {
                    bool success = ApiError.ProcessResponse(response, "Get Devices");
                    if (success)
                    {
                        var deviceInfo = JSON.ToType<DeviceInfo>(response);
                        if (deviceInfo != null)
                        {
                            var table = deviceInfo.ToTable();
                            if (table != null)
                            {
                                var xml = DeviceConfigurationConverter.TableToXML(table);
                                if (xml != null)
                                {
                                    var deviceConfig = DeviceConfiguration.Read(xml);
                                    if (deviceConfig != null && !string.IsNullOrEmpty(deviceConfig.UniqueId))
                                    {
                                        return deviceConfig;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private class GetDeviceInfo
        {
            public GetDeviceInfo(string uniqueId)
            {
                UniqueId = uniqueId;
            }

            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }
        }

        public static List<DeviceConfiguration> Get(UserConfiguration userConfig, string[] deviceUniqueIds)
        {
            if (deviceUniqueIds != null && deviceUniqueIds.Length > 0)
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "devices/get/index.php").ToString();

                var getDeviceInfos = new List<GetDeviceInfo>();
                foreach (var deviceUniqueId in deviceUniqueIds) getDeviceInfos.Add(new GetDeviceInfo(deviceUniqueId));

                string json = JSON.FromObject(getDeviceInfos);
                if (json != null)
                {
                    var postDatas = new NameValueCollection();
                    postDatas["token"] = userConfig.SessionToken;
                    postDatas["sender_id"] = UserManagement.SenderId.Get();
                    postDatas["devices"] = json;

                    string response = HTTP.POST(url, postDatas);
                    if (response != null)
                    {
                        bool success = ApiError.ProcessResponse(response, "Get Devices");
                        if (success)
                        {
                            var deviceInfos = JSON.ToType<List<DeviceInfo>>(response);
                            if (deviceInfos != null)
                            {
                                var deviceConfigs = new List<DeviceConfiguration>();

                                foreach (var deviceInfo in deviceInfos)
                                {
                                    var table = deviceInfo.ToTable();
                                    if (table != null)
                                    {
                                        var xml = DeviceConfigurationConverter.TableToXML(table);
                                        if (xml != null)
                                        {
                                            var deviceConfig = DeviceConfiguration.Read(xml);
                                            if (deviceConfig != null && !string.IsNullOrEmpty(deviceConfig.UniqueId))
                                            {
                                                deviceConfigs.Add(deviceConfig);
                                            }
                                        }
                                    }
                                }

                                return deviceConfigs;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a list of DeviceConfigurations of all of the user's devices
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <returns></returns>
        public static List<DeviceConfiguration> Get(UserConfiguration userConfig)
        {
            Uri apiHost = ApiConfiguration.AuthenticationApiHost;

            string url = new Uri(apiHost, "devices/get/index.php").ToString();

            string format = "{0}?token={1}&sender_id={2}";

            string token = userConfig.SessionToken;
            string senderId = UserManagement.SenderId.Get();

            url = string.Format(format, url, token, senderId);

            string response = HTTP.GET(url);
            if (response != null)
            {
                bool success = ApiError.ProcessResponse(response, "Get Devices");
                if (success)
                {
                    var deviceInfos = JSON.ToType<List<DeviceInfo>>(response);
                    if (deviceInfos != null)
                    {
                        var deviceConfigs = new List<DeviceConfiguration>();

                        foreach (var deviceInfo in deviceInfos)
                        {
                            var table = deviceInfo.ToTable();
                            if (table != null)
                            {
                                var xml = DeviceConfigurationConverter.TableToXML(table);
                                if (xml != null)
                                {
                                    var deviceConfig = DeviceConfiguration.Read(xml);
                                    if (deviceConfig != null && !string.IsNullOrEmpty(deviceConfig.UniqueId))
                                    {
                                        deviceConfigs.Add(deviceConfig);
                                    }
                                }
                            }
                        }

                        return deviceConfigs;
                    }
                }
            }

            return null;
        }

    }
    
}
