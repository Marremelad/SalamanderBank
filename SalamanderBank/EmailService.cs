using MimeKit;
using MailKit.Net.Smtp;
using DotNetEnv;
namespace SalamanderBank;

public static class EmailService
{
    private static readonly Guid Guid = Guid.NewGuid();
    private static readonly DateTime CurrentDate = DateTime.Now;
    public static void SendEmail(string name, string email, string subject, string message)
    {
        Env.Load("./Credentials.env");
        var mimeMessage = new MimeMessage ();
        mimeMessage.From.Add(new MailboxAddress("SalamanderBank", Env.GetString("EMAIL")));
        mimeMessage.To.Add (new MailboxAddress ($"{name}", $"{email}"));
        mimeMessage.Subject = subject;
        
        mimeMessage.Body = new TextPart ("html") 
        {
            Text = message
        };

        using var client = new SmtpClient ();
        client.Connect ("smtp.gmail.com", 587, false);
            
        client.Authenticate ("salamanderbank@gmail.com", Env.GetString("APP_PASSWORD"));

        client.Send (mimeMessage);
        client.Disconnect (true);
    }

    public static string VerificationText(string name)
    {
        return $@"
        <html>
            <body style='margin: 0; padding: 0;'>
                <p style='margin: 0;'>Hello {name}. Welcome to Salamander Bank! To get started, verify your account by using the code below.</p>
                <p style='color:green; margin: 0;'><strong>{Guid}</strong></p>
                <pre style='margin: 0; margin-top: 24px;'>{Logo.FireLogo}</pre>
                <p style='margin: 0; margin-top: 24px;'>// Team Salamander</p>
                <p style='margin: 0;'>{CurrentDate}</p>
            </body>
        </html>";
    }
}