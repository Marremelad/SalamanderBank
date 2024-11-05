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

                var transfer = connection.Query<Transfer, User, Account, User, Account, Transfer>(
                    transferQuery,
                    (transfer, senderUser, senderAccount, receiverUser, receiverAccount) =>
                    {
                        transfer.SenderUser = senderUser;
                        transfer.SenderAccount = senderAccount;
                        transfer.ReceiverUser = receiverUser;
                        transfer.ReceiverAccount = receiverAccount;
                        return transfer;
                    },
                    new { ID = transferId },
                    splitOn: "id"    // "id" should mark each new object split point
                ).FirstOrDefault();
                return transfer;
            }
        }
    }
}
