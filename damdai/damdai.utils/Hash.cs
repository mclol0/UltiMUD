using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Damdai
{
    public class Hash
    {
        public enum Algorithm
        {
            SHA1,
            MD5
        }

        public static string Generate(string str, Algorithm algorithm)
        {
            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] buf = ue.GetBytes(str);
            byte[] hashValue = null;

            switch (algorithm)
            {
                case Algorithm.MD5:
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    hashValue = md5.ComputeHash(buf);
                    break;
                case Algorithm.SHA1:
                    SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                    hashValue = sha1.ComputeHash(buf);
                    break;
                default:
                    throw new Exception("Unsupported algorithm.");
            }

            string hash = "";
            foreach (byte b in hashValue)
            {
                hash += string.Format("{0:x2}", b);
            }
            return hash;
        }
        public static string GenerateFromFile(string path, Algorithm algorithm)
        {
            byte[] hashValue = null;

            using (FileStream fs = File.OpenRead(path))
            {
                switch (algorithm)
                {
                    case Algorithm.MD5:
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        hashValue = md5.ComputeHash(fs);
                        break;
                    case Algorithm.SHA1:
                        SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                        hashValue = sha1.ComputeHash(fs);
                        break;
                    default:
                        throw new Exception("Unsupported algorithm.");
                }
            }

            string hash = "";
            foreach (byte b in hashValue)
            {
                hash += string.Format("{0:x2}", b);
            }
            return hash;
        }

        public static string EncryptPassword(string pass, string salt)
        {
            byte[] buffer1 = Encoding.Unicode.GetBytes(pass);
            byte[] buffer2 = Convert.FromBase64String(salt);
            byte[] buffer3 = new byte[buffer2.Length + buffer1.Length];
            byte[] buffer4 = null;
            Buffer.BlockCopy(buffer2, 0, buffer3, 0, buffer2.Length);
            Buffer.BlockCopy(buffer1, 0, buffer3, buffer2.Length, buffer1.Length);
            // buffer3 is salt plus password
            HashAlgorithm algorithm1 = HashAlgorithm.Create("sha1");
            buffer4 = algorithm1.ComputeHash(buffer3);
            return Convert.ToBase64String(buffer4);
        }
    }
}
