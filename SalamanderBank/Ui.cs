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
    private static string FirstNameDisplay => $"First Name: {_registeredFirstName}".PadLeft(_registeredFirstName != null
        ? "First Name: ".Length + _registeredFirstName.Length + (int)DisplayPadding
        : "First Name: ".Length + (int)DisplayPadding);

    // Display formatted last name with padding.
    private static string LastNameDisplay => $"Last Name: {_registeredLastName}".PadLeft(_registeredLastName != null
        ? "Last Name: ".Length + _registeredLastName.Length + (int)DisplayPadding
        : "Last Name: ".Length + (int)DisplayPadding);

    // Display formatted email with padding.
    private static string EmailDisplay => $"Email: {_registeredEmail}".PadLeft(_registeredEmail != null
        ? "Email: ".Length + _registeredEmail.Length + (int)DisplayPadding
        : "Email: ".Length + (int)DisplayPadding);

    // Display formatted password with padding.
    private static string PasswordDisplay => $"Password: {_registeredPassword}".PadLeft(_registeredPassword != null
        ? "Password: ".Length + _registeredPassword.Length + (int)DisplayPadding
        : "Password: ".Length + (int)DisplayPadding);

    // Display formatted phone number with padding.
    private static string PhoneNumberDisplay => $"Phone Number: {_registeredPhoneNumber}".PadLeft(
        _registeredPhoneNumber != null
            ? "Phone Number: ".Length + _registeredPhoneNumber.Length + (int)DisplayPadding
            : "Phone Number: ".Length + (int)DisplayPadding);

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
        Db.InitializeDatabase();

        if (!UserManager.EmailExists("salamanderbank@gmail.com"))
        {
            UserManager.AddUser(1, $"admin", "salamanderbank@gmail.com", "Salamander", "Admin", null, 1);
        }

        TransferManager.PopulateQueueFromDb();

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
        string option1 = "Create Account".PadLeft("Create Account".Length +
                                                  (int)((Console.WindowWidth - "Create Account".Length) / MenuPadding));
        string option2 =
            "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
        string option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1));

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
                .AddChoiceGroup("", "[yellow]Sign Out[/]")
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
                await TransactionHistory();
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
        UserManager.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName,
            _registeredPhoneNumber);
        _user = Auth.Login(_registeredEmail, _registeredPassword);

        CreateDefaultBankAccounts(_user);

        // Verify user account.
        await VerifyAccount();
    }

    private static void CreateDefaultBankAccounts(User? user)
    {
        AccountManager.CreateAccount(user, "SEK", "Personal Account", 0, 1000000);
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
                Console.Write(
                    $"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
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
                Console.Write(
                    $"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine();
                message = "\u001b[38;2;255;69;0mInvalid email format\u001b[0m";
                Console.Write(
                    $"{message}".PadLeft(message.Length + (int)((Console.WindowWidth - message.Length) / 1.7)));
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
                Console.Write(
                    $"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.7)));
            }
            else
            {
                Console.WriteLine();
                var message2 = "\u001b[38;2;255;69;0mPlease enter a valid email address\u001b[0m";
                Console.Write(
                    $"{message2}".PadLeft(message2.Length + (int)((Console.WindowWidth - message2.Length) / 1.7)));
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

    // Method to get the user's phone number, ensuring it's valid and not already in use
    private static string GetPhoneNumber()
    {
        while (true)
        {
            // Clears the console and displays the full logo
            Console.Clear();
            Logo.DisplayFullLogo();

            // Display user-related information prompts before entering the phone number
            Console.Write(
                $"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}\n{PasswordDisplay}\n{PhoneNumberDisplay}");

            // Reads the phone number entered by the user
            var phoneNumber = Console.ReadLine();

            // Check if the phone number is valid using a regex pattern
            if (phoneNumber != null && Regex.IsMatch(phoneNumber, PhoneNumberPattern))
            {
                // Check if the phone number is already in use in the system
                if (!UserManager.PhoneNumberExists(phoneNumber))
                {
                    return phoneNumber; // Return the phone number if valid and not in use
                }

                // Display a message if the phone number already exists
                Console.WriteLine();
                var message1 = "\u001b[38;2;255;69;0mThis phone number is already in use\u001b[0m";
                Console.Write(
                    $"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.7)));
            }
            else
            {
                // Display a message if the phone number is invalid
                Console.WriteLine();
                var message2 =
                    "\u001b[38;2;255;69;0mPlease enter a valid phone number with '+46' at the beginning\u001b[0m";
                Console.Write(
                    $"{message2}".PadLeft(message2.Length + (int)((Console.WindowWidth - message2.Length) / 1.7)));
            }

            // Pause for 2 seconds before allowing the user to try again
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

            var message1 =
                $"A code has been sent to \u001b[38;2;34;139;34m{_registeredEmail}\u001b[0m use it to verify your account.";
            var message2 = "Enter Code: ";

            Console.WriteLine();
            Console.Write(
                $"{message1}".PadLeft(message1.Length + (int)((Console.WindowWidth - message1.Length) / 1.45)));

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

    // Method to display the user's bank accounts and options for creating a new account or navigating to the main menu.
    private static async Task BankAccounts()
    {
        // Clears the console and displays the full logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Displays user details like name, email, etc.
        UserDetails();

        // Check if the user has any accounts
        if (_user?.Accounts == null) return;

        // Create a dictionary to map indented account display names to their Account objects.
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);

        // Prompt the user to select an account or option
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10) // Number of options per page in the selection prompt
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Set highlight style for selected option
                .Title("[bold underline rgb(190,40,0)]    Account Options[/]") // Title of the selection prompt
                .AddChoices(indentedAccounts.Keys) // Display indented account names for the user to choose from
                .AddChoiceGroup("", "[green]Create new Account[/]") // Option to create a new account
                .AddChoiceGroup("", "[yellow]Main Menu[/]") // Option to go back to the main menu
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")); // More options text

        // Process the user's selection
        switch (choice)
        {
            case "[green]Create new Account[/]":
                // If the user selects "Create new Account," prompt them to choose the type of account to create
                await UserChooseNewAccountType();
                break;

            case "[yellow]Main Menu[/]":
                // If the user selects "Main Menu," navigate back to the signed-in user screen
                await UserSignedIn();
                break;

            default:
                // If the user selects an existing account, show options for that account
                await AccountOptions(indentedAccounts[choice]);
                break;
        }
    }


    // Method to allow the user to choose the type of account they want to create (Personal or Savings).
    private static async Task UserChooseNewAccountType()
    {
        // Clears the console and displays the full logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Displays user details like name, email, etc.
        UserDetails();

        // Prompt the user to select the type of account they want to create
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10) // Number of options per page in the selection prompt
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight style for selected option
                .Title(
                    "[bold underline rgb(190,40,0)]    Choose what Type of Account you want to create[/]") // Title of the selection prompt
                .AddChoices("  Personal Account", "  Savings Account") // Add the options for account types
                .AddChoiceGroup("", "[yellow]Main Menu[/]") // Option to go back to the main menu
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")); // More options text

        // Process the user's selection
        switch (choice.Trim())
        {
            case "Personal Account":
                // If the user selects "Personal Account," create a personal account
                await UserCreateNewAccount(0);
                break;

            case "Savings Account":
                // If the user selects "Savings Account," create a savings account
                await UserCreateNewAccount(1);
                break;

            case "[yellow]Main Menu[/]":
                // If the user selects "Main Menu," navigate back to the signed-in user screen
                await UserSignedIn();
                break;
        }
    }


    // Method to allow the user to create a new account based on the selected type (Personal or Savings).
    private static async Task UserCreateNewAccount(int type)
    {
        // Infinite loop to repeatedly ask for account creation until it's successful
        while (true)
        {
            // Clears the console and displays the full logo
            Console.Clear();
            Logo.DisplayFullLogo();

            // Displays user details like name, email, etc.
            UserDetails();

            // Set the message based on the account type selected by the user
            string message = "";
            switch (type)
            {
                case 0:
                    // Message for creating a personal account with a 0% interest rate
                    message =
                        "You have chosen to create a personal account. The interest rate of this type of account is 0%";
                    break;

                case 1:
                    // Message for creating a savings account with a 30% interest rate
                    message =
                        "You have chosen to create a savings account. The interest rate of this type of account is 30%";
                    break;
            }

            // Prompt the user to either enter an account name or return to the previous menu
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10) // Number of options per page in the selection prompt
                    .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight style for selected option
                    .Title(
                        $"[bold underline rgb(190,40,0)]    {message}[/]") // Title with dynamic message based on account type
                    .AddChoices("  Account Name") // Option to input account name
                    .AddChoiceGroup("", "[yellow]Return[/]") // Option to return to the previous menu
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")); // More options text

            // If the user chooses to enter an account name
            if (choice.Trim() == "Account Name")
            {
                // Ask the user to input the account name
                Console.Write("Choose an account name: ");
                var accountName = Console.ReadLine();

                // If the account name is not empty
                if (!string.IsNullOrEmpty(accountName))
                {
                    // Try to create the account with the specified details
                    if (AccountManager.CreateAccount(_user, "SEK", accountName, type)) break;

                    // If account creation fails due to an existing account name
                    Console.WriteLine("\u001b[38;2;255;69;0mAn account with this name already exists\u001b[0m");
                    Thread.Sleep(3000);
                }
                else
                {
                    // If the account name is empty, notify the user
                    Console.WriteLine("\u001b[38;2;255;69;0mAccount name can not be empty\u001b[0m");
                    Thread.Sleep(3000);
                }
            }
            else
            {
                // If the user chooses to return, go back to the account type selection screen
                await UserChooseNewAccountType();
            }
        }

        // After successfully creating an account, navigate to the signed-in user screen
        await UserSignedIn();
    }


    // Method to display account details in a formatted table
    private static void AccountDetails(Account account)
    {
        // Clears the console and displays the full logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Create a new table to display account details
        var table = new Table();

        // Add column and rows to the table to display account information
        table.AddColumn("Account Information");
        table.AddRow($"Name: {account.AccountName}");
        table.AddRow($"Currency: {account.CurrencyCode}");
        table.AddRow($"Interest: {account.Interest}%");
        table.AddRow($"Balance: {account.Balance:F2}");

        // Style the table with rounded borders and a red border style
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
        table.Alignment(Justify.Center);

        // Display the table in the console using Spectre.Console
        AnsiConsole.Write(table);
    }

    // Method to handle account options (like changing account name or returning to the main account list)
    private static async Task AccountOptions(Account account)
    {
        // Clears the console, displays the logo, and shows the account details
        Console.Clear();
        Logo.DisplayFullLogo();
        AccountDetails(account);

        // Prompt the user with options to change the account name or return
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(3) // Limit the number of options displayed at once
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Style for highlighted options
                .Title("[bold underline rgb(190,40,0)]    Account Options[/]") // Title of the options menu
                .AddChoices("  Change Account Name") // Option to change account name
                .AddChoiceGroup("", "[yellow]Return[/]") // Option to return to the main menu
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more options)[/]")); // Text displayed for more choices

        // Switch statement based on the user's choice
        switch (choice.Trim())
        {
            case "Change Account Name":
                // Call method to change the account name
                await ChangeAccountName(account);
                break;

            case "[yellow]Return[/]":
                // Call method to return to the main bank accounts menu
                await BankAccounts();
                break;
        }
    }

    // Method to handle changing the account name
    private static async Task ChangeAccountName(Account account)
    {
        string? name;

        // Repeatedly prompt the user until a valid account name is provided
        do
        {
            // Clears the console and displays the full logo
            Console.Clear();
            Logo.DisplayFullLogo();

            // Prompt for a new account name, centered in the console
            Console.WriteLine();
            var message = "New Account Name: ";
            Console.Write($"{message}".PadLeft(message.Length + ((Console.WindowWidth - message.Length) / 2)));

            // Repeat until the name is not null or empty
        } while (string.IsNullOrEmpty(name = Console.ReadLine()));

        // Update the account name and save it using the AccountManager
        account.AccountName = name;
        AccountManager.UpdateAccountName(account);

        // After changing the account name, show the account options again
        await AccountOptions(account);
    }


    // Method to prompt the user to select an account to transfer funds from
    private static async Task TransferFrom()
    {
        // If the user does not have any accounts, exit the method
        if (_user?.Accounts == null) return;

        // Create a dictionary to map indented account display names to their Account objects
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);

        // Prompt the user to choose an account to transfer from, using a SelectionPrompt
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5) // Display 5 options at a time
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight the selected option with yellow
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to Transfer from[/]"
                    .PadLeft(5)) // Title for the prompt
                .AddChoices(indentedAccounts.Keys) // Add the account names as choices
                .AddChoiceGroup("", "[yellow]Main Menu[/]") // Option to return to the main menu
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more options)[/]")); // Additional prompt for more choices

        // Switch statement to handle the user's choice
        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                // If "Main Menu" is selected, return to the user sign-in menu
                await UserSignedIn();
                break;

            default:
                // If a valid account is selected, proceed to transfer funds to another account
                await TransferTo(_user, indentedAccounts[choice]);
                break;
        }
    }

    // Method to prompt the user to select an account to transfer funds to
    private static async Task TransferTo(User user, Account senderAccount)
    {
        // Create a list to hold the recipient accounts
        var accounts = new List<Account>();

        // Check if the accounts list is null (this check is unnecessary since it's instantiated right before)
        if (accounts == null) throw new ArgumentNullException(nameof(accounts));

        // If the user has accounts, exclude the sender account from the list
        if (user.Accounts != null)
        {
            accounts.AddRange(user.Accounts!.Where(account => account != senderAccount));

            // Create a dictionary to map indented account display names to their Account objects
            var indentedAccounts = accounts
                .ToDictionary(account => $"  {account.AccountName}", account => account);

            // Prompt the user to choose an account to transfer to, using a SelectionPrompt
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(10) // Display 10 options at a time
                    .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight the selected option with yellow
                    .Title("[bold underline rgb(190,40,0)]    Chose an Account to Transfer to[/]"
                        .PadLeft(5)) // Title for the prompt
                    .AddChoices(indentedAccounts.Keys) // Add the account names as choices
                    .AddChoiceGroup("",
                        "[green]Transfer to another user[/]") // Option to transfer funds to another user
                    .AddChoiceGroup("", "[yellow]Main Menu[/]") // Option to return to the main menu
                    .MoreChoicesText(
                        "[grey](Move up and down to reveal more options)[/]")); // Additional prompt for more choices

            // Switch statement to handle the user's choice
            switch (choice)
            {
                case "[yellow]Main Menu[/]":
                    // If "Main Menu" is selected, return to the user sign-in menu
                    await UserSignedIn();
                    break;
                case "[green]Transfer to another user[/]":
                    // If "Transfer to another user" is selected, proceed to select a user
                    await SelectUser(senderAccount);
                    break;
                default:
                    // If a valid account is selected, proceed to transfer funds to that account
                    await TransferFunds(senderAccount, indentedAccounts[choice]);
                    break;
            }
        }
    }


    // Method to prompt the user to select another user for transferring funds to
    private static async Task SelectUser(Account senderAccount)
    {
        // Search for all users and exclude the current user and the system email
        List<User> users = UserManager.SearchUser("");
        users.RemoveAll(u => u.Email == _user?.Email || u.Email == "salamanderbank@gmail.com");

        // Create a dictionary to map indented user emails to their User objects
        var indentedUsers = users
            .ToDictionary(user => $"  {user.Email}", user => user);

        // Prompt the user to choose a recipient user for the transfer
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10) // Display 10 options at a time
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight the selected option with yellow
                .Title("[bold underline rgb(190,40,0)]    Chose a User to Transfer to[/]"
                    .PadLeft(5)) // Title for the prompt
                .AddChoices(indentedUsers.Keys) // Add the user emails as choices
                .AddChoiceGroup("", "[yellow]Main Menu[/]") // Option to return to the main menu
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more options)[/]")); // Additional prompt for more choices

        // Switch statement to handle the user's choice
        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                // If "Main Menu" is selected, return to the user sign-in menu
                await UserSignedIn();
                break;
            default:
                // If a valid user is selected, proceed to transfer funds to that user
                await TransferTo(indentedUsers[choice], senderAccount);
                break;
        }
    }

    // Method to handle transferring funds between two accounts
    private static async Task TransferFunds(Account sender, Account receiver)
    {
        // Display the details of the receiver's account
        AccountDetails(receiver);

        // Prompt the user to input the amount to transfer
        Console.Write("Amount to Transfer: ");

        // Loop until a valid transfer amount is entered
        while (true)
        {
            // Try parsing the input into a decimal value
            if (decimal.TryParse(Console.ReadLine(), out var transfer))
            {
                // If the transfer amount is valid (greater than 0 and less than or equal to sender's balance)
                if (transfer > 0 && transfer <= sender.Balance)
                {
                    // Create the transfer
                    TransferManager.CreateTransfer(sender, receiver, transfer);
                    break;
                }

                // If the transfer amount is invalid, display an error message
                Console.WriteLine("Invalid transfer amount");
            }
        }

        // Perform any animations related to the transfer
        TransferAnimation();

        // Return to the user sign-in menu
        await UserSignedIn();
    }

    // Method to simulate an animation during a fund transfer process
    private static void TransferAnimation()
    {
        // Clear the console and display the logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Display the user's details
        UserDetails();

        // Start a status animation to indicate the transfer process is ongoing
        AnsiConsole.Status()
            .AutoRefresh(true) // Automatically refreshes the status during the process
            .Spinner(Spinner.Known.Dots) // Use a dot spinner to indicate activity
            .SpinnerStyle(Style.Parse("yellow bold")) // Set the spinner style to bold yellow
            .Start("[yellow]Transferring money...[/]", _ =>
            {
                // Display status messages simulating the transfer process with delays
                AnsiConsole.MarkupLine("[yellow]Checking Account Balance...[/]");
                Thread.Sleep(3000); // Simulate a 3-second delay for balance check

                AnsiConsole.MarkupLine("[yellow]Checking Receiver...[/]");
                Thread.Sleep(3000); // Simulate a 3-second delay for checking the receiver
            });

        // Clear the console again after the animation and display the logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Display the user's details again after the transfer process
        UserDetails();

        // Display a message to indicate the transfer is complete
        AnsiConsole.MarkupLine(
            "\n[green]Transfer complete![/]\nYou will now be redirected to the main menu.");

        // Play a sound effect to signify the transfer completion
        PlaySound(@"./Sounds/cashier-quotka-chingquot-sound-effect-129698.wav");

        // Pause for 3 seconds to allow the user to see the message before proceeding
        Thread.Sleep(3000);
    }

    // Method to prompt the user to select an account for currency exchange
    private static async Task ExchangeMenu()
    {
        // If the user has no accounts, exit the method
        if (_user?.Accounts == null) return;

        // Create a dictionary to map indented account names to their Account objects
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);

        // Prompt the user to choose an account for exchange
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(10) // Display 10 options at a time
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight the selected option in yellow
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to Exchange[/]"
                    .PadLeft(5)) // Title for the prompt
                .AddChoices(indentedAccounts.Keys) // Add the account names to the choices list
                .AddChoiceGroup("", "[yellow]Main Menu[/]") // Add an option for returning to the main menu
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")); // Display more choices message

        // Handle the user's selection from the prompt
        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                // If the user selects the Main Menu option, redirect to the sign-in method
                await UserSignedIn();
                break;

            default:
                // If the user selects an account, proceed to currency exchange for that account
                await Currencies(indentedAccounts[choice]);
                break;
        }
    }


    // Method to handle currency exchange for a given account
    private static async Task Currencies(Account account)
    {
        // Get all available currencies from the CurrencyManager
        var currencyMap = CurrencyManager.GetAllCurrencies();

        // Create a dictionary mapping indented currency details (code and exchange rate) to their currency codes
        Dictionary<string, string?> indentedCurrencies =
            currencyMap.ToDictionary(currency => $"  {currency.CurrencyCode,-5} | {currency.ExchangeRate,-10}",
                currency => currency.CurrencyCode);

        // If the account balance is negative, display an error message and exit the method
        if (account.Balance < decimal.Zero)
        {
            AnsiConsole.MarkupLine("[red]Not enough balance![/]"); // Notify user of insufficient balance
            return; // Exit the method as no exchange can occur
        }

        // Prompt the user to select an exchange rate or return to the main menu
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(20) // Display 20 choices at a time
                .HighlightStyle(new Style(Color.Black, Color.Yellow)) // Highlight the selected choice in yellow
                .Title("[bold underline rgb(190,40,0)]    Select an Exchange Rate[/]".PadLeft(5)) // Title of the prompt
                .AddChoices("[yellow]  Main Menu         [/]") // Option to return to the main menu
                .AddChoices(indentedCurrencies.Keys) // Add available currency options for selection
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more options)[/]")); // Instruction for navigating choices

        // Switch case for handling the user's selection
        switch (choice.Trim())
        {
            // If 'Main Menu' is selected, return to the main menu
            case "[yellow]  Main Menu         [/]":
                await UserSignedIn(); // Navigate back to user sign-in screen
                break;
        }

        // Call method to convert the currency for the selected exchange
        await CurrencyConverter(account.CurrencyCode, indentedCurrencies[choice], account);
        Thread.Sleep(1000); // Sleep for a brief moment before continuing
    }

    // Method to convert the currency from one type to another
    private static async Task CurrencyConverter(string? convertFrom, string? convertTo, Account account)
    {
        try
        {
            // Attempt to convert the balance of the account
            var convertedAmount = CurrencyManager.ConvertCurrency(account.Balance, convertFrom, convertTo);

            // Update the account's balance and currency code
            account.Balance = convertedAmount;
            account.CurrencyCode = convertTo;

            // Call method to show exchange animation and update user interface
            await ExchangeAnimation(account, convertFrom, convertTo, convertedAmount);
        }
        catch (Exception ex)
        {
            // If an error occurs, display the error message
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
        }

        // Call method to present account options after the currency conversion
        await AccountOptions(account);
    }

    // Method to show an animation during the currency exchange process
    private static async Task ExchangeAnimation(Account account, string? fromCurrency, string? toCurrency,
        decimal amount)
    {
        // Define custom style for progress bar text
        var customStyle = new Style(new Color(225, 69, 0));

        // Clear the console and display the logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Display user details on the screen
        UserDetails();

        // Display progress bar with multiple tasks (e.g., processing exchange request and updating account balance)
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
                // Define tasks for processing and updating account balances
                var task1 = ctx.AddTask("[rgb(190,40,0)]Processing exchange request[/]");
                var task2 = ctx.AddTask("[rgb(190,40,0)]Updating account balances[/]");

                // Run task1 (processing the exchange request)
                await RunTaskAsync(task1, 10, "Processing exchange request");

                // Once task1 is done, run task2 (updating account balances)
                await RunTaskAsync(task2, 5, "Updating account balances");
            }).GetAwaiter().GetResult();

        // Clear the console again and display the logo
        Console.Clear();
        Logo.DisplayFullLogo();

        // Show updated account details after the exchange
        AccountDetails(account);

        // Display success message with color formatting
        var message1 = "\u001b[38;2;34;139;34mYour exchange has been successfully processed.\u001b[0m";
        Console.WriteLine($"{message1}");

        // Show the currency conversion details with color formatting
        var message2 =
            $"\u001b[38;2;225;255;0mYou have exchanged from\u001b[0m \u001b[38;2;255;69;0m{fromCurrency}\u001b[0m \u001b[38;2;225;255;0m to \u001b[0m \u001b[38;2;255;69;0m{toCurrency}\u001b[0m. ";
        Console.WriteLine($"{message2}");

        // Display the final amount after exchange in the new currency
        var message3 =
            $"\u001b[38;2;225;255;0mFinal Amount in {toCurrency}:\u001b[0m \u001b[38;2;255;69;0m{amount:f2}\u001b[0m";
        Console.WriteLine($"{message3}");

        // Prompt the user to press any key to continue
        Console.WriteLine("\nPress any key to continue");

        // Wait for user input before proceeding
        Console.ReadLine();

        // Return to the user signed-in screen
        await UserSignedIn();
    }


    // Method to simulate a task's progress with dynamic color coding for the description
    private static async Task RunTaskAsync(ProgressTask task, double incrementValue, string contextDescription)
    {
        while (!task.IsFinished) // Loop until the task is finished
        {
            task.Increment(incrementValue); // Increment task progress

            // Dynamically color-code the task description
            var color = task.Value < 30 ? "rgb(190,40,0)" : task.Value < 100 ? "yellow" : "green";
            task.Description = $"[bold {color}] {contextDescription} {task.Value:0}%[/]";
            await Task.Delay(250); // Simulate work 
        }
    }

    // Method for selecting an account to deposit a loan in
    private static async Task DepositLoanIn()
    {
        if (_user?.Accounts == null) return; // Ensure user has accounts

        // Create a dictionary of account names with indentation
        var indentedAccounts = _user.Accounts
            .ToDictionary(account => $"  {account.AccountName}", account => account);

        // Prompt user to choose an account for the loan deposit
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Chose an Account to deposit your Loan in[/]".PadLeft(5))
                .AddChoices(indentedAccounts.Keys)
                .AddChoiceGroup("", "[yellow]Main Menu[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        // Handle the user's choice
        switch (choice)
        {
            case "[yellow]Main Menu[/]":
                await UserSignedIn(); // Navigate to main menu
                break;

            default:
                await AmountToLoan(indentedAccounts[choice]); // Process loan for the selected account
                break;
        }
    }

    // Method for handling the loan amount input and processing
    private static async Task AmountToLoan(Account account)
    {
        while (true) // Keep asking for loan input until successful
        {
            Console.Clear();
            Logo.DisplayFullLogo(); // Display logo
            AccountDetails(account); // Show account details

            decimal amount;
            while (true)
            {
                Console.Write("Amount to Loan: ");
                if (decimal.TryParse(Console.ReadLine(), out amount)) break; // Ensure valid input
            }

            // Create and process the loan
            var loan = LoanManager.CreateLoan(_user, account, amount);

            if (loan != null) // Loan successfully created
            {
                Console.WriteLine("\u001b[38;2;34;139;34mYour Loan was successfully processed\u001b[0m");
                Console.WriteLine(loan);
                break; // Exit loop after successful loan
            }

            Console.WriteLine();
            Console.WriteLine(
                $"\u001b[38;2;255;69;0mYour only allowed to Loan {LoanManager.LoanAmountAllowed(_user, account)}  {account.CurrencyCode}\u001b[0m");
            Thread.Sleep(3000); // Pause before retrying
        }

        // Prompt to continue
        Console.WriteLine("\nPress any Key to Continue");
        Console.ReadLine();

        await UserSignedIn(); // Return to signed-in user menu
    }

    // Method for admin sign-in and presenting admin menu
    private static async Task AdminSignedIn()
    {
        Console.Clear();
        Logo.DisplayFullLogo(); // Display the logo

        // Prompt user with admin menu options
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(5)
                .HighlightStyle(new Style(Color.Black, Color.Yellow))
                .Title("[bold underline rgb(190,40,0)]    Admin Menu[/]")
                .AddChoices("  Create User Account")
                .AddChoiceGroup("", "[yellow]Sign Out[/]")
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

        // Handle admin menu selection
        switch (selection.Trim())
        {
            case "Create User Account":
                await AdminCreateNewUser(); // Create a new user
                break;

            case "[yellow]Sign Out[/]":
                await DisplayMainMenu(); // Sign out and return to the main menu
                break;
        }
    }

    // Method for selecting an account to view transaction history
    private static async Task TransactionHistory()
    {
        if (_user?.Accounts != null)
        {
            // Create a dictionary of account names with indentation
            var indentedAccounts = _user.Accounts
                .ToDictionary(account => $"  {account.AccountName}", account => account);

            // Prompt user to choose an account to view transaction history
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(5)
                    .HighlightStyle(new Style(Color.Black, Color.Yellow))
                    .Title("[bold underline rgb(190,40,0)]    Chose an Account to check your transactions[/]"
                        .PadLeft(5))
                    .AddChoices(indentedAccounts.Keys)
                    .AddChoiceGroup("", "[yellow]Main Menu[/]")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

            // Handle user's choice
            switch (choice)
            {
                case "[yellow]Main Menu[/]":
                    await UserSignedIn(); // Navigate to main menu
                    break;

                default:
                    await TransactionDisplay(indentedAccounts[choice]); // Show transactions for selected account
                    break;
            }
        }
    }

    // Method for displaying transaction details for the selected account
    private static async Task TransactionDisplay(Account account)
    {
        var headerRow = new Rule("[bold yellow]Transaction Details[/]").RuleStyle("khaki1").Centered();
        AnsiConsole.Write(headerRow);

        // Create and display transaction table
        var table = new Table()
            .Alignment(Justify.Center)
            .BorderColor(Color.Khaki1)
            .Border(TableBorder.Rounded)
            .ShowRowSeparators();

        table.AddColumn(Markup.Escape("Account Name"));
        table.AddColumn(Markup.Escape("Receiver Account"));
        table.AddColumn(Markup.Escape("Receiver Name"));
        table.AddColumn(Markup.Escape("Transaction Date"));
        table.AddColumn(Markup.Escape("Amount"));
        table.AddColumn(Markup.Escape("Currency"));

        // Add each transaction to the table
        foreach (var transaction in account.TransferList)
        {
            table.AddRow(
                Markup.Escape($"{account.AccountName}"),
                Markup.Escape($"{transaction.ReceiverAccount}"),
                Markup.Escape($"{account.User?.FirstName} {account.User?.LastName}"),
                Markup.Escape($"{transaction.TransferDate:yyyy-MM-dd HH:mm:ss}"),
                Markup.Escape($"{transaction.Amount}"),
                Markup.Escape($"{transaction.CurrencyCode}")
            );
        }

        AnsiConsole.Write(table);

        // Footer with all transaction information
        var footerRow = new Rule("[bold green] All Transactions for this account[/]").RuleStyle("khaki1").Centered();
        AnsiConsole.Write(footerRow);

        Console.WriteLine("\nPress any key to continue");
        Console.ReadLine();

        await UserSignedIn(); // Return to signed-in user menu
    }

    // Method to create a new user from admin menu
    private static async Task AdminCreateNewUser()
    {
        EraseFields(); // Clear any previous user data

        // Collect user details
        _registeredFirstName = GetFirstName();
        _registeredLastName = GetLastName();
        _registeredEmail = GetEmail();
        _registeredPassword = GetPassword();
        _registeredPhoneNumber = GetPhoneNumber();

        // Add new user to the system
        UserManager.AddUser(0, _registeredPassword, _registeredEmail, _registeredFirstName, _registeredLastName,
            _registeredPhoneNumber);

        var user = Auth.Login(_registeredEmail, _registeredPassword);
        CreateDefaultBankAccounts(user); // Create default accounts for new user

        ResetAdminFields(); // Reset admin fields

        // Confirm user creation
        AnsiConsole.WriteLine("\nUser was created successfully!");
        Thread.Sleep(2000);
        await AdminSignedIn(); // Return to admin menu
    }

    // Method to clear the fields used for registering new users
    private static void EraseFields()
    {
        _registeredFirstName = "";
        _registeredLastName = "";
        _registeredEmail = "";
        _registeredPassword = "";
        _registeredPhoneNumber = "";
    }

    // Method to reset the admin fields to default values
    private static void ResetAdminFields()
    {
        _registeredFirstName = "Salamander";
        _registeredLastName = "Bank";
        _registeredEmail = "salamanderbank@gmail.com";
    }

    // Method to play a sound from the specified file path
    private static void PlaySound(string filePath)
    {
        using SoundPlayer soundPlayer = new(filePath);
        soundPlayer.PlaySync(); // Play the sound synchronously
    }
}