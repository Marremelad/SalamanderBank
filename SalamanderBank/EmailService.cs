﻿using MimeKit;
using MailKit.Net.Smtp;
using DotNetEnv;

namespace SalamanderBank;

// Provides email-related services for SalamanderBank, including verification, transaction, and transfer notifications.
public static class EmailService
{
    // Unique code for email verification.
    public static Guid Code;

    // Sends an email with specified details to the target email address.
    private static void SendEmail(string? name, string? targetEmail, string subject, string message)
    {
        try
        {
            Env.Load("./Credentials.env");
            var email = Env.GetString("EMAIL");
            var emailPassword = Env.GetString("EMAIL_PASSWORD");

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Salamander", email));
            mimeMessage.To.Add(new MailboxAddress(name, targetEmail));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message
            };

            using var client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate(email, emailPassword);

            client.Send(mimeMessage);
            client.Disconnect(true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong while trying to send an email.\n{e}");
            throw;
        }
    }

    // Sends a verification email with a unique code for account setup.
    public static void SendVerificationEmail(string? name, string? email)
    {
        Code = Guid.NewGuid();
        string htmlBody = $@"
        <html>
            <body style='margin: 0; padding: 0;'>
                <p style='margin: 0;'>Hello {name}. Welcome to Salamander Bank! To get started, verify your account by using the code below.</p>
                <p style='color:green; margin: 0;'><strong>{Code}</strong></p>
                <pre style='margin: 0; margin-top: 24px;'>{Logo.FireLogo}</pre>
                <p style='margin: 0; margin-top: 48px;'>// Team Salamander</p>
                <p style='margin: 0;'>{DateTime.Now}</p>
            </body>
        </html>";

        SendEmail(name, email, "Verification", htmlBody);
    }
}
