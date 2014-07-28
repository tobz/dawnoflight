using System.Security.Cryptography;
using System.Text;

namespace DawnOfLight.GameServer.Utilities
{
    public static class Password
    {
        public static string HashPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            char[] pw = password.ToCharArray();

            var res = new byte[pw.Length * 2];
            for (int i = 0; i < pw.Length; i++)
            {
                res[i * 2] = (byte)(pw[i] >> 8);
                res[i * 2 + 1] = (byte)(pw[i]);
            }

            byte[] bytes = md5.ComputeHash(res);

            var crypted = new StringBuilder();
            crypted.Append("##");
            for (int i = 0; i < bytes.Length; i++)
            {
                crypted.Append(bytes[i].ToString("X"));
            }

            return crypted.ToString();
        }
    }
}
