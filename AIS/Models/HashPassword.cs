using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AIS.Models
{
    public class HashPassword
    {
        public static string GetHashPAssword(string Password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] sourceBytePassword = Encoding.UTF8.GetBytes(Password);
                byte[] hashSourceBytePassword = sha256.ComputeHash(sourceBytePassword);
                string hashPassword = BitConverter.ToString(hashSourceBytePassword).Replace("-", String.Empty);
                return hashPassword;
            }
        }
    }
}