using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DemoAppDatabase.Services
{
    public static class EncryptionService
    {
        private const int Keysize = 256;
        private const int BlockSize = 128;
        private const int DerivationIterations = 100000;

        private const string _defaultSalt = "6b4m62¤#6b293%/2572#¤&";
        public static string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate salt, IV, and HMAC key
            var saltBytes = GenerateRandomEntropy(32); // 256 bits
            var ivBytes = GenerateRandomEntropy(16); // 128 bits
            var hmacKey = GenerateRandomEntropy(32); // 256 bits HMAC key

            using (var aes = Aes.Create())
            {
                var keyBytes = DeriveKeyFromPassword(password, saltBytes);
                aes.BlockSize = BlockSize;
                aes.KeySize = Keysize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();

                    // Get encrypted data
                    var cipherTextBytes = memoryStream.ToArray();

                    // Compute HMAC
                    var hmacBytes = ComputeHMAC(cipherTextBytes, hmacKey);

                    // Combine salt, IV, HMAC key, HMAC, and ciphertext
                    var finalBytes = saltBytes
                        .Concat(ivBytes)
                        .Concat(hmacKey)
                        .Concat(hmacBytes)
                        .Concat(cipherTextBytes)
                        .ToArray();

                    return Convert.ToBase64String(finalBytes);
                }
            }
        }

        public static string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var cipherTextBytesWithSaltAndIvAndHmac = Convert.FromBase64String(cipherText);

            // Extract salt, IV, HMAC key, HMAC, and ciphertext
            var saltBytes = cipherTextBytesWithSaltAndIvAndHmac.Take(32).ToArray();
            var ivBytes = cipherTextBytesWithSaltAndIvAndHmac.Skip(32).Take(16).ToArray();
            var hmacKey = cipherTextBytesWithSaltAndIvAndHmac.Skip(48).Take(32).ToArray();
            var hmacBytes = cipherTextBytesWithSaltAndIvAndHmac.Skip(80).Take(32).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIvAndHmac.Skip(112).ToArray();

            // Verify HMAC
            var computedHmacBytes = ComputeHMAC(cipherTextBytes, hmacKey);
            if (!computedHmacBytes.SequenceEqual(hmacBytes))
                throw new CryptographicException("HMAC verification failed. The data may have been tampered with.");

            using (var aes = Aes.Create())
            {
                var keyBytes = DeriveKeyFromPassword(password, saltBytes);
                aes.BlockSize = BlockSize;
                aes.KeySize = Keysize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
                using (var memoryStream = new MemoryStream(cipherTextBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static byte[] GenerateRandomEntropy(int length)
        {
            var randomBytes = new byte[length];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        private static byte[] DeriveKeyFromPassword(string password, byte[] saltBytes)
        {
            using (var passwordDeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, DerivationIterations))
            {
                return passwordDeriveBytes.GetBytes(Keysize / 8);
            }
        }

        private static byte[] ComputeHMAC(byte[] data, byte[] key)
        {
            using (var hmacsha256 = new HMACSHA256(key))
            {
                return hmacsha256.ComputeHash(data);
            }
        }

        public static string ComputeSha256Hash(string rawData, bool useDefaultSalt = false)
        {
            if (useDefaultSalt)
            {
                rawData += _defaultSalt;
            }

            // Create a SHA256 instance
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }

}
