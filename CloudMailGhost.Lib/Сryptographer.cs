using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace CloudMailGhost.Lib
{
    public static class Сryptographer
    {
        // Фиксированный размер ключа для AES-256
        private const int KEY_SIZE_BYTES = 32;

        public static byte[] Encode(byte[] plainData, string key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = DeriveKey(key, KEY_SIZE_BYTES);
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.Zeros;

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainData, 0, plainData.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public static byte[] Decode(byte[] cipherData, string key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = DeriveKey(key, KEY_SIZE_BYTES);
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.Zeros;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                using (MemoryStream msDecrypt = new MemoryStream(cipherData))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        csDecrypt.CopyTo(msOutput);
                        return msOutput.ToArray();
                    }
                }
            }
        }

        // Пасхал очка
        private static byte[] salt = Encoding.UTF8.GetBytes("0J/QsNGI0LAt0J/QsNGI0LAg0LvRi9GB0YvQuSDQs9C10LksINC+0L0g0L3QsNGB0LjQu9GD0LXRgiDQtNC10YLQtdC5");

        private static byte[] DeriveKey(string password, int keySizeBytes)
        {
            // Используем PBKDF2 для безопасного получения ключа из пароля           
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(keySizeBytes);
            }
        }
    }
}
