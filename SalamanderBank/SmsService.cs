using System.Net.Http.Headers;
using DotNetEnv;

namespace SalamanderBank;

// Service to handle sending SMS messages through the SMS API.
public class SmsService
{
    // Method to send an SMS to the specified phone number.
    public static async Task<string> SendSms(string phoneNumber, string message = "Hello from team Salamander!")
    {
        try
        {
            // Load environment variables for SMS API credentials.
            Env.Load("./Credentials.env");
            var smsApiUsername = Env.GetString("SMS_API_USERNAME");
            var smsApiPassword = Env.GetString("SMS_API_PASSWORD");

            // Set up HTTP client and authentication header.
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{smsApiUsername}:{smsApiPassword}")));

            // Prepare the data for the SMS request.
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("from", "Salamander"),
                new KeyValuePair<string, string>("to", phoneNumber),
                new KeyValuePair<string, string>("message", message)
            };

            // Send the SMS via POST request and retrieve the response.
            using var content = new FormUrlEncodedContent(data);
            using var response = await httpClient.PostAsync("https://api.46elks.com/a1/sms", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong while trying to send an SMS\n{e}");
            throw;
        }
    }
}