using EllipsePaymentService.Models;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.IO;
using System;

namespace EllipsePaymentService.Services
{
    public class SecureService
    {
        private readonly  string _salt;
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
        public SecureService(IConfiguration configuration)
        {
            _salt = configuration.GetValue<string>("Salt");
        }

        public bool IsSignValid(string status, string orderid, string client_oederid, string control, PaymentMethod method)
        {
           string input = string.Concat(new string[]
           {
                status,
                orderid,
                client_oederid,
                method.Secret
           });
           return control == GetSha1(input);
        }

        public string GenerateSign(string orderId, string amount, string email, PaymentMethod method)
        {
            Console.WriteLine($"sign amount input: {amount}");
            var _endpoint = method.Url.Split('/').Last();
            float amount_float = 0;
            try
            {

                amount_float = Convert.ToSingle(amount);
            }
            catch (Exception)
            {

                amount_float = Convert.ToSingle(amount.Replace('.', ','));
            }
            Console.WriteLine($"sign amount_float input: {amount_float}");
            var amount_in_minimal = (int)Math.Round(amount_float * 100);
            Console.WriteLine($"sign amount_in_minimal input: {amount_in_minimal}");
            string input = string.Concat(new string[]
            {
                _endpoint,
                orderId,
                amount_in_minimal.ToString(),
                email,
                method.Secret
            });
            Console.WriteLine($"sign string: {input}");
            return GetSha1(input);
        }        

        private string GetSha1(string input)
        {
            string result;
            using (SHA1Managed sha = new SHA1Managed())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                result = sb.ToString();
            }
            return result;
        }
        public string EncryptString(string plainText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_salt, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    plainText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return plainText;
        }

        public string DecryptString(string encrypted) 
        {
            encrypted = encrypted.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encrypted);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_salt, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    encrypted = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return encrypted;
        }

        public string GetPasswordHash(string password)
        {
            var message = Encoding.ASCII.GetBytes(password);
            var hashString = new SHA256Managed();
            var hex = "";

            var hashValue = hashString.ComputeHash(message);
            foreach (var x in hashValue)
                hex += string.Format("{0:x2}", x);
            return hex;
        }
    }
}
