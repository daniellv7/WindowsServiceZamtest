using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace WindowsServiceZamtest
{
    public class Crypter
    {
        public void EncryptFile(string filePath, string key)
        {
            byte[] bytesRead = File.ReadAllBytes(filePath);
            using (DESCryptoServiceProvider DES = new DESCryptoServiceProvider())
            {
                DES.IV = Encoding.UTF8.GetBytes(key);
                DES.Key = Encoding.UTF8.GetBytes(key);
                DES.Mode = CipherMode.CBC;
                DES.Padding = PaddingMode.PKCS7;
                using (MemoryStream ms = new MemoryStream())
                {
                    CryptoStream cs = new CryptoStream(ms, DES.CreateEncryptor(), CryptoStreamMode.Write);
                    cs.Write(bytesRead, 0, bytesRead.Length);
                    cs.FlushFinalBlock();
                    File.WriteAllBytes(filePath, ms.ToArray());
                }
            }
        }

        public void DecryptFile(string filePath, string key)
        {
            byte[] encrypted = File.ReadAllBytes(filePath);
            using (DESCryptoServiceProvider DES = new DESCryptoServiceProvider())
            {
                DES.IV = Encoding.UTF8.GetBytes(key);
                DES.Key = Encoding.UTF8.GetBytes(key);
                DES.Mode = CipherMode.CBC;
                DES.Padding = PaddingMode.PKCS7;
                using (MemoryStream ms = new MemoryStream())
                {
                    CryptoStream cs = new CryptoStream(ms, DES.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(encrypted, 0, encrypted.Length);
                    cs.FlushFinalBlock();
                    File.WriteAllBytes(filePath, ms.ToArray());
                }
            }
        }
    }
}
