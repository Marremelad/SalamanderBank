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
			string createTableQuery = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY, Name TEXT);";
			using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
			{
				command.ExecuteNonQuery();
			}
		}

		// You can add more methods to execute queries here
	}
}
