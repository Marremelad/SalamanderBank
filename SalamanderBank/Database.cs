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
	public class Database
	{
		private string _dbFile;
		private string _connectionString;

		public Database(string dbFile)
		{
			_dbFile = dbFile;
			_connectionString = $"Data Source={_dbFile};Version=3;";
		}

		// Run this method to check if the database file and correct tables exist
		public void InitializeDatabase()
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
					Console.WriteLine("Connected to the existing database.");
				}
			}
		}

		// Checks if the Users and Accounts tables exist
		private void CreateTables(SQLiteConnection connection)
		{
			string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY, type INTEGER, password TEXT, email TEXT NOT NULL UNIQUE, first_name TEXT, last_name TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Users table created.");
			}

			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (id INTEGER PRIMARY KEY, uid INTEGER, account_name TEXT, balance REAL, status INTEGER, type INTEGER, interest REAL);";
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
			string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (id INTEGER PRIMARY KEY, sender_user_id INTEGER, sender_account_id INTEGER, reciever_id INTEGER, reciever_account_id INTEGER, transfer_date DATETIME, amount REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Transfers table created.");
			}
		}

		// Adds a user
		public void AddUser(int type, string password, string email, string firstName, string lastName)
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

		/* Searches for users based on search term
		 * Searches while using a wild card on email, first name and last name
		 * int[] ids = db.SearchUser("John");
		 * if (ids.Length > 0)
		 * {
		 *		Console.WriteLine("Matching IDs:");
		 *		foreach (int id in ids)
		 *		{
		 *			Console.WriteLine(id);
		 *		}
		 * }
		 * else
		 * {
		 *		Console.WriteLine("No user found with the provided search criteria.");
		 * }
		 */
		public int[] SearchUser(string searchTerm)
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
	}
}
