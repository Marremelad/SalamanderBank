using System.Text.RegularExpressions;
using Spectre.Console;

namespace SalamanderBank;

public static class Ui
{
    private static string? _registeredEmail;
    private static string? _registeredName;
    private static string? _registeredLastName;
    private static readonly decimal AccountBalance = 1500.75m;
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
                    CreateAccount();
                    break;
                                
                case "Sign In":
                    throw new NotImplementedException(); // DisplayLogin();
                    //break;
                
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
        _registeredName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();     
        EmailService.SendVerificationEmail(_registeredName, _registeredEmail);
        ValidateAccount();
    }

    private static string Password() 
    {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .Secret('_'));
    }

    
    private static string GetFirstName()
    {
        string? name;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.Write($"{"Name: ", 77}");
            name = Console.ReadLine();
        } while (string.IsNullOrEmpty(name));

        return name;
    }

    private static string GetLastName()
    {
        string? lastName;
        string nameDisplay = ("Name: " + _registeredName).PadLeft(_registeredName != null ? 77 + _registeredName.Length : 77);
        
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.WriteLine(nameDisplay);
            Console.Write($"{"Last Name: ",82}");
            lastName = Console.ReadLine();
        } while (string.IsNullOrEmpty(lastName));

        return lastName;
    }
    
    private static string GetEmail()
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        
        
        while (true)
        {
            string nameDisplay = ("Name: " + _registeredName).PadLeft(_registeredName != null ? 77 + _registeredName.Length : 77);
            string lastNameDisplay = ("Last Name: " + _registeredLastName).PadLeft(_registeredLastName != null ? 82 + _registeredLastName.Length : 82);

            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.WriteLine(nameDisplay);
            Console.WriteLine(lastNameDisplay);

            Console.Write($"{"Email: ", 78}");
            string? email = Console.ReadLine();
            
            if (email != null && Regex.IsMatch(email, emailPattern))
            {
                return email;
            }

            Console.Write($"\n\u001b[38;2;255;69;0m{"Please enter a valid email",91}\u001b[0m");
            Console.ResetColor();

            Thread.Sleep(2000);
        }

    }
    
    private static void ValidateAccount()
    {
        string nameDisplay = ("Name: " + _registeredName).PadLeft(_registeredName != null ? 77 + _registeredName.Length : 77);
        string lastNameDisplay = ("Last Name: " + _registeredLastName).PadLeft(_registeredLastName != null ? 82 + _registeredLastName.Length : 82);
        string emailDisplay = ("Email: " + _registeredEmail).PadLeft(_registeredEmail != null ? 78 + _registeredEmail.Length : 78);
        
        string? code;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            Console.WriteLine(nameDisplay);
            Console.WriteLine(lastNameDisplay);
            Console.WriteLine(emailDisplay);
            
            // Console.WriteLine($"\n{"A code has been sent to " + _registeredEmail + " use it to log in to your account", 120}");
            Console.Write($"\n{"A code has been sent to ", 61}"); // Adjust padding as needed
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_registeredEmail);
            Console.ResetColor();
            Console.Write(" use it to log in to your account");
            
            Console.Write($"\n\n{"Enter Code: ", 83}");
            code = Console.ReadLine();
            
        } while (string.IsNullOrEmpty(code) || code != EmailService.Code.ToString());
        
        AccountDetails();
    }
    
    private static void AccountDetails()
    {
        var table = new Table();
        
        table.AddColumn("Account Information");
        table.AddRow($"Name: {_registeredName} {_registeredLastName}");
        table.AddRow($"Email: {_registeredEmail}");
        table.AddRow($"Balance: {AccountBalance:F2}");
            
        Console.Clear();
        AnsiConsole.Write(table);
    }
}