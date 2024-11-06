using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;
using System.Text.Json;

namespace SalamanderBank
{
	/*
	 * HOW TO USE THE DATABASE CLASS
	 *
	 * 1. Create an instance of the Database class, passing the database file path as a parameter
	 * 2. Call the InitializeDatabase method to set up the database and create the necessary tables
	 * 3. Use the connection string to establish a connection to the database
	 * 4. Execute SQL queries using the SQLiteConnection and SQLiteCommand classes
	 *
	 * Example usage:
	 *
	 * Database db = new Database("path/to/database.db");
	 * db.InitializeDatabase();
	 * db.AddUser(1, "password", "user@example.com", "John", "Doe");
	 */
	public static class Database
	{
		public static string _dbFile = "SalamanderBank.db";
		public static string _connectionString = $"Data Source={_dbFile};Version=3;";

		// Run this method to check if the database file and correct tables exist
		public static void InitializeDatabase()
		{
			// Check if the database file exists
			if (!File.Exists(_dbFile))
			{
				Console.WriteLine("Database file does not exist. Creating a new database file.");

				// Create and open a new database connection
				using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
				{
					connection.Open();

					// Optional: Create tables or add initial data here
					CreateTables(connection);

					Console.WriteLine("Database and table created successfully.");
				}
			}
			else
			{
				Console.WriteLine("Database file exists. Connecting to existing database.");

				// Open the connection to the existing database
				using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
				{
					connection.Open();
					CreateTables(connection);
					Console.WriteLine("Connected to the existing database.");
				}
			}
		}

		// Checks if the tables exist
		private static void CreateTables(SQLiteConnection connection)
		{
			string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY, type INTEGER, password TEXT, email TEXT NOT NULL UNIQUE, first_name TEXT, last_name TEXT, verified INTEGER);";
			using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Users table created.");
			}

			// currency_code = SEK/USD/EUR/NOK, type 0 = normal account, type 1 = loan
			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (id INTEGER PRIMARY KEY, user_id INTEGER, currency_code TEXT, account_name TEXT, balance REAL, status INTEGER, type INTEGER, interest REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Accounts table created.");
			}

			string createCurrenciesTableQuery = "CREATE TABLE IF NOT EXISTS Currencies (currency_code TEXT UNIQUE, exchange_rate REAL, last_updated TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createCurrenciesTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Currencies table created.");
			}

			string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (id INTEGER PRIMARY KEY, sender_user_id INTEGER, sender_account_id INTEGER, receiver_user_id INTEGER, receiver_account_id INTEGER, transfer_date TEXT, amount REAL, processed INTEGER);";
			using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Transfers table created.");
			}
		}

		// Adds a user
		public static void AddUser(int type, string password, string? email, string? firstName, string? lastName)
		{
			// Query to insert a new user
			string insertQuery = "INSERT INTO Users (type, password, email, first_name, last_name, verified) " +
								 "VALUES (@Type, @Password, @Email, @FirstName, @LastName, 0);";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				// If email doesn't exist, proceed with insertion
				using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
				{
					insertCommand.Parameters.AddWithValue("@Type", type);
					insertCommand.Parameters.AddWithValue("@Password", HashPassword(password));  // Make sure to hash passwords in production
					insertCommand.Parameters.AddWithValue("@Email", Escape(email));
					insertCommand.Parameters.AddWithValue("@FirstName", Escape(firstName));
					insertCommand.Parameters.AddWithValue("@LastName", Escape(lastName));

					int rowsAffected = insertCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
				}
			}
		}

		// Verifies a user by checking the email argument
		// Return 0 if failed, 1 if succeeded
		public static void VerifyUser(string? email)
		{
			string updateQuery = "UPDATE Users SET verified = 1 WHERE email = @Email;";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
				{
					updateCommand.Parameters.AddWithValue("@Email", Escape(email));

					int rowsAffected = updateCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
				}
			}
		}

		// Method that checks if an email already exists in the database
		// Returns true if the email exists, false otherwise
		public static bool EmailExists(string? email)
		{
			string query = "SELECT COUNT(*) FROM Users WHERE email = @Email;";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand command = new SQLiteCommand(query, connection))
				{
					command.Parameters.AddWithValue("@Email", Escape(email));

					long emailExists = (long)command.ExecuteScalar();

					return emailExists > 0;
				}
			}
		}

		// Searches for a user and returns an array user ids that have similar first name, last name and email address
		public static int[] SearchUser(string? searchTerm)
		{
			string searchQuery = "SELECT id FROM Users WHERE email LIKE %@search% OR first_name LIKE %@search% OR last_name LIKE %@search%;";
			List<int> ids = new List<int>();

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand command = new SQLiteCommand(searchQuery, connection))
				{
					// Bind parameters to prevent SQL injection
					command.Parameters.AddWithValue("@search", Escape(searchTerm));

					// Reads the search results
					using (SQLiteDataReader reader = command.ExecuteReader())
					{
						// Reads the next row in the current result
						while (reader.Read())
						{
							// Add each matching uid to the list
							ids.Add(Convert.ToInt32(reader["id"]));
						}
					}
				}
			}

			// Return the list as an array
			return ids.ToArray();
		}

		// Escapes strings for SQL LIKE queries
		public static string Escape(string? input)
		{
			return input
				.Replace("[", "\\[")
				.Replace("]", "\\]")
				.Replace("\\", "[\\]")
				.Replace("%", "[%]")
				.Replace("_", "[_]");
		}

		// Login method that accepts email and password as arguments
		public static object[] Login(string email, string password)
		{
			object[] userArray = null;

			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string query = "SELECT id, email, first_name, last_name FROM Users WHERE email = @Email AND password = @Password";
				using (var command = new SQLiteCommand(query, connection))
				{
					command.Parameters.AddWithValue("@Email", email);
					command.Parameters.AddWithValue("@Password", HashPassword(password));

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							// Adds ID, email address, fist name and last name to the array
							userArray = new object[]
							{
								reader.GetInt32(0),		// ID
								reader.GetString(1),		// Email address
								reader.GetString(2),		// First name
								reader.GetString(3)		// Last name
							};
						}
					}
				}
			}

			// Returns an array
			return userArray;
		}

		// Returns a hashed password that accepts an unhashed string password
		public static string HashPassword(string password)
		{
			var passwordHasher = new PasswordHasher<string>();
			string hashedPassword = passwordHasher.HashPassword(null, password); // 'null' is used as the user identifier here
			return hashedPassword;
		}

		// Accepts a hashed password and account ID, checks hashed password in SQLite
		// Returns true if it matches
		// Returns false if it doesn't match
		public static bool VerifyPassword(string password, int id)
		{
			string actualPassword = null;

			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string query = "SELECT password FROM Users WHERE id = @Id";
				using (var command = new SQLiteCommand(query, connection))
				{
					command.Parameters.AddWithValue("@Id", id);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							actualPassword = (string)reader.GetString(0);		// Password
						}
					}
				}
			}

			var passwordHasher = new PasswordHasher<string>();
			PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(null, HashPassword(password), actualPassword);

			return result == PasswordVerificationResult.Success;
		}

		// Accepts an email and a new password and updates the password in the database
		public static void ChangePassword(string? email, string newPassword)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string query = "UPDATE Users SET password = @NewPassword WHERE email = @Email";
				using (var command = new SQLiteCommand(query, connection))
				{
					command.Parameters.AddWithValue("@Email", Escape(email));
					command.Parameters.AddWithValue("@NewPassword", HashPassword(newPassword));

					int rowsAffected = command.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");
				}
			}
		}

		// A function that moves money from the sender's account to the Transfers table
		public static void TransferMoney(int senderUserId, int senderAccountId, int receiverUserId, int receiverAccountId, double amount)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();
				int rowsAffected;

				// Update the sender's account balance
				string updateSenderQuery = "UPDATE Accounts SET balance = balance - @Amount WHERE id = @SenderAccountId AND user_id = @SenderUserId;";
				using (var updateSenderCommand = new SQLiteCommand(updateSenderQuery, connection))
				{
					updateSenderCommand.Parameters.AddWithValue("@Amount", amount);
					updateSenderCommand.Parameters.AddWithValue("@SenderAccountId", senderAccountId);
					updateSenderCommand.Parameters.AddWithValue("@SenderUserId", senderUserId);

					rowsAffected = updateSenderCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Accounts table.");
				}

				if (rowsAffected > 0)
				{
					// Transfers the money from the account to the Transfers table
					string transferQuery = "INSERT INTO Transfers (sender_user_id, sender_account_id, receiver_user_id, receiver_account_id, transfer_date, amount) VALUES (@SenderUserId, @SenderAccountId, @ReceiverUserId, @ReceiverAccountId, @TransferDate, @Amount, 0);";
					using (var command = new SQLiteCommand(transferQuery, connection))
					{
						command.Parameters.AddWithValue("@SenderUserId", senderUserId);
						command.Parameters.AddWithValue("@SenderAccountId", senderAccountId);
						command.Parameters.AddWithValue("@ReceiverUserId", receiverUserId);
						command.Parameters.AddWithValue("@ReceiverAccountId", receiverAccountId);
						command.Parameters.AddWithValue("@TransferDate", DateTime.Now);
						command.Parameters.AddWithValue("@Amount", amount);

						int rows = command.ExecuteNonQuery();
						Console.WriteLine($"{rows} row(s) updated in Users table.");
					}
				}
			}
		}

		// A function that moves money from the Transfers table to the receiver's account
		public static void ProcessTransfer(int transferId)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				// Update the receiver's account balance
				string updateReceiverQuery = "UPDATE Accounts SET balance = balance + (SELECT amount FROM Transfers WHERE id = @TransferId) WHERE id = (SELECT receiver_account_id FROM Transfers WHERE id = @TransferId);";
				using (var updateReceiverCommand = new SQLiteCommand(updateReceiverQuery, connection))
				{
					updateReceiverCommand.Parameters.AddWithValue("@TransferId", transferId);

					int rowsAffected = updateReceiverCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Accounts table.");
				}

				// Transfers the money from the account to the Transfers table
				string transferQuery = "UPDATE Transfers SET processed = 1 WHERE id = @TransferId;";
				using (var command = new SQLiteCommand(transferQuery, connection))
				{
					command.Parameters.AddWithValue("@TransferId", transferId);

					int rows = command.ExecuteNonQuery();
					Console.WriteLine($"{rows} row(s) updated in Users table.");
				}
			}
		}

		// A function that creates a bank account for a user
		public static void CreateAccount(int userId, string currency_code, string accountName, double balance)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string insertQuery = "INSERT INTO Accounts (user_id, currency_code, account_name, balance) VALUES (@UserId, @CurrencyCode, @AccountName, @Balance);";
				using (var command = new SQLiteCommand(insertQuery, connection))
				{
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@CurrencyCode", currency_code);
					command.Parameters.AddWithValue("@AccountName", accountName);
					command.Parameters.AddWithValue("@Balance", balance);

					int rowsAffected = command.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Accounts table.");
				}
			}
		}
    }
}
