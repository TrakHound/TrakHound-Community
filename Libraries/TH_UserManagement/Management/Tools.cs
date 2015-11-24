using System;
using System.Linq;
using System.Data;

namespace TH_UserManagement.Management
{

    public enum RememberMeType
    {
        Client = 0,
        Server = 1
    }

    public class VerifyUsernameReturn
    {
        public bool available { get; set; }
        public string message { get; set; }
    }


    public static class Security_Functions
    {
        public static bool VerifyPasswordMinimum(System.Security.SecureString pwd)
        {
            if (pwd.Length > 7) return true;
            else return false;
        }

        public static bool VerifyPasswordMaximum(System.Security.SecureString pwd)
        {
            if (pwd.Length < 101) return true;
            else return false;
        }
    }


    public static class Table_Functions
    {
        public static string GetTableValue(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                result = row["value"].ToString();
            }

            return result;
        }

        public static string RemoveTableRow(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                dt.Rows.Remove(row);
            }

            return result;
        }

        public static void UpdateTableValue(string value, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                dt.Rows.Add(row);
            }
        }

        public static void UpdateTableValue(string value, string attributes, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
                row["attributes"] = attributes;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                row["attributes"] = attributes;
                dt.Rows.Add(row);
            }
        }

        /// <summary>
        /// Get the last node in the Address column. Returns just the name and omits any Id's.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string GetLastNode(DataRow row)
        {
            string result = null;

            string adr = row["address"].ToString();

            if (adr.Contains('/'))
            {
                string s = adr;

                // Remove Last forward slash
                if (s[s.Length - 1] == '/') s = s.Substring(0, s.Length - 1);

                // Get index of last forward slash
                int slashIndex = s.LastIndexOf('/') + 1;
                if (slashIndex < s.Length) s = s.Substring(slashIndex);

                // Remove Id
                if (s.Contains("||"))
                {
                    int separatorIndex = s.LastIndexOf("||");
                    s = s.Substring(0, separatorIndex);
                }

                result = s;
            }

            return result;
        }

        /// <summary>
        /// Get value from the attributes column
        /// </summary>
        /// <param name="name"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string GetAttribute(string name, DataRow row)
        {
            string line = row["attributes"].ToString();

            if (line.Contains(name))
            {
                int a = line.IndexOf(name);
                if (a >= 0)
                {
                    int b = line.IndexOf("||", a) + 2;
                    int c = line.IndexOf(";", a);

                    if (b >= 0 && (c - b) > 0)
                    {
                        return line.Substring(b, c - b);
                    }
                }
            }

            return null;
        }

    }
}
