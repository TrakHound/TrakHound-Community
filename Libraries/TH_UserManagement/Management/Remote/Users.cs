using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;
using System.Data;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Web;

namespace TH_UserManagement.Management.Remote
{
    public static class Users
    {
        const int connectionAttempts = 3;

        public const string tablename = "users";

        public static UserConfiguration Login(string username, string password)
        {
            UserConfiguration result = null;

            DataRow dbrow = GetLoginData(username, password);
            if (dbrow != null)
            {
                Logger.Log(username + " Logged in Successfully!");

                result = LoginSuccess(dbrow);
            }
            else Logger.Log("Username '" + username + "' Not Found in Database!");

            return result;
        }

        static DataRow GetLoginData(string id, string password)
        {
            DataRow Result = null;

            NameValueCollection values = new NameValueCollection();
            values["id"] = id;
            values["password"] = password;

            string url = "https://www.feenux.com/php/users/login.php";


            string responseString = HTTP.SendData(url, values);

            DataTable dt = JSON.ToTable(responseString);
            if (dt != null) if (dt.Rows.Count > 0) Result = dt.Rows[0];

            return Result;
        }

        static UserConfiguration LoginSuccess(DataRow dbrow)
        {
            UserConfiguration result = null;

            result = UserConfiguration.GetFromDataRow(dbrow);

            UpdateLoginTime(result);

            return result;
        }


        public static bool CreateUser(UserConfiguration userConfig, string password)
        {
            bool result = false;

            NameValueCollection values = new NameValueCollection();

            values["password"] = password;

            values["username"] = userConfig.username;
            values["first_name"] = userConfig.first_name;
            values["last_name"] = userConfig.last_name;
            values["company"] = userConfig.company;
            values["email"] = userConfig.email;
            values["phone"] = userConfig.phone;
            values["address1"] = userConfig.address1;
            values["address2"] = userConfig.address2;
            values["city"] = userConfig.city;
            values["state"] = userConfig.state;
            values["country"] = userConfig.country;
            values["zipcode"] = userConfig.zipcode;

            string url = "https://www.feenux.com/php/users/createuser.php";

            string responseString = HTTP.SendData(url, values);
            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;
        }


        public static VerifyUsernameReturn VerifyUsername(string username)
        {
            VerifyUsernameReturn result = null;

            NameValueCollection values = new NameValueCollection();
            values["username"] = username;

            string url = "https://www.feenux.com/php/users/verifyusername.php";


            string responseString = HTTP.SendData(url, values);

            string output = responseString.Trim();

            if (output != "")
            {
                result = new VerifyUsernameReturn();

                if (output.Contains(';'))
                {
                    int index = output.IndexOf(';');

                    bool available = false;
                    string avail = output.Substring(0, index);
                    bool.TryParse(avail, out available);

                    string message = null;
                    if (output.Length > index + 1) message = output.Substring(index + 1);

                    result.available = available;
                    result.message = message;
                }
            }

            return result;
        }

        public static bool UpdateLoginTime(UserConfiguration userConfig)
        {
            bool result = false;

            NameValueCollection values = new NameValueCollection();

            values["username"] = userConfig.username;
            values["lastlogin"] = userConfig.last_login.ToString();

            string url = "https://www.feenux.com/php/users/updatelastlogin.php";


            string responseString = HTTP.SendData(url, values);

            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;
        }

        public static bool UpdateImageURL(string imageURL, UserConfiguration userConfig)
        {
            userConfig.image_url = imageURL;

            bool result = false;

            NameValueCollection values = new NameValueCollection();

            values["username"] = userConfig.username;
            values["imageurl"] = userConfig.image_url;

            string url = "https://www.feenux.com/php/users/updateimageurl.php";

            string responseString = HTTP.SendData(url, values);

            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;

        }

        public static class RememberMe
        {
            public static bool Set(UserConfiguration userConfig, RememberMeType type)
            {
                bool result = false;

                string id = userConfig.username;

                string t = type.ToString().ToLower();

                NameValueCollection values = new NameValueCollection();

                values["id"] = id;
                values["type"] = type.ToString();

                string url = "https://www.feenux.com/php/users/setrememberme.php";


                string responseString = HTTP.SendData(url, values);

                if (responseString != null) if (responseString.Trim() != "")
                    {
                        string hash = responseString.Trim();

                        // Set "id" registry key
                        TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_id", userConfig.username);

                        // Set Date of Last Login using Remember Me
                        TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_last_login", DateTime.UtcNow.ToString());

                        // Set "hash" registry key
                        TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_hash", hash);

                        result = true;
                    }

                return result;
            }

            public static UserConfiguration Get(RememberMeType type)
            {
                UserConfiguration result = null;

                string t = type.ToString().ToLower();

                string strLast_Login = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_last_login");
                DateTime last_login = DateTime.MinValue;
                if (DateTime.TryParse(strLast_Login, out last_login))
                {
                    TimeSpan sinceLastLogin = DateTime.UtcNow - last_login;
                    if (sinceLastLogin > TimeSpan.FromDays(14))
                    {
                        Clear(type);
                    }
                    else
                    {
                        string id = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
                        string hash = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_hash");

                        if (hash != null)
                        {
                            NameValueCollection values = new NameValueCollection();

                            values["id"] = id;
                            values["hash"] = hash;
                            values["type"] = type.ToString();

                            string url = "https://www.feenux.com/php/users/getrememberme.php";


                            string responseString = HTTP.SendData(url, values);

                            DataTable dt = JSON.ToTable(responseString);
                            if (dt != null) if (dt.Rows.Count > 0) result = LoginSuccess(dt.Rows[0]);
                        }
                    }
                }

                return result;
            }

            public static bool Clear(RememberMeType type)
            {
                bool result = false;

                string t = type.ToString().ToLower();

                // Set "id" registry key
                string key = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
                if (key != null)
                {
                    NameValueCollection values = new NameValueCollection();

                    values["id"] = key;
                    values["type"] = type.ToString();


                    string url = "https://www.feenux.com/php/users/clearrememberme.php";

                    string responseString = HTTP.SendData(url, values);

                    if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;
                }

                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_id");
                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_last_login");
                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "__hash");

                return result;
            }
        }
    }
}
