using System.Data.SQLite;

namespace SalamanderBank
{
    public static class Db
    {
        public static string DbFile = "SalamanderBank.db";
        public static string ConnectionString = $"Data Source={DbFile};Version=3;";

        // Checks if the database file exists and initializes it if necessary
        public static void InitializeDatabase()
        {
            // Check if the database file exists
            if (!File.Exists(DbFile))
            {
                Console.WriteLine("Database file does not exist. Creating a new database file.");

                // Create and open a new database connection
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Create tables if the database is new
                    CreateTables(connection);

                    Console.WriteLine("Database and table created successfully.");
                }
            }
            else
            {
                Console.WriteLine("Database file exists. Connecting to existing database.");

                // Open the connection to the existing database
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    CreateTables(connection);
                    Console.WriteLine("Connected to the existing database.");
                }
            }
        }

        // Creates necessary tables if they don't exist
        private static void CreateTables(SQLiteConnection connection)
        {
            // SQL query to create the Users table
            string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (ID INTEGER PRIMARY KEY, Type INTEGER, Password TEXT, Email TEXT NOT NULL UNIQUE, FirstName TEXT, LastName TEXT, PhoneNumber TEXT, Verified INTEGER, Locked INTEGER);";
            using (SQLiteCommand command = new SQLiteCommand(createUsersTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Users table created.");
            }

            // SQL query to create the Accounts table
            string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (ID INTEGER PRIMARY KEY, UserID INTEGER, CurrencyCode TEXT, AccountName TEXT, Balance REAL, Status INTEGER, Type INTEGER, Interest REAL);";
            using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Accounts table created.");
            }

            // SQL query to create the Currencies table
            string createCurrenciesTableQuery = "CREATE TABLE IF NOT EXISTS Currencies (CurrencyCode TEXT PRIMARY KEY, ExchangeRate REAL, LastUpdated TEXT);";
            using (SQLiteCommand command = new SQLiteCommand(createCurrenciesTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Currencies table created.");
            }

            // SQL query to create the Transfers table
            string createTransfersTableQuery = "CREATE TABLE IF NOT EXISTS Transfers (ID INTEGER PRIMARY KEY, SenderUserID INTEGER, SenderAccountID INTEGER, ReceiverUserID INTEGER, ReceiverAccountID INTEGER, CurrencyCode TEXT, TransferDate TEXT, Amount REAL, Processed INTEGER);";
            using (SQLiteCommand command = new SQLiteCommand(createTransfersTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Transfers table created.");
            }

            // SQL query to create the Loans table
            string createLoanTableQuery = "CREATE TABLE IF NOT EXISTS Loans (ID INTEGER PRIMARY KEY, UserID INTEGER, Amount REAL, CurrencyCode TEXT, InterestRate REAL, Status INTEGER, LoanDate TEXT);";
            using (SQLiteCommand command = new SQLiteCommand(createLoanTableQuery, connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Loans table created.");
            }
        }

        // Escapes strings for SQL LIKE queries to prevent SQL injection
        public static string? Escape(string? input)
        {
            return input?.Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("\\", "[\\]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}
