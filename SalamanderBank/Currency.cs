using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;
using System.Text.Json;
using System.Data.SQLite;

namespace SalamanderBank
{
    internal class Currency
    {
        public static async Task UpdateCurrenciesAsync()
        {

            using (SQLiteConnection connection = new SQLiteConnection(Database._connectionString))
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

            using (SQLiteConnection connection = new SQLiteConnection(Database._connectionString))
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
