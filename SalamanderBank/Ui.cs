using System.Data.SQLite;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SalamanderBank;

public static class Ui
{
    private static string? _registeredFirstName;
    private static string? _registeredLastName;
    private static string? _registeredEmail;
    private static string? _registeredPassword;

    private static readonly double DisplayPadding = (Console.WindowWidth / 2.25);
    private const double MenuPadding = 2.1;
    
    private static string FirstNameDisplay => $"First Name: {_registeredFirstName}".PadLeft(_registeredFirstName != null ?
        "First Name: ".Length + _registeredFirstName.Length + (int)DisplayPadding : "First Name: ".Length + (int)DisplayPadding);
    
    private static string LastNameDisplay => $"Last Name: {_registeredLastName}".PadLeft(_registeredLastName != null ?
        "Last Name: ".Length + _registeredLastName.Length + (int)DisplayPadding : "Last Name: ".Length + (int)DisplayPadding) ;
    
    private static string EmailDisplay => $"Email: {_registeredEmail}".PadLeft(_registeredEmail != null ?
        "Email: ".Length + _registeredEmail.Length + (int)DisplayPadding : "Email: ".Length + (int)DisplayPadding);

    private static string PasswordDisplay => $"Password: {_registeredPassword}".PadLeft(_registeredPassword != null ?
        "Password: ".Length + _registeredPassword.Length + (int)DisplayPadding : "Password: ".Length + (int)DisplayPadding);
    
    private const decimal AccountBalance = 1500.75m;
    
    public static void DisplayMainMenu()
    {
        Database.InitializeDatabase();
        
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            string option1 = "Create Account".PadLeft("Create Account".Length + (int)((Console.WindowWidth - "Create Account".Length) / MenuPadding));
            string option2 = "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
            string option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1 ));
            
            var login = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10)
                    .AddChoices(option1, option2, option3));
            
            switch (login.Trim())
            {
                case "Create Account":
                    CreateAccount();
                    break;
                                
                case "Sign In":
                    throw new NotImplementedException();
                    break;
                
                case "Exit":
                    Console.WriteLine("Thank you for using SalamanderBank!");
                    Thread.Sleep(2000);
                    return;
            }
        
            break;
        }
                 
    }

    private static void CreateAccount()
    { 
        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();
        
        EmailService.SendVerificationEmail(_registeredFirstName, _registeredEmail);
        
        ValidateAccount();
    }
    
    private static string GetFirstName()
    {
        string? name;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.Write($"{FirstNameDisplay}");
            
        } while (string.IsNullOrEmpty(name = Console.ReadLine()));

        return name;
    }
    
    private static string GetLastName()
    {
        string? lastName;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}");
            
        } while (string.IsNullOrEmpty(lastName = Console.ReadLine()));
    
        return lastName;
    }
    
    private static string GetEmail()
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}");
            
            string? email = Console.ReadLine();
            if (email != null && Regex.IsMatch(email, emailPattern))
            {
                if (!Database.EmailExists(email))
                {
                    return email;
                }
                
                Console.WriteLine();
                string emailExists = "\u001b[38;2;255;69;0mPlease enter a valid email\u001b[0m";
                Console.Write($"{emailExists}".PadLeft(emailExists.Length + (int)((Console.WindowWidth - emailExists.Length) / 1.7)));
                Console.ResetColor();
            }
            
            Console.WriteLine();
            string message = "\u001b[38;2;255;69;0mPlease enter a valid email\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            Console.ResetColor();
        
            Thread.Sleep(2000);
        }
    }

    private static string GetPassword()
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
                
            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}\n{PasswordDisplay}");

            string? password = Console.ReadLine();
            if (!string.IsNullOrEmpty(password) && password.Length >= 8)
            {
                return password;
            }
            
            Console.WriteLine();
            string message = "\u001b[38;2;255;69;0mPassword has to be at least 8 characters long\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            Console.ResetColor();
        
            Thread.Sleep(3000);
        }
        
    }
    
    private static void ValidateAccount()
    {
        string? code;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}\n{PasswordDisplay}\n");
            //Writes out a error message where "email" is in red
            string message = $"A code has been sent to \u001b[38;2;34;139;34m{_registeredEmail}\u001b[0m, use it to verify your account.";
            string message2 = "Enter Code: ";

            Console.WriteLine();
            Console.Write($"{message}".PadLeft(message.Length + ((Console.WindowWidth - message.Length) / 2)));
            Console.WriteLine();
            Console.Write($"{message2}".PadLeft(message2.Length + ((Console.WindowWidth - message2.Length) / 2)));
            code = Console.ReadLine();
            
        } while (string.IsNullOrEmpty(code) || code != EmailService.Code.ToString());
        
        AccountDetails();
    }
    
    private static void AccountDetails()
    {
        var table = new Table();
        
        table.AddColumn("Account Information");
        table.AddRow($"Name: {_registeredFirstName} {_registeredLastName}");
        table.AddRow($"Email: {_registeredEmail}");
        table.AddRow($"Balance: {AccountBalance:F2}");
            
        Console.Clear();
        AnsiConsole.Write(table);
    }
}