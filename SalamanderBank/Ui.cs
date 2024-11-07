using System.Media;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SalamanderBank;

// Static class to handle UI components for the bank application.
public static class Ui
{
    // Fields for storing registered user information.
    private static string? _registeredFirstName;
    private static string? _registeredLastName;
    private static string? _registeredEmail;
    private static string? _registeredPassword;

    // Padding for aligning display elements based on the console window width.
    private static readonly double DisplayPadding = (Console.WindowWidth / 2.25);
    private const double MenuPadding = 2.1;
    
    // Display formatted first name with padding.
    private static string FirstNameDisplay => $"First Name: {_registeredFirstName}".PadLeft(_registeredFirstName != null ?
        "First Name: ".Length + _registeredFirstName.Length + (int)DisplayPadding : "First Name: ".Length + (int)DisplayPadding);
    
    // Display formatted last name with padding.
    private static string LastNameDisplay => $"Last Name: {_registeredLastName}".PadLeft(_registeredLastName != null ?
        "Last Name: ".Length + _registeredLastName.Length + (int)DisplayPadding : "Last Name: ".Length + (int)DisplayPadding) ;
    
    // Display formatted email with padding.
    private static string EmailDisplay => $"Email: {_registeredEmail}".PadLeft(_registeredEmail != null ?
        "Email: ".Length + _registeredEmail.Length + (int)DisplayPadding : "Email: ".Length + (int)DisplayPadding);

    // Display formatted password with padding.
    private static string PasswordDisplay => $"Password: {_registeredPassword}".PadLeft(_registeredPassword != null ?
        "Password: ".Length + _registeredPassword.Length + (int)DisplayPadding : "Password: ".Length + (int)DisplayPadding);
    
    // Starting account balance.
    private const decimal AccountBalance = 1500.75m;
    
    // Main menu display and selection handling method.
    public static void DisplayMainMenu()
    {
        Database.InitializeDatabase();
        
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            // Menu options with padding for alignment.
            string option1 = "Create Account".PadLeft("Create Account".Length + (int)((Console.WindowWidth - "Create Account".Length) / MenuPadding));
            string option2 = "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
            string option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1 ));
            
            var login = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10)
                    .HighlightStyle(new Style(new Color(225, 69, 0)))
                    .AddChoices(option1, option2, option3));
            
            // Handling user selection from the main menu.
            switch (login.Trim())
            {
                case "Create Account":
                    CreateAccount();
                    break;
                                
                case "Sign In":
                    throw new NotImplementedException();
                    // break;
                
                case "Exit":
                    Console.WriteLine("Thank you for using SalamanderBank!");
                    Thread.Sleep(2000);
                    return;
            }

            break;
        }
    }

    // Method to create a new account.
    private static void CreateAccount()
    { 
        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();

        // Adding new user to the database.
        Database.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName);
        
        // Sending verification email to the registered email.
        EmailService.SendVerificationEmail(_registeredFirstName, _registeredEmail);
        
        ValidateAccount();
    }
    
    // Method to get the first name from user input.
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
    
    // Method to get the last name from user input.
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
    
    // Method to get the email from user input with validation.
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
                string message1 = "\u001b[38;2;255;69;0mThis email is already in use\u001b[0m";
                Console.Write($"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.7)));
            }
            else
            {
                Console.WriteLine();
                string message2 = "\u001b[38;2;255;69;0mPlease enter a valid email\u001b[0m";
                Console.Write($"{message2}".PadLeft(message2.Length + (int)((Console.WindowWidth - message2.Length) / 1.7)));
            }

            Thread.Sleep(2000);
        }
    }

    // Method to get the password from user input with validation.
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
            
            Thread.Sleep(3000);
        }
    }
    
    // Method to validate the account through a verification code.
    private static void ValidateAccount()
    {
        string? code;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}\n{PasswordDisplay}\n");
            
            string message1 = $"A code has been sent to \u001b[38;2;34;139;34m{_registeredEmail}\u001b[0m use it to verify your account.";
            string message2 = "Enter Code: ";

            Console.WriteLine();
            Console.Write($"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.45)));
            
            Console.WriteLine("\n");
            Console.Write($"{message2}\n".PadLeft(message2.Length + (Console.WindowWidth - message2.Length) / 2));
            
            Console.WriteLine();
            Console.Write("".PadLeft("".Length + (int)((Console.WindowWidth - "".Length) / 2.6)));
            
            code = Console.ReadLine();
            
        } while (string.IsNullOrEmpty(code) || code != EmailService.Code.ToString());

        Database.VerifyUser(_registeredEmail);
        
        AccountDetails();
    }
    
    // Method to display account details in a formatted table.
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
    
    // Method to transfer funds with a loading animation.
    private static void TransferFunds()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("yellow bold"))
            .Start("[yellow]Transferring money...[/]", _ =>
            {
                AnsiConsole.MarkupLine("[yellow]Checking Account Balance...[/]");
                Thread.Sleep(3000);

                AnsiConsole.MarkupLine("[yellow]Checking Receiver...[/]");
                Thread.Sleep(3000);

            });
        
        Console.Clear();
        Logo.DisplayFullLogo();
        
        AnsiConsole.MarkupLine(
            "\n[green]Transaction Complete![/]\nYou will now be redirected to the main menu.");
        
        PlaySound(@"./Sounds/cashier-quotka-chingquot-sound-effect-129698.wav");
        
        Thread.Sleep(3000);
    }
    
    // Method to play a sound from the specified file path.
    private static void PlaySound(string filePath)
    {
        using SoundPlayer soundPlayer = new(filePath);
        soundPlayer.PlaySync();
    }
}
