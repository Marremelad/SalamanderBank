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
                var sql = "UPDATE Accounts SET balance = @balance WHERE ID = @ID";
                var affectedRows = connection.Execute(sql, new { balance = account.Balance, account.ID });
            }
        }
        public static Account? GetAccount(int id)
        {
            using (var connection = new SQLiteConnection(Database._connectionString))
            {
                connection.Open();
                var sql = @"SELECT a.*, u.* 
                        FROM Accounts a 
                        INNER JOIN Users u on u.id = a.user_id
                        WHERE a.ID = @ID";
                var account = connection.Query<Account, User, Account>(
                    sql,
                    (acc, user) =>
                    {
                        acc.User = user;  // Assuming Account has a property User to hold User info
                        return acc;
                    },
                    new { ID = id },
                    splitOn: "id"  // Use the `id` column to indicate where the User object starts
                    ).FirstOrDefault();

                return account;
            }

        }
    }
}
