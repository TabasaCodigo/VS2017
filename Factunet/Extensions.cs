using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Factunet
{
    public static class Extensions
    {
        public static string GetString(this MemoryStream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            // Reset the stream position after flushing pending operations
            stream.Flush();
            stream.Position = 0;
            // Read the stream contents using a reader
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }


        public static string GetMD5Hash(this string input)
        {
            // Calculate MD5 hash using UTF8 encoding
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the result to lowercase HEX
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sb.Append(hashBytes[i].ToString("x2")); 
            }
            return sb.ToString();
        }
    }
}
