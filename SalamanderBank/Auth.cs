using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace SalamanderBank
{
    public class Auth
    {
        // Login method that accepts email and password as arguments
        public static User? Login(string? email, string? password)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Users WHERE Email = @Email";
                var user = connection.QuerySingleOrDefault<User>(sql, new { Email = email });

                if (VerifyPassword(password, user))
                {
                    Console.WriteLine("Login successful.");
                    AccountManager.GetAccountsFromUser(user);
                    LoanManager.GetLoansFromUser(user);
                    return user;
                }
                else
                {
                    Console.WriteLine("Login failed: No user found with matching email and password.");
                }
                return null;
            }
        }

        // Returns a hashed password that accepts an unhashed string password
        public static string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<string>();
            string hashedPassword = passwordHasher.HashPassword(null, password); // 'null' is used as the user identifier here
            return hashedPassword;
        }

        // Accepts a password and user object, checks hashed password in SQLite
        // Returns true if it matches
        // Returns false if it doesn't match
        public static bool VerifyPassword(string? password, User user)
        {
            var passwordHasher = new PasswordHasher<string>();
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(null, user.Password, password);

            return result == PasswordVerificationResult.Success;
        }
    }
}
