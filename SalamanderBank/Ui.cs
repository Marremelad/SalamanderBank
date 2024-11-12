using System.Media;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1.X509.Qualified;
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
    
    // Field storing user object.
    private static User? _user;

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
    
    // String representing valid email pattern.
    private static readonly string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    
    // Title screen.
    private static void TitleScreen()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        var customStyle = new Style(new Color(225, 69, 0));
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.BorderStyle = customStyle;
        table.AddColumn("[bold yellow blink on rgb(190,40,0)] Welcome to SalamanderBank![/]").Centered();

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        Console.ReadLine();
    }
    
    // Main menu display and selection handling method.
    public static async Task DisplayMainMenu()
    {
        DB.InitializeDatabase();
        TransferManager.PopulateQueueFromDB();

        await CurrencyManager.UpdateCurrenciesAsync();

        var thread = new Thread(TransferManager.ProcessQueue);
        thread.Start();
        
        TitleScreen();
        
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
                SignIn();
                break;
                
            case "Exit":
                Console.WriteLine("Thank you for using SalamanderBank!");
                Thread.Sleep(2000);
                return;
        }
    }
    
    //Second Menu after Signing in
    static void SignedIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        UserDetails();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .AddChoices("Accounts", "Transfer Funds", "Money Exchange", "Take Loan",
                    "View Transactions", "Exit")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (selection)
        {
            case "Accounts":
                BankAccounts();
                break;
            case "Transfer Funds":
                TransferFrom();
                break;
            case "Money Exchange":
                ExchangeMenu();
                throw new NotImplementedException();
                break;
            case "Take Loan":
                //TakeLoan();
                throw new NotImplementedException();
                break;
            case "View Transactions": 
                //ViewTransaction();
                throw new NotImplementedException();
                break;
            case "Exit":
                return;
        }
    }

    // Method to create a new account.
    private static void CreateAccount()
    { 
        // Set values for user information.
        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();

        // Add new user to database and create a standard bank account.
        UserManager.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName, "0707070707");
        _user = Auth.Login(_registeredEmail, _registeredPassword);
        
        AccountManager.CreateAccount(_user, "SEK", "Personal Account", 0, 1000000);
        AccountManager.CreateAccount(_user, "SEK", "Loan Account", 1);
        AccountManager.CreateAccount(_user, "SEK", "Loan Account2", 1);
        
        // Verify user account.
        VerifyAccount();
    }

    // Method for signing in to account.
    private static void SignIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        GetEmailOnSignIn();
        GetPasswordOnSignIn();
        
        SetUserValues();
        IsVerified();
        
        SignedIn();
    }

    // Method to get email input on sign in attempt.
    private static void GetEmailOnSignIn()
    {
        string? email;
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.Write(EmailDisplay);

            email = Console.ReadLine();

            string? message;
            if (email != null && Regex.IsMatch(email, EmailPattern))
            {
                if (UserManager.EmailExists(email)) break;
                
                Console.WriteLine();
                message = "\u001b[38;2;255;69;0mNo account with this email exists\u001b[0m";
                Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine();
                message = "\u001b[38;2;255;69;0mInvalid email format\u001b[0m";
                Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
                Thread.Sleep(2000);
            }
        }

        _registeredEmail = email;
    }

    // Method to get password on sign in attempt.
    private static void GetPasswordOnSignIn()
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.Write($"{EmailDisplay}\n{PasswordDisplay}");

            var password = Console.ReadLine();
            _user = Auth.Login(_registeredEmail, password);

            if (_user != null) break;
            
            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mIncorrect password\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            Thread.Sleep(2000);
        }
    }
    
    // Method to check if account is verified.
    private static void IsVerified()
    {
        if (_user?.Verified == "1") return;
        VerifyAccount();
    }
   
    
    // Method that assigns the database values to a user object.
    private static void SetUserValues()
    {
        if (_user == null) return;
        _registeredFirstName = _user.FirstName;
        _registeredLastName = _user.LastName;
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
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}");
            
            var email = Console.ReadLine();
            if (email != null && Regex.IsMatch(email, EmailPattern))
            {
                if (!UserManager.EmailExists(email))
                {
                    return email;
                }
                
                Console.WriteLine();
                var message1 = "\u001b[38;2;255;69;0mThis email is already in use\u001b[0m";
                Console.Write($"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.7)));
            }
            else
            {
                Console.WriteLine();
                var message2 = "\u001b[38;2;255;69;0mPlease enter a valid email address\u001b[0m";
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

            var password = Console.ReadLine();
            if (!string.IsNullOrEmpty(password) && password.Length >= 8)
            {
                return password;
            }
            
            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mPassword has to be at least 8 characters long\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            
            Thread.Sleep(3000);
        }
    }
    
    // Method to validate the account through a verification code.
    private static void VerifyAccount()
    {
        // Sending verification email to the registered email.
        EmailService.SendVerificationEmail(_registeredFirstName, _registeredEmail);
        
        string? code;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            var message1 = $"A code has been sent to \u001b[38;2;34;139;34m{_registeredEmail}\u001b[0m use it to verify your account.";
            var message2 = "Enter Code: ";

            Console.WriteLine();
            Console.Write($"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.45)));
            
            Console.WriteLine("\n");
            Console.Write($"{message2}\n".PadLeft(message2.Length + (Console.WindowWidth - message2.Length) / 2));
            
            Console.WriteLine();
            Console.Write("".PadLeft("".Length + (int)((Console.WindowWidth - "".Length) / 2.6)));
            
            code = Console.ReadLine();
            
        } while (string.IsNullOrEmpty(code) || code != EmailService.Code.ToString());

        UserManager.VerifyUser(_registeredEmail);
        
        SignedIn();
    }
    
    // Method to display account details in a formatted table.
    private static void UserDetails()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        var table = new Table();
        
        table.AddColumn("User Information");
        table.AddRow($"Name: {_registeredFirstName} {_registeredLastName}");
        table.AddRow($"Email: {_registeredEmail}");
        table.AddRow($"Total Balance: {GetTotalBalance():F2}");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
        table.Alignment(Justify.Center);
        AnsiConsole.Write(table);
    }

    private static decimal GetTotalBalance()
    {
        decimal totalBalance = 0;
        
        foreach (var userAccount in _user?.Accounts!)
        {
            totalBalance += userAccount.Balance;
        }

        return totalBalance;
    }

    private static void BankAccounts()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        UserDetails();
        
        if (_user?.Accounts == null) return;
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<object>()
                .PageSize(4)
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .Title("  Accounts".PadLeft(5))
                .AddChoices(_user.Accounts)
                .AddChoiceGroup("", "Main Menu")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice)
        {
            case "Main Menu":
                SignedIn();
                break;
        }

        AccountOptions((Account)choice);
    }

    private static void AccountDetails(Account account)
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        var table = new Table();
        
        table.AddColumn("Account Information");
        table.AddRow($"Name: {account.AccountName}");
        table.AddRow($"Currency: {account.CurrencyCode}");
        table.AddRow($"Balance: {account.Balance:F2}");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
        table.Alignment(Justify.Center);
        AnsiConsole.Write(table);
    }

    private static void AccountOptions(Account account)
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        AccountDetails(account);
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(3)
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .AddChoices("Change Account Name", "Return")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice)
        {
            case "Change Account Name":
                ChangeAccountName(account);
                break;
            
            case "Return":
                BankAccounts();
                break;
        }
    }
    
    private static void ChangeAccountName(Account account)
    {
        string? name;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.WriteLine();
            var message = "New Account Name: ";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 2)));
            
        } while (string.IsNullOrEmpty(name = Console.ReadLine()));
        
        account.AccountName = name;
        AccountManager.UpdateAccountName(account);
        
        AccountOptions(account);
    }

    private static void ChangeAccountCurrency(Account account)
    {
        while (true)
        {
            string? currency;
            do
            {
                Console.Clear();
                Logo.DisplayFullLogo();
            
                Console.WriteLine();
                var message = "New Currency Code: ";
                Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 2)));
            
            } while (string.IsNullOrEmpty(currency = Console.ReadLine()));

            var exchangeRate = CurrencyManager.GetExchangeRate(currency);

            if (exchangeRate == 0) continue;
            account.CurrencyCode = currency.ToUpper();
            break;
        }
        AccountOptions(account);
    }

    
    private static void TransferFrom() 
    {
        if (_user?.Accounts == null) return;
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<object>()
                .PageSize(4)
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .Title("  Choose Account To Transfer From".PadLeft(5))
                .AddChoices(_user.Accounts)
                .AddChoiceGroup("", "Main Menu")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice)
        {
            case "Main Menu":
                SignedIn();
                break;
        }

        TransferTo((Account)choice);
    }

    private static void TransferTo(Account account)
    {
        List<Account> accounts = new List<Account>();
        if (accounts == null) throw new ArgumentNullException(nameof(accounts));
        
        if (_user?.Accounts != null)
        {
            foreach (var userAccount in _user.Accounts!)
            {
                if (userAccount == account) continue;
                accounts.Add(userAccount);
            }

            if (_user?.Accounts == null) return;
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .PageSize(4)
                    .HighlightStyle(new Style(new Color(225, 69, 0)))
                    .Title("  Choose Account To Transfer To".PadLeft(5))
                    .AddChoices(accounts)
                    .AddChoiceGroup("", "Main Menu")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

            switch (choice)
            {
                case "Main Menu":
                    SignedIn();
                    break;
            }
            
            TransferFunds(account, (Account)choice);
        }
    }

    private static void TransferFunds(Account sender, Account receiver)
    {
        while (true)
        {
            if (decimal.TryParse(Console.ReadLine(), out var transfer))
            {
                if (transfer > 0 && transfer <= sender.Balance)
                {
                    TransferManager.CreateTransfer(sender, receiver, transfer);
                    break;
                }
        
                Console.WriteLine("Invalid transfer amount");
            }
        }
        
        TransferAnimation();
        SignedIn();
    }
    
    // Method to transfer funds with a loading animation.
    private static void TransferAnimation()
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
            "\n[green]Transfer complete![/]\nYou will now be redirected to the main menu.");
        
        PlaySound(@"./Sounds/cashier-quotka-chingquot-sound-effect-129698.wav");
        
        Thread.Sleep(3000);
    }

    private static void ExchangeMenu()
    {
        if(_user?.Accounts == null) return;
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<object>()
                .PageSize(5)
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .Title("    Choose Account To Exchange".PadLeft(5))
                .AddChoices(_user.Accounts)
                .AddChoiceGroup("", "Main Menu")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        switch (choice)
        {
            case "Main Menu":
                SignedIn();
                break;
        }
        Currencies((Account)choice);
    }
    private static void Currencies(Account account)
    {
        //Writes out currencies that are available for exchange
        var customStyle = new Style(new Color(225, 69, 0));

        var prompt = new SelectionPrompt<string>()
            .Title("[bold underline rgb(190,40,0)]Select an Exchange Rate[/]")
            .PageSize(10)
            .HighlightStyle(customStyle);
        
        foreach (var rate in CurrencyManager.GetAllCurrencies()!)
            prompt.AddChoice(
                $"[bold white]{rate.CurrencyCode,-5}[/] | {rate.ExchangeRate,-10} |");
        var selectedRate = AnsiConsole.Prompt(prompt);
        AnsiConsole.MarkupLine($"[bold yellow]You selected:[/] {selectedRate}. " +
                               $"\nWe will now begin the process of exchanging your money.");
        
        CurrencyConverter(selectedRate, account);
        Thread.Sleep(1000);
    }
    
     private static void CurrencyConverter(string selectedRate, Account account)
        {
            CurrencyManager.ConvertCurrency(account.Balance, account.CurrencyCode, selectedRate);
            AccountOptions(account);
            ExchangingMoney(account);
        }
    private static void ExchangingMoney(Account account) //Maybe add the currency choosen
    {
        var customStyle = new Style(new Color(225, 69, 0));
        Console.Clear();
        Logo.DisplayFullLogo();
        AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new RemainingTimeColumn { Style = customStyle },
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                // Defines task1 and task2
                var task1 = ctx.AddTask("[rgb(190,40,0)]Processing exchange request[/]");
                var task2 = ctx.AddTask("[rgb(190,40,0)]Updating account balances[/]");

                // Runs task1 to completion
                await RunTaskAsync(task1, 2, "Processing exchange request");

                // Once task1 is done, run task2
                await RunTaskAsync(task2, 1.5, "Updating account balances");
            }).GetAwaiter().GetResult();
        Console.Clear();
        Logo.DisplayFullLogo();
        AnsiConsole.MarkupLine("[bold green]Your exchange has been successfully processed.[/]"); //Needs to be centered
        //Maybe add the exchange details, currency, acronym, value etc. 
        
        AccountDetails(account);
        Console.ReadLine();
        SignedIn();

        return;

        static async Task RunTaskAsync(ProgressTask task, double incrementValue, string contextDescription)
        {
            while (!task.IsFinished)
            {
                task.Increment(incrementValue); // Increment task progress

                // Dynamically color-code the task description
                var color = task.Value < 30 ? "rgb(190,40,0)" : task.Value < 100 ? "yellow" : "green";
                task.Description = $"[bold {color}] {contextDescription} {task.Value:0}%[/]";
                await Task.Delay(250); // Simulate work 
            }
        }
    }

   
    
    // Method to play a sound from the specified file path.
    private static void PlaySound(string filePath)
    {
        using SoundPlayer soundPlayer = new(filePath);
        soundPlayer.PlaySync();
    }
}
