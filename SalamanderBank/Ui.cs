using System.Media;
using System.Text.RegularExpressions;
using Spectre.Console;



namespace SalamanderBank;

public static class Ui
{
    private static string? _registeredFirstName;
    private static string? _registeredLastName;
    private static string? _registeredEmail;
    private static string? _registeredPassword;

    private static readonly double DisplayPadding = Console.WindowWidth / 2.25;
    private const double MenuPadding = 2.1;

    private static string FirstNameDisplay => $"First Name: {_registeredFirstName}".PadLeft(_registeredFirstName != null
        ? "First Name: ".Length + _registeredFirstName.Length + (int)DisplayPadding
        : "First Name: ".Length + (int)DisplayPadding);

    private static string LastNameDisplay => $"Last Name: {_registeredLastName}".PadLeft(_registeredLastName != null
        ? "Last Name: ".Length + _registeredLastName.Length + (int)DisplayPadding
        : "Last Name: ".Length + (int)DisplayPadding);

    private static string EmailDisplay => $"Email: {_registeredEmail}".PadLeft(_registeredEmail != null
        ? "Email: ".Length + _registeredEmail.Length + (int)DisplayPadding
        : "Email: ".Length + (int)DisplayPadding);

    private static string PasswordDisplay => $"Password: {_registeredPassword}".PadLeft(_registeredPassword != null
        ? "Password: ".Length + _registeredPassword.Length + (int)DisplayPadding
        : "Password: ".Length + (int)DisplayPadding);

    private const decimal AccountBalance = 1500.75m;

    public static void DisplayMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            var option1 = "Create Account".PadLeft("Create Account".Length +
                                                   (int)((Console.WindowWidth - "Create Account".Length) /
                                                         MenuPadding));
            var option2 =
                "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
            var option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1));

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
        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        while (true)
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            Console.Write($"{FirstNameDisplay}\n{LastNameDisplay}\n{EmailDisplay}");

            var password = Console.ReadLine();
            if (password != null && Regex.IsMatch(password, emailPattern)) return password;

            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mPlease enter a valid email\u001b[0m";
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

            var password = Console.ReadLine();
            if (!string.IsNullOrEmpty(password) && password.Length >= 8) return password;

            Console.WriteLine();
            var message = "\u001b[38;2;255;69;0mPassword has to be at least 8 characters long\u001b[0m";
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

            var message =
                $"A code has been sent to \u001b[38;2;34;139;34m{_registeredEmail}\u001b[0m, use it to verify your account.";
            var message2 = "Enter Code: ";

            Console.WriteLine();
            Console.Write($"{message}".PadLeft(message.Length + (Console.WindowWidth - message.Length) / 2));
            Console.WriteLine();
            Console.Write($"{message2}".PadLeft(message2.Length + (Console.WindowWidth - message2.Length) / 2));
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
        table.BorderStyle = new Style(ConsoleColor.DarkRed);
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
                new Layout("Left").Size(30), // Set the width of the Left column
                new Layout("Right").Size(70) // Set the width of the Right column
                    .SplitRows(
                        new Layout("Top").Size(20), // Set the height of the Top section
                        new Layout("Bottom").Size(20) // Set the height of the Bottom section
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
                .AddChoices("Check Balance", "Transfer Funds", "View Transactions", "Exit")
        );

        // Perform actions based on the selection
        AnsiConsole.MarkupLine($"[bold green]You selected:[/] {selection}");
    }

    public static async void LiveAccount2()
    {
        while (true) //First Menu 
        {
            Console.Clear();
            Logo.DisplayFullLogo();

            var option1 = "Create Account".PadLeft("Create Account".Length +
                                                   (int)((Console.WindowWidth - "Create Account".Length) /
                                                         MenuPadding));
            var option2 =
                "Sign In".PadLeft("Sign In".Length + (int)((Console.WindowWidth - "Sign In".Length) / MenuPadding));
            var option3 = "Exit".PadLeft("Exit".Length + (int)((Console.WindowWidth - "Exit".Length) / 2.1));

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
            var customStyle = new Style(new Color(225, 69, 0));
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.BorderStyle = customStyle;
            table.AddColumn("[bold yellow blink on rgb(190,40,0)] Welcome to SalamanderBank![/]").Centered();


            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            Console.ReadLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(3)
                    .AddChoices("Check Balance", "Transfer Funds", "Money Exchange", "Take Loan", "View Transactions",
                        "Exit")
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            );

            switch (selection.Trim())
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
                "\n[]Transaction Complete![/]\nYou will now be redirected to the main menu.");
            PlaySound(
                @"C:\Users\rasmu\source\repos\SalamanderBank\SalamanderBank\SoundPath\cashier-quotka-chingquot-sound-effect-129698.wav");
            Thread.Sleep(3000);
            SignedIn();
        }

        static void PlaySound(string filePath)
        {
            using (var player = new SoundPlayer(filePath))
            {
                player.Load();
                player.Play();
            }
        }

        static void MoneyExchange()
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Choose Account to use for exchange")
                .AddChoices("Account 1", "Account 2", "Account 3", "Return to Main Menu"));

            switch (selection)
            {
                case "Account 1":
                    ExchangeMenu();
                    break;
                case "Account 2":

                    break;
                case "Account 3":

                    break;
                case "Return to Main Menu":

                    break;
            }


            static void ExchangeMenu()
            {
                //Show account to change from
                //Ask for currency to change to
                //Ask for amount to change
                //Show exchange
                //Show the new currency in new account

                var exchange = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Please chose an option:")
                    .AddChoices("Search for currency", "Display currency", "Return to Previous Menu",
                        "Return to Main Menu"));
                switch (exchange)
                {
                    case "Search":
                        Search();
                        break;
                    case "Display currencies":
                        Currencies();
                        break;
                    case "Return to Previous Menu":
                        return;
                    case "Return to Main Menu":
                        break;
                }
            }

            static void Search()
            {
                var currencies = new List<Currency>
                {
                    new("US", "Dollar", "1234", "546", "USD"),
                    new("EURO", "", "1234", "546", "EUR"),
                    new("Danish", "Krone", "1234", "546", "NOK"),
                    new("Japanese", "Yen", "1234", "546", "JPY"),
                    new("Norwegian", "Krone", "1234", "546", "NOK"),
                    new("Polish", "Zloty", "1234", "546", "PLN"),
                    new("South African", "Rand", "1234", "546", "ZAR"),
                    new("Swedish", "Krona", "1234", "546", "SEK"),
                    new("Swiss", "Franc", "1234", "546", "CHF"),
                    new("Brazilian", "Real", "1234", "546", "BRL"),
                    new("Canadian", "Dollar", "1234", "546", "CAD")
                };


                Console.Write("Enter a search term (Country,Currency Name or Acronym): ");
                var searchTerm = Console.ReadLine();

                var filteredResults = currencies
                    .Where(c => searchTerm != null &&
                                (c.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                 || c.CurrencyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                 || c.Acronym.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                if (filteredResults.Count == 0)
                {
                    var retryPrompt = new SelectionPrompt<string>()
                        .Title(
                            "No matches found. Do you want to try again or return to precious menu?")
                        .AddChoices("Retry", "Return");
                    var userChoice = AnsiConsole.Prompt(retryPrompt);
                    switch (userChoice)
                    {
                        case "Retry":
                            break;
                        case "Return":
                            MoneyExchange();
                            break;
                    }
                }

                // Present the filtered results as options for selection using a SelectionPrompt
                var prompt = new SelectionPrompt<string>()
                    .Title("Select a currency:")
                    .AddChoices(filteredResults.Select(c => $"{c.Country} - {c.CurrencyName}"));

                // Get the selected option
                var selectedOption = AnsiConsole.Prompt(prompt);

                // Find the selected Currency object based on the selection
                var selectedCurrency = filteredResults
                    .FirstOrDefault(c => $"{c.Country} - {c.CurrencyName}" == selectedOption);

                if (selectedCurrency != null)
                {
                    // Display the details of the selected currency in a table
                    var table = new Table();
                    table.AddColumn("Country");
                    table.AddColumn("Currency Name");
                    table.AddColumn("Acronym");
                    table.AddColumn("We Sell");
                    table.AddColumn("We Buy");

                    table.AddRow(selectedCurrency.Country, selectedCurrency.CurrencyName,
                        selectedCurrency.Acronym, selectedCurrency.Value1, selectedCurrency.Value2);

                    // Render the table
                    AnsiConsole.Write(table);

                    var selection = new SelectionPrompt<string>()
                        .Title("Do you want to exchange to this Currency?: y/n")
                        .AddChoices("Yes", "No");
                    var selectedOption2 = AnsiConsole.Prompt(selection);

                    switch (selectedOption2)
                    {
                        case "Yes":
                            ExchangingMoney();
                            break;
                        case "No":
                            Console.WriteLine("Exchange Canceled." +
                                              "\n Returning to Previous Menu.");
                            MoneyExchange();
                            Console.Clear();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("No currency or country found");
                }
                //Let user search for currencies
                //Let user choose currency
            }
        }

        static void Currencies()
        {
            var customStyle = new Style(new Color(225, 69, 0));
            var table = new Table()
                .Title("Exchange Rates")
                .BorderColor(Color.Khaki1)
                .Border(TableBorder.Rounded)
                .BorderStyle(customStyle)
                .Alignment(Justify.Center);

            //table.AddColumn(" ");//flag
            table.AddColumn(" "); //Country name
            table.AddColumn(" "); //Currency
            table.AddColumn("Acronym");
            table.AddColumn("[white] We Buy [/] ");
            table.AddColumn("[white] We sell [/] ");

            table.AddRow("US", "Dollar", "USD", "1234", "546");
            table.AddRow("EURO", "", "EUR", "1234", "546");
            table.AddRow("Danish", "Krone", "NOK", "1234", "546");
            table.AddRow("Japanese", "Yen", "JPY", "1234", "546");
            table.AddRow("Norwegian", "Krone", "NOK", "1234", "546");
            table.AddRow("Polish", "Zloty", "PLN", "1234", "546");
            table.AddRow("South African", "Rand", "ZAR", "1234", "546");
            table.AddRow("Swedish", "Krona", "SEK", "1234", "546");
            table.AddRow("Swiss", "Franc", "CHF", "1234", "546");
            table.AddRow("Brazilian", "Real", "BRL", "1234", "546");
            table.AddRow("Canadian", "Dollar", "CAD", "1234", "546");

            table.Caption("This is the available [green]currencies[/] to choose from");

            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine("[green] Witch Currency do you want to exchange to?");


            //Show currencies available to chose from
            //let user choose 
            ExchangingMoney();
        }

        static void ExchangingMoney()
        {
            //Display Exchange
            //Update accounts
            //Return to main menu
        }



    }

    public static void TakeLoan()
    {
        {
            //Ask for account to use
            //Show maximum amount of money to Loan
            //Loan transaction
            //Return to Main Menu
        }

        
    }
    public static void ViewTransaction()
        {
            //show accounts where transactions has been made
        }
   

    public class Currency
    {
        public string Country { get; set; }
        public string CurrencyName { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Acronym { get; set; }

        public Currency(string country, string currencyName, string value1, string value2, string acronym)
        {
            Country = country;
            CurrencyName = currencyName;
            Value1 = value1;
            Value2 = value2;
            Acronym = acronym;
        }
    }

   

   
}