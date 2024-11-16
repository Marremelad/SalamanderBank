using DotNetEnv;
using System.Text.Json;
using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class CurrencyManager
    {
        // Updates the currency exchange rates by making an API call and updating the database
        public static async Task UpdateCurrenciesAsync()
        {
            // Opens a connection to the SQLite database
            using (SQLiteConnection connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();

                // Retrieves the latest update time from the database
                string lastUpdateQuery = "SELECT MAX(LastUpdated) FROM Currencies;";

                // Initializes a variable to store the data of the last update
                DateTime lastUpdated = DateTime.MinValue;

                using (SQLiteCommand command = new SQLiteCommand(lastUpdateQuery, connection))
                {
                    // Executes the query to get last update time timestamp
                    var result = command.ExecuteScalar();

                    // If a valid date is returned, update the lastUpdated variable
                    if (result != DBNull.Value)
                    {
                        lastUpdated = Convert.ToDateTime(result);
                    }
                }

                // Checks if more than 24 hours have passed since the last update
                TimeSpan timeSinceLastUpdate = DateTime.Now - lastUpdated;

                // If less than 24 hours have passed, exit the method
                if (timeSinceLastUpdate.TotalHours < 24)
                {
                    DateTime nextUpdate = lastUpdated.AddHours(24);
                    Console.WriteLine($"No update has been made to the Currencies Database.");
                    Console.WriteLine($"The next update will occur on {nextUpdate:yyyy-MM-dd} at {nextUpdate:HH:mm}.");
                    return;
                }

                // Makes an API call to fetch current exchange rates
                Env.Load("./Credentials.env"); // Loads API key from environment variables
                string apiKey = Env.GetString("CURRENCY_API_KEY");
                string url = $"https://api.currencyapi.com/v3/latest?apikey={apiKey}&currencies=&base_currency=SEK";

                using (HttpClient client = new HttpClient())
                {
                    // Sends the API request and waits for the response
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Processes the response if it's successful
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Parses JSON response to access exchange rate data
                        using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement data = document.RootElement.GetProperty("data");

                            // Starts a transaction for bulk insertion/updating of exchange rates
                            using (SQLiteTransaction transaction = connection.BeginTransaction())
                            {
                                foreach (JsonProperty currency in data.EnumerateObject())
                                {
                                    string currencyCode = currency.Name; // Currency code (e.g., USD, EUR)

                                    // Retrieves the exchange rate value for the currency
                                    decimal exchangeRate = currency.Value.GetProperty("value").GetDecimal();

                                    // Defines an SQL query to insert or update the currency exchange rate
                                    string upsertQuery = @"
                                    INSERT INTO Currencies (CurrencyCode, ExchangeRate, LastUpdated)
                                    VALUES (@currencyCode, @exchangeRate, @lastUpdated)
                                    ON CONFLICT(CurrencyCode) 
                                    DO UPDATE SET ExchangeRate = @exchangeRate, LastUpdated = @lastUpdated;";

                                    using (SQLiteCommand command = new SQLiteCommand(upsertQuery, connection))
                                    {
                                        // Adds parameters to the query to prevent SQL injection
                                        command.Parameters.AddWithValue("@currencyCode", currencyCode);
                                        command.Parameters.AddWithValue("@exchangeRate", exchangeRate);
                                        command.Parameters.AddWithValue("@lastUpdated", DateTime.Now);

                                        // Executes the query to insert/update the database
                                        command.ExecuteNonQuery();
                                    }
                                }
                                // Commits the transaction after all updates are completed
                                transaction.Commit();
                            }
                            Console.WriteLine("Updated successfully, next update will be in 24 hours.");
                        }
                    }
                    else
                    {
                        // Logs message if API request failed
                        Console.WriteLine("Failed to retrieve exchange rates.");
                    }
                }
            }
        }

        // Gets the exchange rate for a specific currency code
        public static decimal GetExchangeRate(string? currencyCode)
        {
            // Initializes a variable to store the exchange rate
            decimal exchangeRate;

            // Defines a query to fetch the exchange rate for the specified currency code
            string query = "SELECT ExchangeRate FROM Currencies WHERE CurrencyCode = @currencyCode;";

            using (SQLiteConnection connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // Adds currency code as a parameter to the query
                    command.Parameters.AddWithValue("@currencyCode", currencyCode);

                    // Executes the query and stores the result
                    var result = command.ExecuteScalar();

                    // If valid exchange rate is found, updates the exchangeRate variable
                    if (result != DBNull.Value)
                    {
                        exchangeRate = Convert.ToDecimal(result);
                    }
                    else
                    {
                        // Logs an error message if the currency code was not found
                        Console.WriteLine($"The currency '{currencyCode}' does not exist in the database.");
                        return 0; // Returns 0 to indicate an error
                    }
                }
            }
            return exchangeRate;
        }

        // Converts an amount from one currency to another based on exchange rates
        public static decimal ConvertCurrency(decimal amount, string? convertFrom, string? convertTo)
        {
            // Retrieves exchange rates for the specified currencies
            decimal fromRate = GetExchangeRate(convertFrom);
            decimal toRate = GetExchangeRate(convertTo);

            // Checks if either of the exchange rates is 0 (invalid)
            if (fromRate == 0 || toRate == 0)
            {
                throw new Exception("One of the exchange rates is invalid. Check that the currency codes are correct and that the exchange rates have been updated.");
            }

            // Calculates the converted amount based on the exchange rates
            decimal convertedAmount = (amount / fromRate) * toRate;
            return convertedAmount;
        }

        // Retrieves all the currencies from the database
        public static List<Currency> GetAllCurrencies()
        {
            string query = @"SELECT * FROM Currencies";

            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                List<Currency> codes = connection.Query<Currency>(query).ToList();
                return codes;
            }
        }
    }
}
