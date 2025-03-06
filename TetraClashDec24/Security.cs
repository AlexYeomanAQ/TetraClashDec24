using System;
using System.Security.Cryptography;
using System.Text;

namespace TetraClashDec24
{
    class Security
    {
        public static string GenerateSalt() //Random string
        {
            byte[] saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public static string GenerateHash(string password, string salt) //passwordRandom string
        {
            string saltedPassword = password + salt;
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashBytes); //hewdjslfwkeqoljfg
        }
    }
}
