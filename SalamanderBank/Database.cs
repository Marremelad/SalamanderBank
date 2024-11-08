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
using Dapper;

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
			string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (ID INTEGER PRIMARY KEY, Type INTEGER, Password TEXT, Email TEXT NOT NULL UNIQUE, FirstName TEXT, LastName TEXT, Telephone TEXT, Verified INTEGER);";
			using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Users table created.");
			}

			// currency_code = SEK/USD/EUR/NOK, type 0 = normal account, type 1 = loan
			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (ID INTEGER PRIMARY KEY, UserID INTEGER, CurrencyCode TEXT, AccountName TEXT, Balance REAL, Status INTEGER, Type INTEGER, Interest REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Accounts table created.");
			}

			string createCurrenciesTableQuery = "CREATE TABLE IF NOT EXISTS Currencies (CurrencyCode TEXT PRIMARY KEY, ExchangeRate REAL, LastUpdated TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createCurrenciesTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Currencies table created.");
			}

			string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (ID INTEGER PRIMARY KEY, SenderUserID INTEGER, SenderAccountID INTEGER, ReceiverUserID INTEGER, ReceiverAccountID INTEGER, TransferDate TEXT, Amount REAL, Processed INTEGER);";
			using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Transfers table created.");
			}
		}

		// Adds a user
		public static void AddUser(int type, string password, string? email, string? firstName, string? lastName, string? telephone)
		{
			// Query to insert a new user
			string insertQuery = "INSERT INTO Users (Type, Password, Email, FirstName, LastName, Telephone, Verified) " +
								 "VALUES (@Type, @Password, @Email, @FirstName, @LastName, @Telephone, 0);";

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
					insertCommand.Parameters.AddWithValue("@Telephone", Escape(telephone));
					int rowsAffected = insertCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
				}
			}
		}

		// Verifies a user by checking the email argument
		// Return 0 if failed, 1 if succeeded
		public static void VerifyUser(string? email)
		{
			string updateQuery = "UPDATE Users SET Verified = 1 WHERE Email = @Email;";

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
			string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email;";

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
			string searchQuery = "SELECT ID FROM Users WHERE Email LIKE %@search% OR FirstName LIKE %@search% OR LastName LIKE %@search%;";
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
							ids.Add(Convert.ToInt32(reader["ID"]));
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
		public static User? Login(string? email, string? password)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string sql = "SELECT * FROM Users WHERE Email = @Email";
				var user = connection.QuerySingleOrDefault<User>(sql, new { Email = email });

				if (VerifyPassword(password, user))
				{
					Console.WriteLine("Login successful.");
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
		public static bool VerifyPassword(string? password, User? user)
		{
			var passwordHasher = new PasswordHasher<string>();
			PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(null, user.Password, password);

			return result == PasswordVerificationResult.Success;
		}

		// Accepts an email and a new password and updates the password in the database
		public static void ChangePassword(string? email, string newPassword)
		{
			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				string query = "UPDATE Users SET Password = @NewPassword WHERE Email = @Email";
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
				string updateSenderQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE ID = @SenderAccountId AND UserID = @SenderUserId;";
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
					string transferQuery = "INSERT INTO Transfers (SenderUserID, SenderAccountID, ReceiverUserID, ReceiverAccountID, TransferDate, Amount) VALUES (@SenderUserId, @SenderAccountId, @ReceiverUserId, @ReceiverAccountId, @TransferDate, @Amount, 0);";
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
				string updateReceiverQuery = "UPDATE Accounts SET Balance = Balance + (SELECT Amount FROM Transfers WHERE ID = @TransferId) WHERE ID = (SELECT ReceiverAccountID FROM Transfers WHERE ID = @TransferId);";
				using (var updateReceiverCommand = new SQLiteCommand(updateReceiverQuery, connection))
				{
					updateReceiverCommand.Parameters.AddWithValue("@TransferId", transferId);

					int rowsAffected = updateReceiverCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Accounts table.");
				}

				// Transfers the money from the account to the Transfers table
				string transferQuery = "UPDATE Transfers SET Processed = 1 WHERE ID = @TransferId;";
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

				string insertQuery = "INSERT INTO Accounts (UserID, CurrencyCode, AccountName, Balance) VALUES (@UserId, @CurrencyCode, @AccountName, @Balance);";
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
