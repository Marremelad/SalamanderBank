using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class AccountManager
    {
        // Retrieves the transfer history for the specified account
        public static void GetAccountTransferHistory(Account account)
        {
            string transferQuery = @"
                SELECT t.*, 
                       su.*, sa.*, 
                       ru.*, ra.*
                FROM Transfers t
                INNER JOIN Users su ON su.ID = t.SenderUserID
                INNER JOIN Accounts sa ON sa.ID = t.SenderAccountID
                INNER JOIN Users ru ON ru.ID = t.ReceiverUserID
                INNER JOIN Accounts ra ON ra.ID = t.ReceiverAccountID
                WHERE sa.id = @ID OR ra.id = @ID
                ORDER BY t.TransferDate DESC";

            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                // Type hints inform Dapper which classes to use when mapping the SQL query result.
                var transferList = connection.Query<Transfer, User, Account, User, Account, Transfer>(
                    transferQuery,
                    (transfer, senderUser, senderAccount, receiverUser, receiverAccount) =>
                    {
                        // Sets the attributes of the Transfer object to the objects made from the joins.
                        senderAccount.User = senderUser;
                        receiverAccount.User = receiverUser;
                        transfer.SenderUser = senderUser;
                        transfer.SenderAccount = senderAccount;
                        transfer.ReceiverUser = receiverUser;
                        transfer.ReceiverAccount = receiverAccount;
                        return transfer;
                    },
                    // Passes the account's ID as a parameter to the query.
                    new { ID = account.Id },
                    splitOn: "id"    // Specifies the columns Dapper uses to split the result into separate objects.
                ).ToList();
                account.TransferList = transferList; // Assigns the list of transfers to the account
            }
        }

        // Updates the account's balance in the database
        public static void UpdateAccountBalance(Account account)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET Balance = @balance WHERE ID = @ID";
                connection.Execute(sql, new { balance = account.Balance, ID = account.Id });
            }
        }

        // Updates the account's currency code in the database
        public static void UpdateAccountCurrency(Account account)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET CurrencyCode = @currencyCode WHERE ID = @ID";
                connection.Execute(sql, new { currencyCode = account.CurrencyCode, ID = account.Id });
            }
        }

        // Updates the account's name in the database
        public static void UpdateAccountName(Account account)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET AccountName = @name WHERE ID = @ID";
                connection.Execute(sql, new { name = account.AccountName, ID = account.Id });
            }
        }

        // Retrieves an account by its ID from the database
        public static Account? GetAccount(int id)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                var sql = @"SELECT a.*, u.* 
                        FROM Accounts a 
                        INNER JOIN Users u on u.ID = a.UserID
                        WHERE a.ID = @ID";
                var account = connection.Query<Account, User, Account>(
                    sql,
                    (acc, user) =>
                    {
                        acc.User = user;  // Sets the User property of the Account object
                        return acc;
                    },
                    new { ID = id },
                    splitOn: "ID"  // Uses the `ID` column to indicate where the User object starts
                    ).FirstOrDefault();

                return account;
            }
        }

        // Retrieves all accounts associated with a specific user
        public static void GetAccountsFromUser(User? user)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                var sql = @"SELECT a.*, u.*
                        FROM Accounts a
                        INNER JOIN Users u on u.ID = a.UserID
                        WHERE a.UserID = @UserID";
                if (user != null)
                {
                    var accounts = connection.Query<Account, User, Account>(
                        sql,
                        (acc, accUser) =>
                        {
                            acc.User = accUser;  // Sets the User property of the Account object
                            return acc;
                        },
                        new { UserID = user.Id },
                        splitOn: "ID"  // Uses the `ID` column to indicate where the User object starts
                    ).ToList();
                    user.Accounts = accounts; // Assigns the retrieved accounts to the user's Accounts property
                }

                if (user?.Accounts != null)
                    foreach (Account acc in user.Accounts)
                    {
                        GetAccountTransferHistory(acc); // Retrieves the transfer history for each account
                    }
            }
        }

        // Converts the currency of an account and updates the balance and currency
        public static Account ConvertAccountCurrency(Account account, string? newCurrencyCode)
        {
            // Checks if the account is trying to convert to a different currency
            if (account.CurrencyCode != newCurrencyCode)
            {
                // Converts the balance to the new currency using CurrencyManager
                decimal newBalance = CurrencyManager.ConvertCurrency(account.Balance, account.CurrencyCode, newCurrencyCode);

                if (newBalance > 0)
                {
                    account.Balance = newBalance; // Updates the account balance
                    account.CurrencyCode = newCurrencyCode; // Updates the account currency
                    UpdateAccountBalance(account); // Updates the account balance in the database
                    UpdateAccountCurrency(account); // Updates the account currency in the database
                }
            }

            // Returns the updated or unchanged account
            return account;
        }

        // Creates a new account for a user
        public static bool CreateAccount(User? user, string currencyCode, string accountName, int type, decimal balance = 0)
        {
            // Checks if the account name is already in use
            if (user != null && user.Accounts!.Any(acc => acc.AccountName == accountName))
            {
                return false; // Returns false if the account name is already in use
            }

            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();

                // Gets the interest rate based on the account type from the Account.AccountTypes dictionary
                float interest = Account.AccountTypes[type];

                // Inserts the new account into the database
                var sql = "INSERT INTO Accounts (UserID, CurrencyCode, AccountName, Balance, Status, Type, Interest) VALUES (@UserID, @CurrencyCode, @AccountName, @Balance, @Status, @Type, @Interest)";
                if (user != null)
                {
                    var affectedRows = connection.Execute(sql, new { UserID = user.Id, CurrencyCode = currencyCode, AccountName = accountName, Balance = balance, Status = 1, Type = type, Interest = interest });
                    Console.WriteLine($"{affectedRows} rows inserted into Accounts.");
                }

                GetAccountsFromUser(user); // Retrieves and assigns the new account to the user
            }

            return true; // Returns true if the account was successfully created
        }
    }
}
