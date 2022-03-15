using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Modules.WDCore
{
    public static class Decipher
    {
        private static byte[] EncryptStringToBytes_Des(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (DES des = DES.Create())
            {
                des.Key = Key;
                des.IV = IV;
                des.Mode = CipherMode.ECB;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = des.CreateEncryptor(des.Key, des.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static string DecryptStringFromBytes_Des(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (DES des = DES.Create())
            {
                des.Key = Key;
                des.IV = IV;
                des.Mode = CipherMode.ECB;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = des.CreateDecryptor(des.Key, des.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        public static byte[] EncryptStringToBytes_Des(string plainText)
        {
            /*Rfc2898DeriveBytes pdb = new 
                    Rfc2898DeriveBytes(password, new byte[] 
                        { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });*/

            if (string.IsNullOrEmpty(plainText)) return null;
            
            var des = DES.Create();

            var bytes = EncryptStringToBytes_Des(plainText, des.Key, des.IV);
            var resultBytes = new byte[bytes.Length + des.Key.Length + des.IV.Length];

            Buffer.BlockCopy(des.Key, 0, resultBytes, 0, des.Key.Length);
            Buffer.BlockCopy(des.IV, 0, resultBytes, des.Key.Length, des.IV.Length);
            Buffer.BlockCopy(bytes, 0, resultBytes, des.Key.Length + des.IV.Length, bytes.Length);

            return resultBytes;
        }
        
        public static string DecryptStringFromBytes_Des(byte[] cipherText)
        {
            /*Rfc2898DeriveBytes pdb = new 
                    Rfc2898DeriveBytes(password, new byte[] 
                        { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });*/

            if (cipherText == null) return null;
            var des = DES.Create();
            
            var key = new byte[des.Key.Length];
            var IV = new byte[des.IV.Length];
            var extractCipherText = new byte[cipherText.Length - des.Key.Length - des.IV.Length];

            Buffer.BlockCopy(cipherText, 0, key, 0, des.Key.Length);
            Buffer.BlockCopy(cipherText, des.Key.Length, IV, 0, des.IV.Length);
            Buffer.BlockCopy(cipherText, des.Key.Length + des.IV.Length, extractCipherText, 0, extractCipherText.Length);

            return DecryptStringFromBytes_Des(extractCipherText, key, IV);
        }

        public static byte[] DecryptBytesFromFile_OpenSSL_Des(string password, byte[] bytes)
        {
            var magic = "Salted__";
            var PKCS5_SALT_LEN = 8;

            var base64 = Convert.ToBase64String(bytes);
            var input = Convert.FromBase64String(base64);

            byte[] salt = new byte[PKCS5_SALT_LEN];
            byte[] msg = new byte[input.Length - magic.Length - PKCS5_SALT_LEN];
            Buffer.BlockCopy(input, magic.Length, salt, 0, salt.Length);
            Buffer.BlockCopy(input, magic.Length + PKCS5_SALT_LEN, msg, 0, msg.Length);

            var key = new byte[8];
            var IV = new byte[8];
            var pwd = Encoding.ASCII.GetBytes(password);

            var derived = Pbkdf2Sha256GetBytes(8, pwd, salt, 1000);

            key = derived;

            for (int i = 0; i < 8; i++) // or 16
            {
                IV[i] = derived[i];
            }

            using (var DES = new DESCryptoServiceProvider())
            {
                DES.IV = IV;
                DES.Key = key;
                DES.Mode = CipherMode.ECB;
                DES.Padding = PaddingMode.PKCS7;

                using (var memStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memStream, DES.CreateDecryptor(), CryptoStreamMode.Write);

                    cryptoStream.Write(msg, 0, msg.Length);
                    cryptoStream.FlushFinalBlock();
                    return memStream.ToArray();
                }
            }
        }

        private static byte[] Pbkdf2Sha256GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            using (var hmac = new HMACSHA256(password))
            {
                int hashLength = hmac.HashSize / 8;
                if ((hmac.HashSize & 7) != 0)
                    hashLength++;
                int keyLength = dklen / hashLength;
                if ((long) dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                    throw new ArgumentOutOfRangeException("dklen");
                if (dklen % hashLength != 0)
                    keyLength++;
                byte[] extendedkey = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
                using (var ms = new System.IO.MemoryStream())
                {
                    for (int i = 0; i < keyLength; i++)
                    {
                        extendedkey[salt.Length] = (byte) (((i + 1) >> 24) & 0xFF);
                        extendedkey[salt.Length + 1] = (byte) (((i + 1) >> 16) & 0xFF);
                        extendedkey[salt.Length + 2] = (byte) (((i + 1) >> 8) & 0xFF);
                        extendedkey[salt.Length + 3] = (byte) (((i + 1)) & 0xFF);
                        byte[] u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, salt.Length, 4);
                        byte[] f = u;
                        for (int j = 1; j < iterationCount; j++)
                        {
                            u = hmac.ComputeHash(u);
                            for (int k = 0; k < f.Length; k++)
                            {
                                f[k] ^= u[k];
                            }
                        }

                        ms.Write(f, 0, f.Length);
                        Array.Clear(u, 0, u.Length);
                        Array.Clear(f, 0, f.Length);
                    }

                    byte[] dk = new byte[dklen];
                    ms.Position = 0;
                    ms.Read(dk, 0, dklen);
                    ms.Position = 0;
                    for (long i = 0; i < ms.Length; i++)
                    {
                        ms.WriteByte(0);
                    }

                    Array.Clear(extendedkey, 0, extendedkey.Length);
                    return dk;
                }
            }
        }
    }
}