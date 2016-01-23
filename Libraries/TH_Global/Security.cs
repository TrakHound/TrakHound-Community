using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;


namespace TH_Global
{
    public static class Security
    {

        public class Password
        {
            public string text { get; set; }
            public string hash { get; set; }
            public int salt { get; set; }

            public Password(string txt)
            {
                text = txt;
                salt = CreateRandomSalt();
                hash = ComputeSaltedHash(txt, salt);
            }

            public Password(string txt, int slt)
            {
                text = txt;
                salt = slt;
                hash = ComputeSaltedHash(txt, salt);
            }

            public static string CreateRandomPassword(int PasswordLength)
            {
                String _allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ23456789";
                Byte[] randomBytes = new Byte[PasswordLength];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(randomBytes);
                char[] chars = new char[PasswordLength];
                int allowedCharCount = _allowedChars.Length;

                for (int i = 0; i < PasswordLength; i++)
                {
                    chars[i] = _allowedChars[(int)randomBytes[i] % allowedCharCount];
                }

                return new string(chars);
            }

            public static int CreateRandomSalt()
            {
                Byte[] _saltBytes = new Byte[4];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(_saltBytes);

                return ((((int)_saltBytes[0]) << 24) + (((int)_saltBytes[1]) << 16) +
                  (((int)_saltBytes[2]) << 8) + ((int)_saltBytes[3]));
            }

            public static string ComputeSaltedHash(string text, int salt)
            {
                // Create Byte array of password string
                ASCIIEncoding encoder = new ASCIIEncoding();
                Byte[] _secretBytes = encoder.GetBytes(text);

                // Create a new salt
                Byte[] _saltBytes = new Byte[4];
                _saltBytes[0] = (byte)(salt >> 24);
                _saltBytes[1] = (byte)(salt >> 16);
                _saltBytes[2] = (byte)(salt >> 8);
                _saltBytes[3] = (byte)(salt);

                // append the two arrays
                Byte[] toHash = new Byte[_secretBytes.Length + _saltBytes.Length];
                Array.Copy(_secretBytes, 0, toHash, 0, _secretBytes.Length);
                Array.Copy(_saltBytes, 0, toHash, _secretBytes.Length, _saltBytes.Length);

                SHA1 sha1 = SHA1.Create();
                Byte[] computedHash = sha1.ComputeHash(toHash);

                return encoder.GetString(computedHash);
            }

            public static string ComputeSaltedHash(string text, string salt)
            {
                // Create Byte array of password string
                ASCIIEncoding encoder = new ASCIIEncoding();
                Byte[] _secretBytes = encoder.GetBytes(text);

                Byte[] _saltBytes = encoder.GetBytes(salt);

                // append the two arrays
                Byte[] toHash = new Byte[_secretBytes.Length + _saltBytes.Length];
                Array.Copy(_secretBytes, 0, toHash, 0, _secretBytes.Length);
                Array.Copy(_saltBytes, 0, toHash, _secretBytes.Length, _saltBytes.Length);

                SHA1 sha1 = SHA1.Create();
                Byte[] computedHash = sha1.ComputeHash(toHash);

                return encoder.GetString(computedHash);
            }

            public static string ComputeHash(string text)
            {
                // Create Byte array of password string
                //UTF8Encoding encoder = new UTF8Encoding();
                ASCIIEncoding encoder = new ASCIIEncoding();
                Byte[] _secretBytes = encoder.GetBytes(text);

                // append the two arrays
                Byte[] toHash = new Byte[_secretBytes.Length];
                Array.Copy(_secretBytes, 0, toHash, 0, _secretBytes.Length);

                SHA1 sha1 = SHA1.Create();
                Byte[] computedHash = sha1.ComputeHash(toHash);

                return encoder.GetString(computedHash);
            }

        }

    }
}
