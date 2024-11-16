using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
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
    public static class DB
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
            string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (ID INTEGER PRIMARY KEY, Type INTEGER, Password TEXT, Email TEXT NOT NULL UNIQUE, FirstName TEXT, LastName TEXT, PhoneNumber TEXT, Verified INTEGER, Locked INTEGER);";
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

            string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (ID INTEGER PRIMARY KEY, SenderUserID INTEGER, SenderAccountID INTEGER, ReceiverUserID INTEGER, ReceiverAccountID INTEGER, CurrencyCode TEXT, TransferDate TEXT, Amount REAL, Processed INTEGER);";
            using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Transfers table created.");
            }

            string createLoanTableQuery = "CREATE TABLE IF NOT EXISTS Loans (ID INTEGER PRIMARY KEY, UserID INTEGER, Amount REAL, CurrencyCode TEXT, InterestRate REAL, Status INTEGER, LoanDate TEXT);";
            using (SQLiteCommand command = new SQLiteCommand(createLoanTableQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Loans table created.");
            }
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
    }
}
