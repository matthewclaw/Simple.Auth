﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Auth.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Generates a SHA256 hash of the input string.
        /// PLEASE NOTE: This does not use a salt and so is NOT recommended for things like passwords.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>A hexadecimal string representation of the SHA256 hash. Returns an empty string if the input is null or empty.</returns>
        public static string GenerateBasicHash(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (var sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // Compute the hash
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2")); // "x2" formats as lowercase hex with two digits
                }
                return sb.ToString();
            }
        }
    }
}

