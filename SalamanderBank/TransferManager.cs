using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class TransferManager
    {
        public static Transfer GetTransfer(int transferId)
        {
            // Selects a transfer based on transferId and joins relevant information
            // from Users and Accounts tables.
            string transferQuery = @"
                SELECT t.*, 
                       su.*, sa.*, 
                       ru.*, ra.*
                FROM Transfers t
                INNER JOIN Users su ON su.id = t.sender_user_id
                INNER JOIN Accounts sa ON sa.id = t.sender_account_id
                INNER JOIN Users ru ON ru.id = t.receiver_user_id
                INNER JOIN Accounts ra ON ra.id = t.receiver_account_id
                WHERE t.id = @ID";

            using (var connection = new SQLiteConnection(Database._connectionString))
            {
                connection.Open();
                // Type hints informs Dapper which classes to use when mapping the information
                // it gets back from the SQL query.
                var transfer = connection.Query<Transfer, User, Account, User, Account, Transfer>(
                    transferQuery,
                    (transfer, senderUser, senderAccount, receiverUser, receiverAccount) =>
                    {
                        // Sets the attributes of the Transfer object to the objects made from the joins.
                        transfer.SenderUser = senderUser;
                        transfer.SenderAccount = senderAccount;
                        transfer.ReceiverUser = receiverUser;
                        transfer.ReceiverAccount = receiverAccount;
                        return transfer;
                    },
                    // Determines the value of the @ID parameter.
                    new { ID = transferId },
                    splitOn: "id"    // Creates a new group of columns whenever it encounters a column named "id".
                                     // This allows Dapper to sequentially map each group of columns to each Class.
                ).FirstOrDefault(); // Returns the first result found or returns null without throwing an exception.
                return transfer;
            }
        }
    }
}
