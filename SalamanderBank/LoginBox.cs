using System.Text.RegularExpressions;
using Spectre.Console;

namespace SalamanderBank;

public class LoginBox
{
    private static readonly string RegisteredEmail = "user@example.com";
    private static readonly string RegisteredPassword = "123";
    private static readonly decimal AccountBalance = 1500.75m;
    private static readonly string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    public static void DisplayMainMenu()
    {
        while (true)
        {
            var login = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Welcome to SalamanderBank")
                    .PageSize(10)
                    .AddChoices("Create a new account", "Login to existing account", "Exit"));
            switch (login)
            {
                case "Create a new account":
                    DisplayCreateAccount();
                    break;
                                
                case "Login to Existing Account":
                    DisplayLogin();
                    break;
                case "Exit":
                    return;
            }
        
            break;
        }
                 
    }

    public static void DisplayCreateAccount()
    {
            var table = new Table();
            table.AddColumn(new TableColumn("Welcome to SalamanderBank"));
            table.AddRow("Create Account");
            
            string email = GetValidEmail(); // Use the new method for email validation
            var password = Password();
            
            table.AddRow($"Email: {email}\nPassword: {new string ('*', password.Length)}");

            AnsiConsole.Write(table);
            
            AnsiConsole.MarkupLine("Account created successfully!");
            DisplayLogin();
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
            var email = AnsiConsole.Prompt(new TextPrompt<string>("Enter valid email address:"));
            if (Regex.IsMatch(email, EmailPattern))
            {
                return email;
            }
            else
            {
                AnsiConsole.MarkupLine("Please enter a valid email address!");
                
            }
        }
    }

    public static void DisplayAccountDetails()
    {
            AnsiConsole.MarkupLine("Welcome to SalamanderBank");
            
            var table = new Table();
            table.AddColumn("Account Information");
            table.AddRow($"Email: {RegisteredEmail}");
            table.AddRow($"Balance: {AccountBalance:F2}");
            
            AnsiConsole.Write(table);
    }

    public static void DisplayLogin()
    {
        int attempt = 3;

        while (attempt > 0)
        {
            var email = AnsiConsole.Prompt(new TextPrompt<string>("Enter email:"));
            var password = Password();

            if (email == RegisteredEmail && password == RegisteredPassword)
            {
                DisplayAccountDetails();
                return;
            }
            else
            {
                while (email != RegisteredEmail && password != RegisteredPassword)
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
    
}