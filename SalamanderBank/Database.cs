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

		private void CreateTables(SQLiteConnection connection)
		{
			string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY, Uid INTEGER, Type INTEGER, Password TEXT, Email TEXT, FirstName TEXT, LastName TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Users table created.");
			}

			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (Id INTEGER PRIMARY KEY, Uid INTEGER, AccountName TEXT, Balance INTEGER, Status INTEGER);";
			using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Accounts table created.");
			}
		}

		public void AddUser(int uid, int type, string password, string email, string firstName, string lastName)
		{
			string insertQuery = "INSERT INTO Users (Uid, Type, Password, Email, FirstName, LastName) " +
								 "VALUES (@Uid, @Type, @Password, @Email, @FirstName, @LastName);";

			using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
				{
					// Bind parameters to prevent SQL injection
					command.Parameters.AddWithValue("@Uid", uid);
					command.Parameters.AddWithValue("@Type", type);
					command.Parameters.AddWithValue("@Password", password);  // Be sure to hash passwords in production
					command.Parameters.AddWithValue("@Email", email);
					command.Parameters.AddWithValue("@FirstName", firstName);
					command.Parameters.AddWithValue("@LastName", lastName);

					int rowsAffected = command.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) inserted into Users table.");
				}
			}
		}

		// You can add more methods to execute queries here
	}
}
