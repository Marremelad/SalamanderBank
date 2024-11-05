using System.Media;
using System.Text.RegularExpressions;
using NAudio.Dmo;
using Spectre.Console;
using NAudio.Wave;


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
            
            string? password = Console.ReadLine();
            if (password != null && Regex.IsMatch(password, emailPattern))
            {
                return password;
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
        Console.Clear();
        Logo.DisplayFullLogo();
        
        var table = new Table();
        
        
        table.AddColumn("Account Information");
        table.AddRow($"Name: {_registeredFirstName} {_registeredLastName}");
        table.AddRow($"Email: {_registeredEmail}");
        table.AddRow($"Total Balance: {AccountBalance:F2}");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(foreground: ConsoleColor.DarkRed);
        table.AddRow($"Password: {_registeredPassword}");
        table.Alignment(Justify.Center);
        AnsiConsole.Write(table);
       
    }

    public static async void LiveAccount()
    {
        Console.Clear();

        // Display the logo at the top of the console
        Logo.DisplayFullLogo();

        // Create a new layout with limited space usage
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Left").Size(30),       // Set the width of the Left column
                new Layout("Right").Size(70)       // Set the width of the Right column
                    .SplitRows(
                        new Layout("Top").Size(20),    // Set the height of the Top section
                        new Layout("Bottom").Size(20)  // Set the height of the Bottom section
                    )
            );

        // Update the Left section with initial text
        layout["Left"].Update(
            new Panel(
                Align.Center(
                    new Markup("[bold green]Hello SalamanderBank![/]"),
                    VerticalAlignment.Middle
                )
            )
        );

        // Add content to the Right sections
        layout["Top"].Update(new Panel("Top Content"));
        layout["Bottom"].Update(new Panel("Bottom Content"));

        // Render the layout first
        AnsiConsole.Write(layout);

        // Render the selection prompt separately
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Please select an option:[/]")
                .PageSize(10)
                .AddChoices( "Check Balance", "Transfer Funds", "View Transactions", "Exit" )
        );

        // Perform actions based on the selection
        AnsiConsole.MarkupLine($"[bold green]You selected:[/] {selection}");
    }
    
    public static async void LiveAccount2()
    {

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
                    SignedIn();
                    break;
                
                case "Exit":
                    Console.WriteLine("Thank you for using SalamanderBank!");
                    Thread.Sleep(2000);
                    return;
            }
        
            break;
        }

        static void SignedIn()
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            var welcome = new Panel(new Markup("[bold red]Welcome to SalamanderBank![/]"))
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                Width = 40
            };
            
            AnsiConsole.Write(welcome);
            AnsiConsole.WriteLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(3)
                    .AddChoices("Check Balance", "Transfer Funds", "Money Exchange", "Take Loan", "View Transactions", "Exit")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    );
            
            switch (selection)
            {
                case "Check Balance":
                    AccountDetails();
                    break;
                case "Transfer Funds":
                    TransferFunds();
                    break;
                case "Money Exchange":
                    MoneyExchange();
                    break;
                case "Take Loan":
                    TakeLoan();
                    break;
                case "View Transactions":
                    ViewTransaction();
                    break;
                case "Exit":
                    return;
            }
            
        }
        //Transferring funds between accounts,
        //Needs account information
        //Needs sound
        static void TransferFunds()
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
                PlaySound(@"C:\Users\rasmu\source\repos\SalamanderBank\SoundPath\cashier-quotka-chingquot-sound-effect-129698.wav");
                Thread.Sleep(3000);
                SignedIn();
            
        }
        static void PlaySound(string filePath)
        {
            using (SoundPlayer player = new SoundPlayer(filePath))
            {
                player.Load();
                player.Play();
            }
        
        }

        static void MoneyExchange()
        {
            while (true)
            {
                //Show account to change from
                //Ask for currency to change to
                //Ask for amount to change
                //Show exchange
                //Show the new currency in new account
                var selection = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Choose Account to use for exchange")
                    .AddChoices("Account 1", "Account 2", "Account 3", "Return to Main Menu"));
                switch (selection)
                {
                    case "Account 1":
                        //Access and use chosen account
                        break;
                    
                    case "Account 2":
                        //Access and use chosen account
                        break;
                    
                    case "Account 3":
                        //Access and use chosen account
                        break;
                    
                    case "Return to Main Menu":
                        SignedIn();
                        break;
                }
                
                var exchange = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("What currency do you want to use?")
                    .AddChoices("Search for currency", "Display currency", "Return to Previous Menu", "Return to Main Menu"));
                switch (exchange)
                {
                    case "Search":
                        Search();
                        break;
                    case "Display currencies":
                        Currencies();
                        break;
                    case "Return to Previous Menu":
                        continue;
                    case "Return to Main Menu":
                        SignedIn();
                        break;
                }

                break;

                static void Currencies()
                {
                    //Show currencies available to chose from
                    //let user choose 
                    ExchangingMoney();
                }

                static void Search()
                {
                    //Let user search for currencies
                    //Let user choose currency
                    ExchangingMoney();
                }

                static void ExchangingMoney()
                {
                    //Display Exchange
                    //Update accounts
                    //Return to main menu
                }
            }
        }

        static void TakeLoan()
        {
            //Ask for account to use
            //Show maximum amount of money to Loan
            //Loan transaction
            //Return to Main Menu
        }

        static void ViewTransaction()
        {
            //show accounts where transactions has been made
        }


    }

    public static async void LiveAccount3()
    {
     
    }

   
    

}