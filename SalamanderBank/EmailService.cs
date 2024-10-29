using MimeKit;
using MailKit.Net.Smtp;
namespace SalamanderBank;

public static class EmailService
{
    private const string Password = "bdkw ufnw npnc hjvc"; // DO NOT CHANGE!!!
    public static Guid Guid = Guid.NewGuid();
    public static DateTime CurrentDate = DateTime.Now;
    public static void SendEmail(string name, string email, string message)
    {
        var mimeMessage = new MimeMessage ();
        mimeMessage.From.Add(new MailboxAddress("SalamanderBank", "salamanderbank@gmail.com"));
        mimeMessage.To.Add (new MailboxAddress ($"{name}", $"{email}"));
        mimeMessage.Subject = "Verification";
        
        mimeMessage.Body = new TextPart ("html") 
        {
            Text = message
        };

        using (var client = new SmtpClient ()) 
        {
            client.Connect ("smtp.gmail.com", 587, false);
            
            client.Authenticate ("salamanderbank@gmail.com", Password);

            client.Send (mimeMessage);
            client.Disconnect (true);
        }
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