using System.Globalization;
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

    public static void TitleScreen()
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

    public static void  LiveAccount()
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
                .HighlightStyle(new Style(new Color(225, 69, 0)))
                .AddChoices("Check Balance", "Transfer Funds", "View Transactions", "Exit")
        );

        // Perform actions based on the selection
        AnsiConsole.MarkupLine($"[bold green]You selected:[/] {selection}");

        // static void Currency()
        {
            // var customStyle = new Style(new Color(225, 69, 0));
            // var table = new Table()
            //     .Title("Exchange Rates")
            //     .BorderColor(Color.Khaki1)
            //     .Border(TableBorder.Rounded)
            //     .BorderStyle(customStyle)
            //     .Alignment(Justify.Center);
            //
            // //table.AddColumn(" ");//flag
            // table.AddColumn(" "); //Country name
            // table.AddColumn(" "); //Currency
            // table.AddColumn("Acronym");
            // table.AddColumn("[white] We Buy [/] ");
            // table.AddColumn("[white] We sell [/] ");
            //
            // table.AddRow("US", "Dollar", "USD", "1234", "546");
            // table.AddRow("EURO", "", "EUR", "1234", "546");
            // table.AddRow("Danish", "Krone", "NOK", "1234", "546");
            // table.AddRow("Japanese", "Yen", "JPY", "1234", "546");
            // table.AddRow("Norwegian", "Krone", "NOK", "1234", "546");
            // table.AddRow("Polish", "Zloty", "PLN", "1234", "546");
            // table.AddRow("South African", "Rand", "ZAR", "1234", "546");
            // table.AddRow("Swedish", "Krona", "SEK", "1234", "546");
            // table.AddRow("Swiss", "Franc", "CHF", "1234", "546");
            // table.AddRow("Brazilian", "Real", "BRL", "1234", "546");
            // table.AddRow("Canadian", "Dollar", "CAD", "1234", "546");
            //
            // table.Caption("This is the available [green]currencies[/] to choose from");
            //
            // AnsiConsole.Write(table);
            //
            // var prompt = new SelectionPrompt<string>()
            //     .Title("[bold underline rgb(190,40,0)]Select an Exchange Rate[/]")
            //     .PageSize(10)
            //     .HighlightStyle(customStyle);
            //     
            // var exchangeRates = new[]
            // {
            //     "US             | Dollar  | USD | 1234 | 546",
            //     "EURO           |         | EUR | 1234 | 546",
            //     "Danish         | Krone   | DKK | 1234 | 546",
            //     "Japanese       | Yen     | JPY | 1234 | 546",
            //     "Norwegian      | Krone   | NOK | 1234 | 546",
            //     "Polish         | Zloty   | PLN | 1234 | 546",
            //     "South African  | Rand    | ZAR | 1234 | 546",
            //     "Swedish        | Krona   | SEK | 1234 | 546",
            //     "Swiss          | Franc   | CHF | 1234 | 546",
            //     "Brazilian      | Real    | BRL | 1234 | 546",
            //     "Canadian       | Dollar  | CAD | 1234 | 546"
            // };
            // foreach (var rate in exchangeRates)
            // {
            //     prompt.AddChoice($"{rate.Value.Country} | {rate.Value.CurrencyName} | {rate.Value.Acronym} | {rate.Value.BuyRate} | {rate.Value.SellRate}");
            // }
            // var selectedRate = AnsiConsole.Prompt(prompt);
            // AnsiConsole.MarkupLine($"[bold yellow]You selected:[/] {selectedRate}. " +
            //                        $"\nWe will now begin the process of exchanging your money.");
            // ExchangingMoney();
            //     
            //Console.WriteLine("[green] Witch Currency do you want to exchange to?");


            //Show currencies available to chose from
            //let user choose 
        }

        // static void MoneyExchange()
        {
            // var exchangeRates = new Dictionary<string, (string Currency, string Acronym, double BuyRate, double SellRate)>
            //        {
            //            { "USD", ("Dollar", "USD", 1234, 546) },
            //            { "EUR", ("Euro", "EUR", 1234, 546) },
            //            { "NOK", ("Krone", "NOK", 1234, 546) },
            //            { "JPY", ("Yen", "JPY", 1234, 546) },
            //            { "PLN", ("Zloty", "PLN", 1234, 546) },
            //            { "ZAR", ("Rand", "ZAR", 1234, 546) },
            //            { "SEK", ("Krona", "SEK", 1234, 546) },
            //            { "CHF", ("Franc", "CHF", 1234, 546) },
            //            { "BRL", ("Real", "BRL", 1234, 546) },
            //            { "CAD", ("Dollar", "CAD", 1234, 546) },
            //        };
            //        var prompt = new SelectionPrompt<string>()
            //            .Title("[bold yellow]Select the currency to exchange[/]")
            //            .PageSize(10)
            //            .MoreChoicesText("[grey](Move up and down to see more options)[/]");
            //        foreach (var rate in exchangeRates)
            //        {
            //            prompt.AddChoice($"{rate.Value.Currency} ({rate.Value.Acronym}) - Buy: {rate.Value.BuyRate}, Sell: {rate.Value.SellRate}");
            //        }
            //        var selectedRate = AnsiConsole.Prompt(prompt);
            //        var selectedCurrency = selectedRate.Split('(')[1].Split(')')[0].Trim();
            //        var (currencyName, acronym, buyRate, sellRate) = exchangeRates[selectedCurrency];
            //        // Ask the user if they want to buy or sell
            //        var transactionType = AnsiConsole.Prompt(
            //            new SelectionPrompt<string>()
            //                .Title("[bold yellow]Would you like to buy or sell?[/]")
            //                .AddChoices("Buy", "Sell"));
            //        var amount = AnsiConsole.Ask<decimal>($"[yellow]Enter the amount to {transactionType.ToLower()} in {currencyName} ({acronym}):[/]");
            //        var option = transactionType == "Buy" ? buyRate : sellRate;
            //        var exchangedAmount = amount * (decimal)option;
            //        AnsiConsole.MarkupLine($"[green]Transaction completed![/] Exchanged [yellow]{amount}[/] {acronym} at a rate of [yellow]{option}[/].");
            //        AnsiConsole.MarkupLine($"[bold green]Total: {exchangedAmount}[/]");
            //        Console.ReadLine();
            //        SignedIn();
        }
    }

    public static void LiveAccount2()
    {
        TitleScreen();

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
                    .HighlightStyle(new Style(new Color(225, 69, 0)))
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

        //Second Menu after Signing in
        static void SignedIn()
        {
            
            Console.Clear();
            Logo.DisplayFullLogo();
            AccountDetails();
            while (true) // Second Menu loop
            {
                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .PageSize(3)
                        .HighlightStyle(new Style(new Color(225, 69, 0)))
                        .AddChoices("Check Balance", "Transfer Funds", "Money Exchange", "Take Loan",
                            "View Transactions", "Exit")
                        .MoreChoicesText("[grey](Move up and down to reveal more options)[/]"));

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

            //Needs account information
            //Needs to show the transaction been made between accounts
            
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
                using var player = new SoundPlayer(filePath);
                player.Load();
                player.Play();
            }

            static void MoneyExchange()
            {
                Console.Clear();
                Logo.DisplayFullLogo();
                var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Choose Account to use for exchange")
                    .HighlightStyle(new Style(new Color(225, 69, 0)))
                    .AddChoices("Account 1", "Account 2", "Account 3", "Return to Main Menu"));

                switch (selection)
                {
                    case "Account 1": // needs to display money on account
                        ExchangeMenu();
                        break;
                    case "Account 2":// needs to display money on account
                        ExchangeMenu();
                        break;
                    case "Account 3":// needs to display money on account
                        ExchangeMenu();
                        break;
                    case "Return to Main Menu":

                        break;
                }


                static void ExchangeMenu()
                {
                    //Show account to change from
                    //Ask for currency to change to
                    //Show exchange
                    //Show the new currency in new account
                    while (true) // Keep Exchange Menu open until exit
                    {
                        var exchange = AnsiConsole.Prompt(new SelectionPrompt<string>()
                            .Title("Please choose an option:")
                            .HighlightStyle(new Style(new Color(225, 69, 0)))
                            .AddChoices("Search for currency", "Display currency", "Return to Previous Menu"));

                        switch (exchange)
                        {
                            case "Search for currency":
                                Search();
                                break;
                            case "Display currency":
                                Currencies();
                                break;
                            case "Return to Previous Menu":
                                return;
                        }
                    }
                }

                static void Search()
                {
                    while (true)
                    {
                            string currentInput= "";
                            while (true)
                            {
                                
                                AnsiConsole.MarkupLine("Press the first letter of the acronym you are looking for:");
                                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // Read a key press
                                char pressedKey = char.ToUpper(keyInfo.KeyChar); // Convert to uppercase for consistency

                                // Filter the acronyms based on the first letter of the key
                                var filteredAcronyms = ExchangeRates
                                    .Where(a => a.Key.StartsWith(pressedKey.ToString(),
                                        StringComparison.OrdinalIgnoreCase))
                                    .ToDictionary(a => a.Key, a => a.Value);

                                // Check if there are matching acronyms
                                if (!filteredAcronyms.Any())
                                {
                                    AnsiConsole.MarkupLine($"[red]No acronyms found starting with '{pressedKey}'[/]");
                                    return;
                                }

                                if (keyInfo.Key == ConsoleKey.Enter)
                                {
                                    ExchangingMoney();
                                }

                                if (keyInfo.Key == ConsoleKey.Backspace && currentInput.Length > 0)
                                {
                                    currentInput = currentInput.Substring(currentInput.Length - 1);
                                }
                                else
                                {
                                    currentInput += keyInfo.KeyChar;
                                }
                                
                                var acronymKeys = filteredAcronyms.Keys.ToList();
                                
                                var selection = new SelectionPrompt<string>()
                                    .Title($"Currencies found: '{pressedKey}':")
                                    .PageSize(10)
                                    .AddChoices(acronymKeys); 

                                var chosenAcronym = AnsiConsole.Prompt(selection);
                                ExchangingMoney();
                                break;

                               
                                var selectedAcronym = filteredAcronyms[chosenAcronym];
                                AnsiConsole.MarkupLine(
                                    $"[green]You selected:[/] {chosenAcronym} - {selectedAcronym.CurrencyName} ({selectedAcronym.Country})");
                                AnsiConsole.MarkupLine(
                                    $"Buy Rate: {selectedAcronym.BuyRate}, Sell Rate: {selectedAcronym.SellRate}");
                            }
                            //Bellow is original code, DON'T TOUCH ITS WORKING
                        // Console.Clear();
                        //                         Logo.DisplayFullLogo();
                        //                         
                        //                         Console.Write("Enter a search term (Country,Currency or Acronym(SEK)): ");
                        //                         var searchTerm = Console.ReadLine()?.ToLower();
                        // var filteredResults = ExchangeRates.Where(c => string.Equals(c.Value.Country, searchTerm, StringComparison.OrdinalIgnoreCase) || 
                        //                                                string.Equals(c.Value.CurrencyName, searchTerm, StringComparison.OrdinalIgnoreCase) || string.Equals(c.Value.Acronym, searchTerm, StringComparison.OrdinalIgnoreCase))
                        //     .ToList();
                        //
                        // if (filteredResults.Count == 0)
                        // {
                        //     Console.Clear();
                        //     Logo.DisplayFullLogo();
                        //     var retryPrompt = new SelectionPrompt<string>().HighlightStyle(new Style(new Color(225, 69, 0)))
                        //         .Title("No matches found. Do you want to try again or return to the previous menu?")
                        //         .AddChoices("Retry", "Return");
                        //
                        //     var userChoice = AnsiConsole.Prompt(retryPrompt);
                        //     if (userChoice == "Retry")
                        //         continue;
                        //     ExchangeMenu();
                        //     return;
                        // }
                        //
                        // // Present the filtered results as options for selection using a SelectionPrompt
                        // var prompt = new SelectionPrompt<string>().Title("Select a currency:")
                        //     .HighlightStyle(new Style(new Color(225, 69, 0)))
                        //     .AddChoices(filteredResults.Select(c => $"{c.Value.Country} - {c.Value.CurrencyName}"));
                        //
                        // // Get the selected option
                        // var selectedOption = AnsiConsole.Prompt(prompt);
                        // var selectedCurrency = filteredResults.FirstOrDefault(c => $"{c.Value.Country} - {c.Value.CurrencyName}" == selectedOption).Value;
                        //
                        // if (selectedCurrency == default) return;
                        // // Display the details of the selected currency in a table
                        // var table = new Table();
                        // table.AddColumn("Country");
                        // table.AddColumn("Currency Name");
                        // table.AddColumn("Acronym");
                        // table.AddColumn("We Sell");
                        // table.AddColumn("We Buy");
                        //
                        // table.AddRow(selectedCurrency.Country, selectedCurrency.CurrencyName, selectedCurrency.Acronym, selectedCurrency.SellRate.ToString(CultureInfo.InvariantCulture), selectedCurrency.BuyRate.ToString(CultureInfo.InvariantCulture));
                        // Console.Clear();
                        // Logo.DisplayFullLogo();
                        // AnsiConsole.Write(table);
                        //
                        // var selection = new SelectionPrompt<string>().Title("Do you want to exchange to this Currency?: y/n")
                        //     .HighlightStyle(new Style(new Color(225, 69, 0)))
                        //     .AddChoices("Yes", "No");
                        // var userDecision = AnsiConsole.Prompt(selection);
                        //
                        // if (userDecision == "Yes")
                        // {
                        //     ExchangingMoney();
                        // }
                        //
                        // Console.WriteLine("Exchange Canceled. Returning to Previous Menu.");
                        // Thread.Sleep(1000);
                        // ExchangeMenu();

                    }
                }

                static void Currencies()
                {
                    //Writes out currencies that are available for exchange
                    var customStyle = new Style(new Color(225, 69, 0));

                    var prompt = new SelectionPrompt<string>()
                        .Title("[bold underline rgb(190,40,0)]Select an Exchange Rate[/]")
                        .PageSize(3)
                        .HighlightStyle(customStyle);
                    //Writes out every currency available in dictionary, needs to connect with GetExchangeRates
                    foreach (var rate in ExchangeRates)
                        prompt.AddChoice(
                            $"[bold white]{rate.Key,-5}[/] | {rate.Value.Country,-25} | {rate.Value.CurrencyName,-10} | {rate.Value.Acronym,-5} | {rate.Value.BuyRate,10:F2} | {rate.Value.SellRate,10:F2}");
                    var selectedRate = AnsiConsole.Prompt(prompt);
                    AnsiConsole.MarkupLine($"[bold yellow]You selected:[/] {selectedRate}. " +
                                           $"\nWe will now begin the process of exchanging your money.");
                    Thread.Sleep(1000);
                    ExchangingMoney();
                }

                static void ExchangingMoney()//Maybe add the currency choosen
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
                    AnsiConsole.MarkupLine("[bold green]Your exchange has been successfully processed.[/]");//Needs to be centered
                   //Maybe add the exchange details, currency, acronym, value etc. 
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
            }


            static void TakeLoan()
            {
                Console.Clear();
                Logo.DisplayFullLogo();
                {
                    var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title("Choose Account to use for taking a loan")
                        .HighlightStyle(new Style(new Color(225, 69, 0)))
                        .AddChoices("Account 1", "Account 2", "Account 3", "Return to Main Menu"));

                    switch (selection)
                    {
                        case "Account 1": // needs to display money on account

                            break;
                        case "Account 2":

                            break;
                        case "Account 3":

                            break;
                        case "Return to Main Menu":

                            break;
                    }


                    //Ask for account to use
                    //Show maximum amount of money to Loan
                    //Loan transaction
                    //Return to Main Menu
                }
            }

            static void ViewTransaction()
            {
                Console.Clear();
                Logo.DisplayFullLogo();
                //show accounts where transactions has been made
            }
        }
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
    // Static dictionary
    public static readonly
        Dictionary<string, (string Country, string CurrencyName, string Acronym, double BuyRate, double SellRate)>
        ExchangeRates =
            new()
            {
                { "USD", ("United States of America", "Dollar", "USD", 1234, 546) },
                { "EUR", ("EURO", "", "EUR", 1234, 546) },
                { "DKK", ("Denmark", "Krone", "DKK", 1234, 546) },
                { "JPY", ("Japan", "Yen", "JPY", 1234, 546) },
                { "NOK", ("Norway", "Krone", "NOK", 1234, 546) },
                { "PLN", ("Poland", "Zloty", "PLN", 1234, 546) },
                { "ZAR", ("South Africa", "Rand", "ZAR", 1234, 546) },
                { "SEK", ("Sweden", "Krona", "SEK", 1234, 546) },
                { "CHF", ("Switzerland", "Franc", "CHF", 1234, 546) },
                { "BRL", ("Brazil", "Real", "BRL", 1234, 546) },
                { "CAD", ("Canada", "Dollar", "CAD", 1234, 546) }
            };
}