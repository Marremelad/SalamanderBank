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

			string createAccountsTableQuery = "CREATE TABLE IF NOT EXISTS Accounts (id INTEGER PRIMARY KEY, user_id INTEGER, account_name TEXT, balance REAL, status INTEGER, type INTEGER, interest REAL);";
			using (SQLiteCommand command = new SQLiteCommand(createAccountsTableQuery, connection))
			{
				int rowsAffected = command.ExecuteNonQuery();
				Console.WriteLine($"Accounts table created.");
			}

			string createCurrenciesTableQuery = "CREATE TABLE IF NOT EXISTS Currencies (id INTEGER PRIMARY KEY, currency_code TEXT UNIQUE, value_in_SEK REAL);";
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
					insertCommand.Parameters.AddWithValue("@Password", HashPassword(password));  // Make sure to hash passwords in production
					insertCommand.Parameters.AddWithValue("@Email", Escape(email));
					insertCommand.Parameters.AddWithValue("@FirstName", Escape(firstName));
					insertCommand.Parameters.AddWithValue("@LastName", Escape(lastName));

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
					updateCommand.Parameters.AddWithValue("@Email", Escape(email));

					int rowsAffected = updateCommand.ExecuteNonQuery();
					Console.WriteLine($"{rowsAffected} row(s) updated in Users table.");

					if (rowsAffected == 0) { return 0; }
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
		public static string Escape(string input)
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
		public static bool VerifyPassword(string hashedPassword, int id)
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
			PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(null, hashedPassword, actualPassword);

			return result == PasswordVerificationResult.Success;
		}

		// Accepts an email and a new password and updates the password in the database
		public static void ChangePassword(string email, string newPassword)
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

        public static async Task UpdateCurrenciesAsync()
        {

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {

                connection.Open();


                // Retrieve the latest update time from the database
                string lastUpdateQuery = "SELECT MAX(last_updated) FROM Currencies;";

                DateTime lastUpdated = DateTime.MinValue;

                using (SQLiteCommand command = new SQLiteCommand(lastUpdateQuery, connection))
                {

                    var result = command.ExecuteScalar();

                    if (result != DBNull.Value)
                    {

                        lastUpdated = Convert.ToDateTime(result);
                    }
                }
                // Check if more than 24 hours have passed since the last update
                TimeSpan timeSinceLastUpdate = DateTime.Now - lastUpdated;



                if (timeSinceLastUpdate.TotalHours < 24)
                {
                    // Notify that no update is needed
                    DateTime nextUpdate = lastUpdated.AddHours(24);
                    Console.WriteLine($"No update has been made to the Currencies Database.");
                    Console.WriteLine($"The next update will occur on {nextUpdate:yyyy-MM-dd} at {nextUpdate:HH:mm}.");
                    return;
                }

                // Make API call to fetch current exchange rates
                // Taken from "https://currencyapi.com/"

                Env.Load("./Credentials.env");
                string apiKey = Env.GetString("CURRENCY_API_KEY");
                string url = $"https://api.currencyapi.com/v3/latest?apikey={apiKey}&currencies=&base_currency=SEK";

                using (HttpClient client = new HttpClient())
                {

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement data = document.RootElement.GetProperty("data");
                            using (SQLiteTransaction transaction = connection.BeginTransaction())
                            {
                                foreach (JsonProperty currency in data.EnumerateObject())
                                {
                                    string currencyCode = currency.Name;

                                    decimal exchangeRate = currency.Value.GetProperty("value").GetDecimal();
                                    string upsertQuery = @"

                                    INSERT INTO Currencies (currency_code, exchange_rate, last_updated)

                                    VALUES (@currencyCode, @exchangeRate, @lastUpdated)

                                    ON CONFLICT(currency_code) 

                                    DO UPDATE SET exchange_rate = @exchangeRate, last_updated = @lastUpdated;";

                                    
                                    using (SQLiteCommand command = new SQLiteCommand(upsertQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@currencyCode", currencyCode);

                                        command.Parameters.AddWithValue("@exchangeRate", exchangeRate);

                                        command.Parameters.AddWithValue("@lastUpdated", DateTime.Now);

                                        command.ExecuteNonQuery();

                                    }
                                }
                                transaction.Commit();
                            }
                            Console.WriteLine("Updated successfully, next update will be in 24 hours.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve exchange rates.");
                    }
                }
            }
        }
        public static decimal GetExchangeRate(string currencyCode)
        {
            decimal exchangeRate = 0;
            string query = "SELECT exchange_rate FROM Currencies WHERE currency_code = @currencyCode;";

            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@currencyCode", currencyCode);

                    var result = command.ExecuteScalar();

                    if (result != DBNull.Value)
                    {
                        exchangeRate = Convert.ToDecimal(result);
                    }
                    else
                    {
                        // Currency code not found
                        Console.WriteLine($"The currency '{currencyCode}' does not exist in the database.");
                        return 0; // Return 0 to indicate an error
                    }
                }
            }
            return exchangeRate;
        }
        public static decimal ConvertCurrency(decimal amount, string convertFrom, string convertTo)
        {
            // Retrieve exchange rates for the specified currencies
            decimal fromRate = GetExchangeRate(convertFrom);
            decimal toRate = GetExchangeRate(convertTo);

            // Check if either of the exchange rates is 0
            if (fromRate == 0 || toRate == 0)
            {
                throw new Exception("One of the exchange rates is invalid. Check that the currency codes are correct and that the exchange rates have been updated.");
            }

            // Convert the amount to the target currency
            decimal convertedAmount = (amount / fromRate) * toRate;
            return convertedAmount;
        }
    }
}
