using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace SalamanderBank
{
    public class UserManager
    {

        // Adds a user
        public static void AddUser(int type, string password, string? email, string? firstName, string? lastName, string phoneNumber = null)
        {
            // Check if the email already exists
            if (!EmailExists(email))
            {
                // Query to insert a new user
                string insertQuery = "INSERT INTO Users (Type, Password, Email, FirstName, LastName, PhoneNumber, Verified) " +
                                                "VALUES (@Type, @Password, @Email, @FirstName, @LastName, @PhoneNumber, 0);";

                using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
                {
                    connection.Open();

                    // If email doesn't exist, proceed with insertion
                    using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Type", type);
                        insertCommand.Parameters.AddWithValue("@Password", Auth.HashPassword(password));  // Make sure to hash passwords in production
                        insertCommand.Parameters.AddWithValue("@Email", DB.Escape(email));
                        insertCommand.Parameters.AddWithValue("@FirstName", DB.Escape(firstName));
                        insertCommand.Parameters.AddWithValue("@LastName", DB.Escape(lastName));

                        // Set @PhoneNumber to NULL if phoneNumber is null
                        if (phoneNumber == null)
                            insertCommand.Parameters.AddWithValue("@PhoneNumber", DBNull.Value);
                        else
                            insertCommand.Parameters.AddWithValue("@PhoneNumber", DB.Escape(phoneNumber));

                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
                    }
                }
            }
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

        // Searches for a user and returns an array user ids that have similar first name, last name and email address
        public static int[] SearchUser(string? searchTerm)
        {
            string searchQuery = "SELECT ID FROM Users WHERE Email LIKE %@search% OR FirstName LIKE %@search% OR LastName LIKE %@search%;";
            List<int> ids = new List<int>();

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(searchQuery, connection))
                {
                    // Bind parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@search", DB.Escape(searchTerm));

                    // Reads the search results
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Reads the next row in the current result
                        while (reader.Read())
                        {
                            // Add each matching uid to the list
                            ids.Add(Convert.ToInt32(reader["ID"]));
                        }
                    }
                }
            }
            // Return the list as an array
            return ids.ToArray();
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
    }
}
