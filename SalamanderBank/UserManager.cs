using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    public class UserManager
    {

        // Adds a user
        public static User? AddUser(int type, string password, string? email, string? firstName, string? lastName, string? phoneNumber = null, int verified = 0)
        {
            // Check if the email already exists
            if (!EmailExists(email))
            {
                // Query to insert a new user
                string insertQuery = @"
                    INSERT INTO Users (Type, Password, Email, FirstName, LastName, PhoneNumber, Verified, Locked) 
                    VALUES (@Type, @Password, @Email, @FirstName, @LastName, @PhoneNumber, @Verified, 0);
                    SELECT * FROM Users WHERE Id = last_insert_rowid();";

                using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
                {
                    connection.Open();

                    // If email doesn't exist, proceed with insertion
                    var parameters = new
                    {
                        Type = type,
                        Password = Auth.HashPassword(password),
                        Email = DB.Escape(email),
                        FirstName = DB.Escape(firstName),
                        LastName = DB.Escape(lastName),
                        PhoneNumber = phoneNumber == null ? null : DB.Escape(phoneNumber),
                        Verified = verified
                    };
                    User user = connection.QuerySingle<User>(insertQuery, parameters);
                    return user;
                }   
            }
            return null;
        }

        // Verifies a user by checking the email argument
        // Return 0 if failed, 1 if succeeded
        public static void VerifyUser(string? email)
        {
            string updateQuery = "UPDATE Users SET Verified = 1 WHERE Email = @Email;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Email", DB.Escape(email));

                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
                }
            }
        }


        // Method that checks if an email already exists in the database
        // Returns true if the email exists, false otherwise
        public static bool EmailExists(string? email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", DB.Escape(email));

                    long emailExists = (long)command.ExecuteScalar();

                    return emailExists > 0;
                }
            }
        }
        public static bool PhoneNumberExists(string? phoneNumber)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE PhoneNumber = @PhoneNumber;";
            int count = 0;
            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                count = connection.ExecuteScalar<int>(query, new {PhoneNumber = phoneNumber});
                
            }
            return count > 0;
        }

        // Searches for a user and returns an array user ids that have similar first name, last name and email address
        public static List<User> SearchUser(string? searchTerm)
        {
            string searchQuery = @"SELECT * FROM Users
                WHERE Email LIKE @search OR FirstName LIKE @search OR LastName LIKE @search;";
            var parameters = new { search = $"%{searchTerm}%" };


            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                List<User> users = connection.Query<User>(searchQuery, parameters).ToList();
                foreach(User user in users)
                {
                    AccountManager.GetAccountsFromUser(user);
                }
                return users;
            }
        }

		// Updates user password
        public static void ChangePassword(string? email, string newPassword)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                string query = "UPDATE Users SET Password = @NewPassword WHERE Email = @Email";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", DB.Escape(email));
                    command.Parameters.AddWithValue("@NewPassword", Auth.HashPassword(newPassword));

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
                }
            }
        }

		// Accepts a User object and a new password
		// Updates the User object's password
        // Accepts an email and a new password and updates the password in the database
		public static void UpdateUserPassword(User user, string newPassword)
        {
            string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE ID = @UserID;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                {
                    string newHashedPassword = Auth.HashPassword(newPassword);
                    

					updateCommand.Parameters.AddWithValue("@NewPassword", newHashedPassword);
                    updateCommand.Parameters.AddWithValue("@UserID", user.ID);

                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");

					// Checks if account successfully updated
					if (rowsAffected > 0)
					{
						// Updates the User object's password
						user.Password = newHashedPassword;
					}
				}
            }
        }

        // Updates user phone number
        // Accepts a User object and a new phone number
        public static void UpdateUserPhoneNumber(User user, string newPhoneNumber)
        {
            string updateQuery = "UPDATE Users SET PhoneNumber = @NewPhoneNumber WHERE ID = @UserID;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@NewPhoneNumber", DB.Escape(newPhoneNumber));
                    updateCommand.Parameters.AddWithValue("@UserID", user.ID);

                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
					
                    // Checks if account successfully updated
					if (rowsAffected > 0)
                    {
						// Updates the User object's phone number
						user.PhoneNumber = newPhoneNumber;
					}
                }
            }
        }

        // Updates user email
        // Accepts a User object and a new email
        public static void UpdateUserEmail(User user, string newEmail)
        {
            string updateQuery = "UPDATE Users SET Email = @NewEmail WHERE ID = @UserID;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@NewEmail", DB.Escape(newEmail));
                    updateCommand.Parameters.AddWithValue("@UserID", user.ID);

                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");

                    // Checks if account successfully updated
                    if (rowsAffected > 0)
                    {
						// Updates the User object's email
						user.Email = newEmail;
					}
                }
            }
        }

		// Changes User object's locked column in database
		public static void UpdateUserLock(string? email, int locked)
		{
			string updateQuery = "UPDATE Users SET Locked = @NewLocked WHERE Email = @Email;";

			using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
			{
				connection.Open();

				using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
				{
					updateCommand.Parameters.AddWithValue("@NewLocked", locked);
					updateCommand.Parameters.AddWithValue("@Email", email);

					int rowsAffected = updateCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
				}
			}
		}
	}
}
