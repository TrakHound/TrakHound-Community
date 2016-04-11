using System;
using System.Text;
using TH_Global.Web;

namespace TH_GitHub
{
    public static class Authentication
    {
        public const string GITHUB_API_URL = "https://api.github.com/";
        public const string AUTHENTICATION_URL = "https://api.github.com/authorizations";

        public class Crendentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static HTTP.HeaderData GetHeaderData(Crendentials credentials)
        {
            String encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.Username + ":" + credentials.Password));

            var header = new HTTP.HeaderData();
            header.Id = "Authorization";
            header.Text = "Basic " + encoded;
            return header;
        }

        public static string GetToken(string username)
        {
            string format = "'client_secret':'{0}', 'note':'{1}','note_url':'{2}'";

            string client_secret = "acf7e9e80eab5f238271ad8a2e0863025ad326ba";
            string note = "TrakHound";
            string note_url = "http://www.trakhound.org";

            string data = string.Format(format, client_secret, note, note_url);
            data = "{" + data + "}";

            byte[] bytes = Encoding.UTF8.GetBytes(data);

            return TH_Global.Web.HTTP.PUT(AUTHENTICATION_URL + "/clients/" + username, bytes);
        }

        public static string CreateToken(string username)
        {
            string format = "'scopes':['{0}'],'client_id':'{1}','client_secret':'{2}', 'note':'{3}','note_url':'{4}'";

            string scopes = "public_repo";
            string client_id = "a1178c3ffdfd1adea560";
            string client_secret = "acf7e9e80eab5f238271ad8a2e0863025ad326ba";
            string note = "TrakHound";
            string note_url = "http://www.trakhound.org";

            string data = string.Format(format, scopes, client_id, client_secret, note, note_url);
            data = "{" + data + "}";

            byte[] bytes = Encoding.UTF8.GetBytes(data);

            return TH_Global.Web.HTTP.POST(AUTHENTICATION_URL, bytes);
        }

    }
}
