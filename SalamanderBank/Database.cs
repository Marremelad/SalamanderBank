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
		// Returns 0 if email address is already in use and 1 if it was successful
		public static int AddUser(int type, string password, string email, string firstName, string lastName)
		{
			// Query to check if an account with the same email already exists
			string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE email = @Email;";

			// Query to insert a new user
			string insertQuery = "INSERT INTO Users (type, password, email, first_name, last_name, verified) " +
								 "VALUES (@Type, @Password, @Email, @FirstName, @LastName, 0);";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				// First, check if the email already exists
				using (SQLiteCommand checkCommand = new SQLiteCommand(checkEmailQuery, connection))
				{
					checkCommand.Parameters.AddWithValue("@Email", email);
					long emailExists = (long)checkCommand.ExecuteScalar();

					if (emailExists > 0)
					{
						// Email already exists, return 0
						return 0;
					}
				}

				// If email doesn't exist, proceed with insertion
				using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
				{
					insertCommand.Parameters.AddWithValue("@Type", type);
					insertCommand.Parameters.AddWithValue("@Password", EscapeForLike(password));  // Make sure to hash passwords in production
					insertCommand.Parameters.AddWithValue("@Email", EscapeForLike(email));
					insertCommand.Parameters.AddWithValue("@FirstName", EscapeForLike(firstName));
					insertCommand.Parameters.AddWithValue("@LastName", EscapeForLike(lastName));

					int rowsAffected = insertCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
				}
			}

			// Return 1 to indicate success
			return 1;
		}

		// Verifies a user by checking the email argument
		// Return 0 if failed, 1 if succeeded
		public static int VerifyUser(string email)
		{
			string updateQuery = "UPDATE Users SET verified = 1 WHERE id = @Email;";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand updateCommand = new SQLiteCommand(updateQuery, connection))
				{
					updateCommand.Parameters.AddWithValue("@Email", email);

					int rowsAffected = updateCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");

					if (rowsAffected > 0) { return 0; }
				}
			}

			// Return 1 to indicate success
			return 1;
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
