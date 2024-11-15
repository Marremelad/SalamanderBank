using System.Data.Entity.Infrastructure.Interception;
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
    private static string? _registeredPhoneNumber;
    
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
    
    // Display formatted phone number with padding.
    private static string PhoneNumberDisplay => $"Phone Number: {_registeredPhoneNumber}".PadLeft(
        _registeredPhoneNumber != null
            ? "Phone Number: ".Length + _registeredPhoneNumber.Length + (int)DisplayPadding : "Phone Number: ".Length + (int)DisplayPadding);
    
    // String representing valid email pattern.
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    private const string PhoneNumberPattern = @"^\+46\d{9}$";
    
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

    public static async Task RunProgram()
    {
        DB.InitializeDatabase();
        
        if (!UserManager.EmailExists("salamanderbank@gmail.com"))
        {
            UserManager.AddUser(1, $"admin", "salamanderbank@gmail.com", "Salamander", "Admin", null, 1);
        }
        
        TransferManager.PopulateQueueFromDB();
        
        await CurrencyManager.UpdateCurrenciesAsync();
        
        var thread = new Thread(TransferManager.ProcessQueue);
        thread.Start();
        
        TitleScreen();

        await DisplayMainMenu();
    }
    
    // Main menu display and selection handling method.
    private static async Task DisplayMainMenu()
    {
        EraseFields();
        
        Console.Clear();
        Logo.DisplayFullLogo();
            
        // Menu options with padding for alignment.
        string option1 = "Create Account".PadLeft("Create Account".Length + (int)((Console.WindowWidth - "Create Account".Length) / MenuPadding));
        string option2 = "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
        string option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1 ));
            
        var login = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .HighlightStyle(new Style(Color.Yellow))
                .AddChoices(option1, option2, option3));
            
        // Handling user selection from the main menu.
        switch (login.Trim())
        {
            case "Create Account":
                await CreateAccount();
                break;
                                
            case "Sign In":
                await SignIn();
                break;
            
            case "Exit":
                Environment.Exit(0);
                break;
        }
    }
    
    //Second Menu after Signing in
    private static async Task UserSignedIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        UserDetails();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Main Menu[/]")
                .AddChoices("  Accounts", "  Transfer Funds", "  Money Exchange", "  Take Loan",
                    "  View Transactions")
                .AddChoiceGroup("","[yellow]Sign Out[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (selection.Trim())
        {
            case "Accounts":
                await BankAccounts();
                break;
            
            case "Transfer Funds":
                await TransferFrom();
                break;
            
            case "Money Exchange":
                await ExchangeMenu();
                break;
            
            case "Take Loan":
                await DepositLoanIn();
                break;
            
            case "View Transactions": 
                //ViewTransaction();
                throw new NotImplementedException();
            
            case "[yellow]Sign Out[/]":
                await DisplayMainMenu();
                break;
        }
    }

    // Method to create a new account.
    private static async Task CreateAccount()
    { 
        // Set values for user information.
        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();
        _registeredPhoneNumber = GetPhoneNumber();

        // Add new user to database and create a standard bank account.
        UserManager.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName, _registeredPhoneNumber);
        _user = Auth.Login(_registeredEmail, _registeredPassword);

        CreateDefaultBankAccounts(_user);
        
        // Verify user account.
        await VerifyAccount();
    }

    private static void CreateDefaultBankAccounts(User? user)
    {
        AccountManager.CreateAccount(user, "SEK", "Personal Account", 0, 1000000);
        AccountManager.CreateAccount(user, "SEK", "Loan Account", 1);
        AccountManager.CreateAccount(user, "SEK", "Savings Account", 2);
    }

    // Method for signing in to account.
    private static async Task SignIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        GetEmailOnSignIn();

        if (GetPasswordOnSignIn())
        {
            SetUserValues();
            
            if (IsLocked() && _user?.Type == 0)
            {
                Console.WriteLine();
                var message = "\u001b[38;2;255;69;0mThis Account is Locked\u001b[0m";
                Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            
            await IsVerified();
            
            if (_user is { Type: 1 })
            {
                await AdminSignedIn();
            }
            else
            {
                await UserSignedIn();
            }
        }
        else
        {
            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mThis Account has been Locked due to too many sign in attempts\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            Thread.Sleep(2000);
            Environment.Exit(0);
        }
        
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
    private static bool GetPasswordOnSignIn()
    {
        int signInAttempt = 0;
        while (true)
        {
            if (signInAttempt == 3)
            {
               UserManager.UpdateUserLock(_registeredEmail, 1);
               return false;
            }
            
            Console.Clear();
            Logo.DisplayFullLogo();
            Console.Write($"{EmailDisplay}\n{PasswordDisplay}");

            var password = Console.ReadLine();
            _user = Auth.Login(_registeredEmail, password);

            signInAttempt += 1;
            
            if (_user != null) break;
            
            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mIncorrect password\u001b[0m";
            Console.Write($"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
            Thread.Sleep(2000);
        }

        return true;
    }
    
    // Method to check if account is verified.
    private static async Task IsVerified()
    {
        if (_user?.Verified == "1") return;
        await VerifyAccount();
    }

    private static bool IsLocked()
    {
        return _user?.Locked == 1;
    }
   
    
    // Method that assigns the database values to a user object.
    private static void SetUserValues()
    {
        if (_user == null) return;
        _registeredFirstName = _user.FirstName;
        _registeredLastName = _user.LastName;
        _registeredPhoneNumber = _user.PhoneNumber;
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

    private static string GetPhoneNumber()
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}\n{PasswordDisplay}\n{PhoneNumberDisplay}");
            
            var phoneNumber = Console.ReadLine();
            if (phoneNumber != null && Regex.IsMatch(phoneNumber, PhoneNumberPattern))
            {
                
                if (!UserManager.PhoneNumberExists(phoneNumber))
                {
                    return phoneNumber;
                }
                Console.WriteLine();
                var message1 = "\u001b[38;2;255;69;0mThis phone number is already in use\u001b[0m";
                Console.Write($"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.7)));
            }
            else
            {
                Console.WriteLine();
                var message2 = "\u001b[38;2;255;69;0mPlease enter a valid phone number with '+46' at the beginning\u001b[0m";
                Console.Write($"{message2}".PadLeft(message2.Length + (int)((Console.WindowWidth - message2.Length) / 1.7)));
            }
            

            Thread.Sleep(2000);
        }
    }
    
    // Method to validate the account through a verification code.
    private static async Task VerifyAccount()
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
        
        await UserSignedIn();
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
        table.AddRow($"Phone Number: {_registeredPhoneNumber}");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
        table.Alignment(Justify.Center);
        AnsiConsole.Write(table);
    }
    
    private static async Task BankAccounts()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        UserDetails();
        
        if (_user?.Accounts == null) return;

        // Create a dictionary to map indented account display names to their Account objects.
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Account Options[/]") // Keep title indentation consistent.
                .AddChoices(indentedAccounts.Keys) // Display indented account names.
                .AddChoiceGroup("", "[green]Create new Account[/]")
                .AddChoiceGroup("", "[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (choice)
        { 
            case "[green]Create new Account[/]":
                await UserChooseNewAccountType();
                break;
            
            case "[yellow]Main Menu[/]":
                await UserSignedIn();
                break;
            
            default:
                await AccountOptions(indentedAccounts[choice]);
                break;
        }
    }

    private static async Task UserChooseNewAccountType()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        UserDetails();
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Choose what Type of Account you want to create[/]")
                .AddChoices("  Personal Account", "  Savings Account")
                .AddChoiceGroup("","[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (choice.Trim())
        {
            case "Personal Account":
                await UserCreateNewAccount(0);
                break;
            
            case "Savings Account":
                await UserCreateNewAccount(1);
                break;
            
            case "[yellow]Main Menu[/]":
                await UserSignedIn();
                break;
        }
    }

    private static async Task UserCreateNewAccount(int type)
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            UserDetails();
            
            string message = "";
            switch (type)
            {
                case 0:
                    message = "You have chosen to create a personal account. The interest rate of this type of account is 0%";
                    break;
                
                case 1:
                    message = "You have chosen to create a savings account. The interest rate of this type of account is 30%";
                    break;
            }
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.Black, Color.Yellow))
                    .Title($"[bold underline rgb(190,40,0)]    {message}[/]")
                    .AddChoices("  Account Name")
                    .AddChoiceGroup("","[yellow]Return[/]")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

            if (choice.Trim() == "Account Name")
            {
                Console.Write("Choose an account name: ");
                var accountName = Console.ReadLine();
            
                if (!string.IsNullOrEmpty(accountName))
                {
                    if (AccountManager.CreateAccount(_user, "SEK", accountName, type)) break;

                    Console.WriteLine("\u001b[38;2;255;69;0mAn account with this name already exists\u001b[0m");
                    Thread.Sleep(3000);
                
                }
                else
                {
                    Console.WriteLine("\u001b[38;2;255;69;0mAccount name can not be empty\u001b[0m");
                    Thread.Sleep(3000);
                }
            }
            else
            {
                await UserChooseNewAccountType();
            }
        }

        await UserSignedIn();
    }

    private static void AccountDetails(Account account)
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        var table = new Table();
        
        table.AddColumn("Account Information");
        table.AddRow($"Name: {account.AccountName}");
        table.AddRow($"Currency: {account.CurrencyCode}");
        table.AddRow($"Interest: {account.Interest}%");
        table.AddRow($"Balance: {account.Balance:F2}");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
        table.Alignment(Justify.Center);
        AnsiConsole.Write(table);
    }

    private static async Task AccountOptions(Account account)
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        AccountDetails(account);
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(3)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Account Options[/]")
                .AddChoices("  Change Account Name")
                .AddChoiceGroup("","[yellow]Return[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice.Trim())
        {
            case "Change Account Name":
                await ChangeAccountName(account);
                break;
            
            case "[yellow]Return[/]":
                await BankAccounts();
                break;
        }
    }
    
    private static async Task ChangeAccountName(Account account)
    {
        string? name;
        do
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            
            Console.WriteLine();
            var message = "New Account Name: ";
            Console.Write($"{message}".PadLeft(message.Length + ((Console.WindowWidth - message.Length) / 2)));
            
        } while (string.IsNullOrEmpty(name = Console.ReadLine()));
        
        account.AccountName = name;
        AccountManager.UpdateAccountName(account);
        
        await AccountOptions(account);
    }
    
    private static async Task TransferFrom() 
    {
        if (_user?.Accounts == null) return;
        
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to Transfer from[/]".PadLeft(5))
                .AddChoices(indentedAccounts.Keys)
                .AddChoiceGroup("", "[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                await UserSignedIn();
                break;
            
            default:
                await TransferTo(indentedAccounts[choice]);
                break;
        }
        
    }

    private static async Task TransferTo(Account senderAccount)
    {
        var accounts = new List<Account>();
        if (accounts == null) throw new ArgumentNullException(nameof(accounts));
        
        if (_user?.Accounts != null)
        {
            accounts.AddRange(_user.Accounts!.Where(account => account != senderAccount));
            
            var indentedAccounts = accounts
                .ToDictionary(account => $"  {account.AccountName}", account => account);
                
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.Black, Color.Yellow))
                    .Title("[bold underline rgb(190,40,0)]    Chose an Account to Transfer to[/]".PadLeft(5))
                    .AddChoices(indentedAccounts.Keys)
                    .AddChoiceGroup("", "[yellow]Main Menu[/]")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

            switch (choice)
            {
                case "[yellow]Main Menu[/]":
                    await UserSignedIn();
                    break;
                
                default:
                    await TransferFunds(senderAccount, indentedAccounts[choice]);
                    break;
            }
        }
    }

    private static async Task TransferFunds(Account sender, Account receiver)
    {
        AccountDetails(receiver);
        
        Console.Write("Amount to Transfer: ");
        
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
        
        await UserSignedIn();
    }
    
    // Method to transfer funds with a loading animation.
    private static void TransferAnimation()
    {
        Console.Clear();
        Logo.DisplayFullLogo();
        
        UserDetails();
        
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
        
        UserDetails();
        
        AnsiConsole.MarkupLine(
            "\n[green]Transfer complete![/]\nYou will now be redirected to the main menu.");
        
        PlaySound(@"./Sounds/cashier-quotka-chingquot-sound-effect-129698.wav");
        
        Thread.Sleep(3000);
    }

    private static async Task ExchangeMenu()
    {
        if (_user?.Accounts == null) return;
        
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.Yellow ))
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to Exchange[/]".PadLeft(5))
                .AddChoices(indentedAccounts.Keys)
                .AddChoiceGroup("", "[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                await UserSignedIn();
                break;
            
            default:
                await Currencies(indentedAccounts[choice]);
                break;
        }
    }

    private static async Task Currencies(Account account)
    {
        var currencyMap  = CurrencyManager.GetAllCurrencies();

        if (currencyMap != null)
        {
            var indentedCurrencies =
                currencyMap.ToDictionary(currency => $"  {currency.CurrencyCode, -5} | {currency.ExchangeRate, -10}",
                    currency => currency.CurrencyCode);
            
            if (account.Balance < decimal.Zero)
            {
                AnsiConsole.MarkupLine("[red]Not enough balance![/]");
                return;
            }
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(20)
                    .HighlightStyle(new Style(Color.Black, Color.Yellow))
                    .Title("[bold underline rgb(190,40,0)]    Select an Exchange Rate[/]".PadLeft(5))
                    .AddChoices("[yellow]  Main Menu         [/]")
                    .AddChoices(indentedCurrencies.Keys)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

            switch (choice.Trim())
            {
                case "[yellow]  Main Menu         [/]":
                    await UserSignedIn();
                    break;
            }
            
            await CurrencyConverter(account.CurrencyCode, indentedCurrencies[choice], account);
            Thread.Sleep(1000);
        }
    }

    private static async Task CurrencyConverter(string convertFrom, string convertTo, Account account)
    {
        try
        {
            var convertedAmount = CurrencyManager.ConvertCurrency(account.Balance, convertFrom, convertTo);

            account.Balance = convertedAmount;
            account.CurrencyCode = convertTo;
            
            await ExchangeAnimation(account, convertFrom, convertTo, convertedAmount);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
        }

        await AccountOptions(account);
    }

    private static async Task ExchangeAnimation(Account account, string fromCurrency, string toCurrency, decimal amount)
    {
        var customStyle = new Style(new Color(225, 69, 0));
        Console.Clear();
        Logo.DisplayFullLogo();
        
        UserDetails();
        
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
                await RunTaskAsync(task1, 10, "Processing exchange request");

                // Once task1 is done, run task2
                await RunTaskAsync(task2, 5, "Updating account balances");
            }).GetAwaiter().GetResult();

        Console.Clear();
        Logo.DisplayFullLogo();
        
        AccountDetails(account);
        
        var message1 = "\u001b[38;2;34;139;34mYour exchange has been successfully processed.\u001b[0m";
        Console.WriteLine($"{message1}");
        
        var message2 = $"\u001b[38;2;225;255;0mYou have exchanged from\u001b[0m \u001b[38;2;255;69;0m{fromCurrency}\u001b[0m \u001b[38;2;225;255;0m to \u001b[0m \u001b[38;2;255;69;0m{toCurrency}\u001b[0m. ";
        Console.WriteLine($"{message2}");
        
        var message3 = $"\u001b[38;2;225;255;0mFinal Amount in {toCurrency}:\u001b[0m \u001b[38;2;255;69;0m{amount:f2}\u001b[0m";
        Console.WriteLine($"{message3}");

        Console.WriteLine("\nPress any Key to Continue");
        
        Console.ReadLine();
       
        await UserSignedIn();
    }

    private static async Task RunTaskAsync(ProgressTask task, double incrementValue, string contextDescription)
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
    
    private static async Task DepositLoanIn()
    {
        if (_user?.Accounts == null) return;
        
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to deposit your Loan in[/]".PadLeft(5))
                .AddChoices(indentedAccounts.Keys)
                .AddChoiceGroup("", "[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                await UserSignedIn();
                break;
            
            default:
                await AmountToLoan(indentedAccounts[choice]);
                break;
        }
    }

    private static async Task AmountToLoan(Account account)
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();
            AccountDetails(account);
            
            decimal amount;
            while (true)
            {
                Console.Write("Amount to Loan: ");
                if (decimal.TryParse(Console.ReadLine(), out amount)) break;
            }

            var loan = LoanManager.CreateLoan(_user, account, amount);
            
            if (loan != null)
            {
                Console.WriteLine("\u001b[38;2;34;139;34mYour Loan was successfully processed\u001b[0m");
                Console.WriteLine(loan);
                break;
            }

            Console.WriteLine();
            Console.WriteLine($"\u001b[38;2;255;69;0mYour only allowed to Loan {LoanManager.LoanAmountAllowed(_user, account)}  {account.CurrencyCode}\u001b[0m");
            Thread.Sleep(3000);
        }
        
        
        Console.WriteLine("\nPress any Key to Continue");
        Console.ReadLine();
       
        await UserSignedIn();
    }
     
    private static async Task AdminSignedIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(Color.Black, Color. Yellow))
                .Title("[bold underline rgb(190,40,0)]    Admin Menu[/]")
                .AddChoices("  Create User Account", "  Remove User Account")
                .AddChoiceGroup("", "[yellow]Sign Out[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));
        
        switch (selection.Trim())
        {
            case "Create User Account":
                await AdminCreateNewUser();
                break;
            
            case "Remove User Account":
                throw new NotImplementedException();
            
            case "[yellow]Sign Out[/]":
                await DisplayMainMenu();
                break;
        }
    }

    private static async Task AdminCreateNewUser()
    {
        EraseFields();

        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();
        _registeredPhoneNumber = GetPhoneNumber();
        
        UserManager.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName, _registeredPhoneNumber);
        
        var user = Auth.Login(_registeredEmail, _registeredPassword);
        CreateDefaultBankAccounts(user);
        
        ResetAdminFields();
        
        AnsiConsole.WriteLine("\nUser was created successfully!");
        Thread.Sleep(2000);
        await AdminSignedIn();
    }

    private static void EraseFields()
    {
        _registeredFirstName = "";
        _registeredLastName = "";
        _registeredEmail = "";
        _registeredPassword = "";
        _registeredPhoneNumber = "";
    }

    private static void ResetAdminFields()
    {
        _registeredFirstName = "Salamander";
        _registeredLastName = "Bank";
        _registeredEmail = "salamanderbank@gmail.com";
    }
    
    // Method to play a sound from the specified file path.
    private static void PlaySound(string filePath)
    {
        using SoundPlayer soundPlayer = new(filePath);
        soundPlayer.PlaySync();
    }
}
