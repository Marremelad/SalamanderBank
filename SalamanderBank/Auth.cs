using System.Data.SQLite;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace SalamanderBank
{
    public class Auth
    {
        // Login method that accepts email and password as arguments
        // Verifies the credentials against the database
        public static User? Login(string? email, string? password)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Users WHERE Email = @Email"; // Retrieves the user from the database by email
                var user = connection.QuerySingleOrDefault<User>(sql, new { Email = email });

                if (VerifyPassword(password, user)) // Verifies the password against the stored hashed password
                {
                    Console.WriteLine("Login successful.");
                    AccountManager.GetAccountsFromUser(user); // Fetches the accounts associated with the user
                    LoanManager.GetLoansFromUser(user); // Fetches the loans associated with the user
                    return user; // Returns the user object if the login is successful
                }
                else
                {
                    Console.WriteLine("Login failed: No user found with matching email and password.");
                }
                return null; // Returns null if the login fails
            }
        }

        // Hashes a password string using PasswordHasher
        // Accepts an unhashed string password and returns a hashed version
        public static string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<string?>();
            string hashedPassword = passwordHasher.HashPassword(null, password); // 'null' is used as the user identifier here
            return hashedPassword; // Returns the hashed password
        }

        // Verifies the password by comparing the given plain password with the stored hashed password
        // Returns true if the password matches, otherwise returns false
        public static bool VerifyPassword(string? password, User? user)
        {
            var passwordHasher = new PasswordHasher<string?>();
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(null, user?.Password, password); // Compares the hashed password

            return result == PasswordVerificationResult.Success; // Returns true if the verification is successful
        }
    }
}
