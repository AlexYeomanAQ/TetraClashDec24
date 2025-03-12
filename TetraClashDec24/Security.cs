using System;
using System.Security.Cryptography;
using System.Text;

namespace TetraClashDec24
{
    class Security
    {
        // Generates a cryptographically secure random salt.
        // The salt is a random byte array of 16 bytes, then encoded to a Base64 string for easy storage.
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16]; // Create a byte array to hold 16 random bytes.
            
            RandomNumberGenerator.Fill(saltBytes); // Fill the byte array with cryptographically strong random bytes.
            
            return Convert.ToBase64String(saltBytes); // Fill the byte array with cryptographically strong random bytes.
        }

        // Generates a SHA256 hash for the given password and salt.
        // The salt is appended to the password to increase security, preventing precomputed attacks.
        public static string GenerateHash(string password, string salt) // password + random string
        {
            string saltedPassword = password + salt; // Combine the password and salt. This ensures that the same password will hash differently for each user.
            
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword)); // Convert the combined string into a byte array using UTF8 encoding, then compute the SHA256 hash of this byte array.
            
            return Convert.ToBase64String(hashBytes); // Convert the hash bytes into a Base64 encoded string, which is easier to store and compare.
        }
    }
}
