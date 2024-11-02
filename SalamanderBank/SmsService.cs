using System.Net.Http.Headers;
using DotNetEnv;

namespace SalamanderBank;

public class SmsService
{
    public static async Task<string> SendSms(string phoneNumber, string message = "Hello from team salamander!")
    {
        try
        {Env.Load("./Credentials.env");
            var smsApiUsername = Env.GetString("SMS_API_USERNAME");
            var smsApiPassword = Env.GetString("SMS_API_PASSWORD");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{smsApiUsername}:{smsApiPassword}")));

            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("from", "Salamander"),
                new KeyValuePair<string, string>("to", phoneNumber), // Replace dash with number to send sms to.
                new KeyValuePair<string, string>("message", message)
            };

            using var content = new FormUrlEncodedContent(data);
            using var response = await httpClient.PostAsync("https://api.46elks.com/a1/sms", content);
            string responseContent = await response.Content.ReadAsStringAsync();

            return responseContent; }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong while trying to send an SMS\n{e}");
            throw;
        }
    }
}