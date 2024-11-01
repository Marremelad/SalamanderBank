using System.Data.Entity.ModelConfiguration.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SalamanderBank;

public class LoginBox
{
    private static string _registeredEmail = "";
    private static string _registeredName = "";
    private static readonly string RegisteredPassword = "123";
    private static readonly decimal AccountBalance = 1500.75m;
    private static readonly string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    private static readonly Regex NamePattern = new Regex(@"^[a-zA-Z\s]+$");
    //Setting Padding for centering the text
    private const int LeftPadding = 75;
    public static void DisplayMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            //Centering the text
            string option1 = "Create Account".PadLeft( LeftPadding + ("Create Account".Length / 2));
            string option2 = "Sign In".PadLeft( LeftPadding + ("Sign In".Length / 2));
            string option3 = "Exit".PadLeft(LeftPadding + ("Exit".Length / 2));
            var login = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10)
                    .AddChoices(option1, option2, option3));
            
            switch (login.Trim())
            {
                case "Create Account":
                    DisplayCreateAccount();
                    break;
                                
                case "Sign In":
                    DisplayLogin();
                    break;
                case "Exit":
                    Console.WriteLine("Thank you for using SalamanderBank!");
                    Thread.Sleep(2000);
                    return;
            }
        
            break;
        }
                 
    }
    
    public static void DisplayCreateAccount()
    {
            _registeredName = GetValidName();
            _registeredEmail = GetValidEmail(); 
            EmailService.SendVerificationEmail(_registeredName, _registeredEmail);
            ValidateAccount();
    }
   
    public static string Password() 
    {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .Secret('_'));
    }

    private static string GetValidEmail()
    {
        while (true)
        {
            //Original Code
            // var email = AnsiConsole.Prompt(new TextPrompt<string>("Enter valid email address:"));
            // if (Regex.IsMatch(email, EmailPattern))
            // {
            //     return email;
            // }
            // AnsiConsole.MarkupLine("Please enter a valid email address!");
            // Centered prompt message
            
            // Test 1
            string promptMessage = "Enter a valid email address:";
            AnsiConsole.MarkupLine("[bold]{0}[/]", promptMessage.PadLeft(LeftPadding + (promptMessage.Length / 2)));
            
            var email = AnsiConsole.Prompt(new TextPrompt<string>("").PromptStyle("green"));
            
            if (Regex.IsMatch(email, EmailPattern))
            {
                return email;
            }
            Console.Clear();
            
            Logo.DisplayFullLogo();
            
            string errorMessage = "Please enter a valid email address!";
            AnsiConsole.MarkupLine("[bold red]{0}[/]", errorMessage.PadLeft(LeftPadding + (errorMessage.Length / 2)));
            
        }
    } 
    private static string GetValidName()
    {
        
        string promptMessage = "Enter a valid name (letters only):";
        Console.WriteLine(promptMessage.PadLeft(LeftPadding + (promptMessage.Length / 2)));
        
        StringBuilder input = new StringBuilder();
        while (true)
        {
            //
            // string promptMessage = "Enter a valid name (letters only):";
            // Console.WriteLine(promptMessage.PadLeft(LeftPadding + (promptMessage.Length / 2)));
            //
            // // Read user input
            // Console.ForegroundColor = ConsoleColor.Green;
            // string name = Console.ReadLine();
            // Console.ResetColor();
            //
            // // Check if input is non-empty and contains only letters/spaces
            // if (!string.IsNullOrWhiteSpace(name) && NamePattern.IsMatch(name))
            // {
            //     return name; // Valid input, exit loop
            // }
            // Console.Clear();
            //
            // // Centered error message for invalid input
            // Logo.DisplayFullLogo();
            // string errorMessage = "Please enter a valid name (letters only)!";
            // Console.ForegroundColor = ConsoleColor.Red;
            // Console.WriteLine(errorMessage.PadLeft(LeftPadding + (errorMessage.Length / 2)));
            // Console.ResetColor();

            //Original Code
            // string name = AnsiConsole.Prompt(new TextPrompt<string>("What is you name?"));
            //
            // if (string.IsNullOrEmpty(name)) Console.WriteLine("Name can not be empty.");
            // else return name;
            
            //Test
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.WriteLine("\b \b");
                }
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (NamePattern.IsMatch(input.ToString()))
                {
                    return input.ToString();
                    
                }

                Console.Clear();
                Logo.DisplayFullLogo();
                string errorMessage = "Please enter a valid name (letters only)!";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage.PadLeft(LeftPadding + (errorMessage.Length / 2)));
                Console.ResetColor();
                input.Clear();
                // string promptMessage = "Enter a valid name (letters only):";
                Console.WriteLine(promptMessage.PadLeft(LeftPadding + (promptMessage.Length / 2)));
                continue;
            }

            if (char.IsLetter(keyInfo.KeyChar) || char.IsWhiteSpace(keyInfo.KeyChar))
            {
                input.Append(keyInfo.KeyChar);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(keyInfo.KeyChar);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(keyInfo.KeyChar);
            }
            Console.ResetColor();
        }
    }

    public static void DisplayAccountDetails()
    {
            string promptMessage = ("Welcome to SalamanderBank");
            //AnsiConsole.MarkupLine("[bold]{0}[/]", promptMessage.PadLeft((Console.WindowWidth / 2) + (promptMessage.Length / 2)));
            var table = new Table();
            table.AddColumn("Account Information");
            table.AddRow($"Email: {_registeredEmail}");
            table.AddRow($"Balance: {AccountBalance:F2}");
            
            AnsiConsole.Write(table);
    }

    public static void ValidateAccount()
    {
        while(AnsiConsole.Prompt(new TextPrompt<string>("Enter password:")) != EmailService.DefaultPassword.ToString())
        {
            Console.WriteLine("Invalid password!");
        }
        DisplayAccountDetails();

    }
    public static void DisplayLogin()
    {
        int attempt = 3;

        while (attempt > 0)
        {
            var email = AnsiConsole.Prompt(new TextPrompt<string>("Password:"));
            var password = Password();

            if (password == RegisteredPassword)
            {
                DisplayAccountDetails();
                return;
            }
            
            while (email != _registeredEmail && password != RegisteredPassword)
            {
                attempt--;
                if (attempt > 0)
                {
                    AnsiConsole.MarkupLine("Invalid email or password.");
                    AnsiConsole.MarkupLine($"[red]Invalid email or password. Attempts left: {attempt}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]All login attempts exhausted. Returning to main menu.[/]");
                    DisplayMainMenu();
                }
            }
            
        }

        
    }
    
}