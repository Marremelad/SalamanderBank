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
        public static void UpdateAccountBalance(Account account)
        {
            using (var connection = new SQLiteConnection(Database._connectionString))
            {
                connection.Open();
                var sql = "UPDATE Accounts SET Balance = @balance WHERE ID = @ID";
                var affectedRows = connection.Execute(sql, new { balance = account.Balance, account.ID });
            }
        }

		public static void UpdateAccountCurrency(Account account)
		{
			using (var connection = new SQLiteConnection(Database._connectionString))
			{
				connection.Open();
				var sql = "UPDATE Accounts SET CurrencyCode = @currencyCode WHERE ID = @ID";
				var affectedRows = connection.Execute(sql, new { currencyCode = account.CurrencyCode, account.ID });
			}
		}

		public static Account? GetAccount(int id)
        {
            using (var connection = new SQLiteConnection(Database._connectionString))
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
        public static List<Account> GetAccountsFromUser(User user)
        {
            using (var connection = new SQLiteConnection(Database._connectionString))
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

                return accounts;
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
    }
}
