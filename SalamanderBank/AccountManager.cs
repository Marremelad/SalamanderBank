using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class AccountManager
    {
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

            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                // Type hints informs Dapper which classes to use when mapping the information
                // it gets back from the SQL query.
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
                    // Determines the value of the @ID parameter.
                    new { ID = account.ID },
                    splitOn: "id"    // Creates a new group of columns whenever it encounters a column named "id".
                                     // This allows Dapper to sequentially map each group of columns to each Class.
                ).ToList();
                account.TransferList = transferList;
            }
        }

		// Updates the account's balance in the database
		public static void UpdateAccountBalance(Account account)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET Balance = @balance WHERE ID = @ID";
                var affectedRows = connection.Execute(sql, new { balance = account.Balance, account.ID });
            }
        }

		// Updates the account's currency in the database
		public static void UpdateAccountCurrency(Account account)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET CurrencyCode = @currencyCode WHERE ID = @ID";
                var affectedRows = connection.Execute(sql, new { currencyCode = account.CurrencyCode, account.ID });
            }
        }

		// Updates the account's name in the database
		public static void UpdateAccountName(Account account)
		{
			using (var connection = new SQLiteConnection(DB._connectionString))
			{
				connection.Open();
				var sql = "UPDATE Accounts SET Name = @name WHERE ID = @ID";
				var affectedRows = connection.Execute(sql, new { name = account.AccountName, account.ID });
			}
		}

		public static Account? GetAccount(int id)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
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
                        acc.User = user;  // Assuming Account has a property User to hold User info
                        return acc;
                    },
                    new { ID = id },
                    splitOn: "ID"  // Use the `ID` column to indicate where the User object starts
                    ).FirstOrDefault();

                return account;
            }
        }

        // A method that retreives all accounts where all any account's UserID column = user.ID
        public static void GetAccountsFromUser(User? user)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                var sql = @"SELECT a.*, u.*
                        FROM Accounts a
                        INNER JOIN Users u on u.ID = a.UserID
                        WHERE a.UserID = @UserID";
                var accounts = connection.Query<Account, User, Account>(
                    sql,
                    (acc, user) =>
                    {
                        acc.User = user;  // Assuming Account has a property User to hold User info
                        return acc;
                    },
                    new { UserID = user.ID },
                    splitOn: "ID"  // Use the `ID` column to indicate where the User object starts
                    ).ToList();
                user.Accounts = accounts;
                foreach(Account acc in user.Accounts) 
                {
                    GetAccountTransferHistory(acc);
                }
            }
        }

        // Method that converts the currency of an account
        public static Account ConvertAccountCurrency(Account account, string newCurrencyCode)
        {
            // Checks if it tries to convert to the same currency
            if (account.CurrencyCode != newCurrencyCode)
            {
                // The new balance will be calculated by CurrencyManager.ConvertCurrency
                decimal newBalance = CurrencyManager.ConvertCurrency(account.Balance, account.CurrencyCode, newCurrencyCode);

                if (newBalance > 0)
                {
                    account.Balance = newBalance;
                    account.CurrencyCode = newCurrencyCode;
                    UpdateAccountBalance(account);
                    UpdateAccountCurrency(account);
                }
            }

            // Either way this method will return the same account, updated or not
            return account;
        }

        // Creates an account for the user used as an argument
        public static void CreateAccount(User? user, string currencyCode, string accountName, int type, decimal balance = 0)
        {
            // Checks if the account name is already in use
            if (user.Accounts.Any(acc => acc.AccountName == accountName))
            {
                Console.WriteLine("Account name already in use.");
                return;
            }
            else
            {
                using (var connection = new SQLiteConnection(DB._connectionString))
                {
                    connection.Open();

                    // Gets the interest rate for account type based on dictionary in Account.AccountTypes
                    float interest = Account.AccountTypes[type];

                    // Inserts the account into SQL
                    var sql = "INSERT INTO Accounts (UserID, CurrencyCode, AccountName, Balance, Status, Type, Interest) VALUES (@UserID, @CurrencyCode, @AccountName, @Balance, @Status, @Type, @Interest)";
                    var affectedRows = connection.Execute(sql, new { UserID = user.ID, CurrencyCode = currencyCode, AccountName = accountName, Balance = balance, Status = 1, Type = type, Interest = interest });
                    Console.WriteLine($"{affectedRows} rows inserted into Accounts.");

                    GetAccountsFromUser(user);
                }
            }
        }
    }
}
