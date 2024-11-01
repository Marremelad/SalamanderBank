using System.Net.Http.Headers;
using DotNetEnv;

namespace SalamanderBank;

public class SmsService
{
    public static async Task SendSms()
    {
        Env.Load("./Credentials.env");
        var smsApiUsername = Env.GetString("SMS_API_USERNAME");
        var smsApiPassword = Env.GetString("SMS_API_PASSWORD");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{smsApiUsername}:{smsApiPassword}")));

        var data = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("from", "Salamander"),
            new KeyValuePair<string, string>("to", "-"), // Replace dash with number to send sms to.
            new KeyValuePair<string, string>("message", "Hello from team Salamander")
        };

        using var content = new FormUrlEncodedContent(data);
        using var response = await httpClient.PostAsync("https://api.46elks.com/a1/sms", content);
        string responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
    }
}