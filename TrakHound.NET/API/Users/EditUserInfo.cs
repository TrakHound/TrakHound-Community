// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Newtonsoft.Json;

namespace TrakHound.API.Users
{
    public class EditUserInfo
    {
        [JsonIgnore]
        public string SessionToken { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        //[JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        //public string Password { get; set; }

        //[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
        //public string FirstName { get; set; }

        //[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
        //public string LastName { get; set; }

        //[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
        //public string Company { get; set; }

        //[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        //public string Email { get; set; }

        //[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
        //public string Phone { get; set; }

        //[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
        //public string Address1 { get; set; }

        //[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
        //public string Address2 { get; set; }

        //[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        //public string City { get; set; }

        //[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        //public string State { get; set; }

        //[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        //public string Country { get; set; }

        //[JsonProperty("zipcode", NullValueHandling = NullValueHandling.Ignore)]
        //public string Zipcode { get; set; }

        //[JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
        //public string ImageUrl { get; set; }
    }
}
