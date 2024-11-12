using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;
using System.Text.Json;
using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class CurrencyManager
    {
        public static async Task UpdateCurrenciesAsync()
        {
            //Open a connection to the SQLite database 
            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {

                connection.Open();


                // Retrieve the latest update time from the database
                string lastUpdateQuery = "SELECT MAX(LastUpdated) FROM Currencies;";

                //Initalze a variable to store the data of the last update
                DateTime lastUpdated = DateTime.MinValue;

                using (SQLiteCommand command = new SQLiteCommand(lastUpdateQuery, connection))
                {
                    // Execute the query to get last update time timestamp
                    var result = command.ExecuteScalar();


                    //If a valid date is returned, update the lastUpdated variable
                    if (result != DBNull.Value)
                    {

                        lastUpdated = Convert.ToDateTime(result);
                    }
                }


                // Check if more than 24 hours have passed since the last update
                TimeSpan timeSinceLastUpdate = DateTime.Now - lastUpdated;

                // Check if more than 24 hours have passed since the last update
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

                Env.Load("./Credentials.env"); //Load API key from enviroment variables
                string apiKey = Env.GetString("CURRENCY_API_KEY");
                string url = $"https://api.currencyapi.com/v3/latest?apikey={apiKey}&currencies=&base_currency=SEK";

                using (HttpClient client = new HttpClient())
                {
                    //Send the API request and wait the response
                    HttpResponseMessage response = await client.GetAsync(url);

                    //Process the response if it's successful
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                   
                        //Parse JSON response to access exchnage rate data
                        using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement data = document.RootElement.GetProperty("data");


                            //Start a transaction for bulk insertion/updating of exchange rates
                            using (SQLiteTransaction transaction = connection.BeginTransaction())
                            {
                                foreach (JsonProperty currency in data.EnumerateObject())
                                {
                                    string currencyCode = currency.Name; // Currency code (e.g, USD,EUR)

                                    // Retrieve the exchange rate value for the currency
                                    decimal exchangeRate = currency.Value.GetProperty("value").GetDecimal();

                                    //Define an SQL query insert or update the currency or update the currency exchange rate
                                    string upsertQuery = @"

                                    INSERT INTO Currencies (CurrencyCode, ExchangeRate, LastUpdated)

                                    VALUES (@currencyCode, @exchangeRate, @lastUpdated)

                                    ON CONFLICT(CurrencyCode) 

                                    DO UPDATE SET ExchangeRate = @exchangeRate, LastUpdated = @lastUpdated;";

                                    
                                    using (SQLiteCommand command = new SQLiteCommand(upsertQuery, connection))
                                    {
                                        // Add parameters to the query to prevent SQL injection
                                        command.Parameters.AddWithValue("@currencyCode", currencyCode);

                                        command.Parameters.AddWithValue("@exchangeRate", exchangeRate);

                                        command.Parameters.AddWithValue("@lastUpdated", DateTime.Now);

                                        //Execute the query to insert/update the database 
                                        command.ExecuteNonQuery();

                                    }
                                }
                                //Commit the transaction after all updates are completed 
                                transaction.Commit();
                            }
                            Console.WriteLine("Updated successfully, next update will be in 24 hours.");
                        }
                    }
                    else
                    {
                        //Log message if API request failed 
                        Console.WriteLine("Failed to retrieve exchange rates.");
                    }
                }
            }
        }

        public static decimal GetExchangeRate(string currencyCode)
        {
            //Intialize a variable to sotre the exchange rate
            decimal exchangeRate = 0;

            // Define a query to fetch the exchange rate for the specified currency code 
            string query = "SELECT ExchangeRate FROM Currencies WHERE CurrencyCode = @currencyCode;";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // Add currency code as a parameter to the query
                    command.Parameters.AddWithValue("@currencyCode", currencyCode);

                    // Excute the query and store the result
                    var result = command.ExecuteScalar();

                    //If valid exchnage rate is found, update the exchangeRate variable
                    if (result != DBNull.Value)
                    {
                        exchangeRate = Convert.ToDecimal(result);
                    }
                    else
                    {
                        // Log an error message if the currency code was not found 
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

            // Check if either of the exchange rates is 0 (invalid)
            if (fromRate == 0 || toRate == 0)
            {
                throw new Exception("One of the exchange rates is invalid. Check that the currency codes are correct and that the exchange rates have been updated.");
            }

            // Calculate the converted the amount based on the exchange rates
            decimal convertedAmount = (amount / fromRate) * toRate;
            return convertedAmount;
        }
        public static List<Currency>? GetAllCurrencies()
        {
            string query = @"SELECT * FROM Currencies";

            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                List<Currency> codes = connection.Query<Currency>(query).ToList();
                return codes;
            }
        }
    }
}
