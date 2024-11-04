using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

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
	 * db.AddUser(1001, 1, "hashed_password", "user@example.com", "John", "Doe");
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

		// Checks if the Users and Accounts tables exist
		private static void CreateTables(SQLiteConnection connection)
		{
			string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY, type INTEGER, password TEXT, email TEXT NOT NULL UNIQUE, first_name TEXT, last_name TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Users table created.");
			}

			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (id INTEGER PRIMARY KEY, user_id INTEGER, account_name TEXT, balance REAL, status INTEGER, type INTEGER, interest REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Accounts table created.");
			}

			string createCurrenciesTableQuery = "CREATE TABLE IF NOT EXISTS Currencies (id INTEGER PRIMARY KEY, currency_code TEXT, value_in_SEK REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createCurrenciesTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Currencies table created.");
			}

			string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (id INTEGER PRIMARY KEY, sender_user_id INTEGER, sender_account_id INTEGER, reciever_user_id INTEGER, reciever_account_id INTEGER, transfer_date DATETIME, amount REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Transfers table created.");
			}
		}

		// Adds a user
		public static void AddUser(int type, string password, string email, string firstName, string lastName)
		{
			// This query will insert a user into the Users table, based on the arguments in the method
			string insertQuery = "INSERT INTO Users (type, password, email, first_name, last_name) " +
								 "VALUES (@type, @password, @email, @first_name, @last_name);";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
				{
					// Bind parameters to prevent SQL injection
					command.Parameters.AddWithValue("@type", type);
					command.Parameters.AddWithValue("@password", EscapeForLike(password));  // Be sure to hash passwords in production
					command.Parameters.AddWithValue("@email", EscapeForLike(email));
					command.Parameters.AddWithValue("@first_name", EscapeForLike(firstName));
					command.Parameters.AddWithValue("@last_name", EscapeForLike(lastName));

					int rowsAffected = command.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
				}
			}
		}

		// Searches for a user and returns an array user ids that have similar first name, last name and email address
		public static int[] SearchUser(string searchTerm)
		{
			string searchQuery = "SELECT id FROM Users WHERE email LIKE %@search% OR first_name LIKE %@search% OR last_name LIKE %@search%;";
			List<int> ids = new List<int>();

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand command = new SQLiteCommand(searchQuery, connection))
				{
					// Bind parameters to prevent SQL injection
					command.Parameters.AddWithValue("@search", EscapeForLike(searchTerm));

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
		public static string EscapeForLike(string input)
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
					command.Parameters.AddWithValue("@Password", password);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							// Adds ID, email address, fist name and last name to the array
							userArray = new object[]
							{
							reader.GetInt32(0),        // ID
                            reader.GetString(1),       // Email address
                            reader.GetString(2),       // First name
                            reader.GetString(3)        // Last name
							};
						}
					}
				}
			}

			// Returns an array
			return userArray;
		}
	}
}
