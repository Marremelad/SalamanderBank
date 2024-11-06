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
        public static Queue<Transfer> TransferQueue = new();
        public static void ProcessQueue()
        {
            while (true)
            {
                int timeToSleep = 900000; // If the queue is empty the thread will sleep for 15 minutes.

                if (TransferQueue.TryDequeue(out Transfer transfer))
                {
                    // Changes timeToSleep so the thread wakes up in time for the next transfer.
                    timeToSleep = (int)(DateTime.Now.AddMilliseconds(timeToSleep) - transfer.TransferDate).TotalMilliseconds;
                }
                // In case the program imports old transfers the timeToSleep can become negative, in which case it doesn't sleep.
                if (timeToSleep > 0) { Thread.Sleep(timeToSleep); }
                // If there was a transfer it's sent for processing.
                if (transfer != null)
                {
                    ProcessTransfer(transfer);
                }
            }
        }

        public static void ProcessTransfer(Transfer transfer)
        {
            // Makes sure the receiver and the receiving account exists.
            if(transfer.ReceiverUser == null || transfer.ReceiverAccount == null)
            {
                // If something goes wrong we return the money to the sender.
                transfer.SenderAccount.Balance += transfer.Amount;
                AccountManager.UpdateAccountBalance(transfer.SenderAccount);
                return;
            }
            
            // Converts the amount to the receiving accounts currency.
            transfer.Amount = CurrencyManager.ConvertCurrency(transfer.Amount, transfer.SenderAccount.Currency_code, transfer.ReceiverAccount.Currency_code);
            transfer.ReceiverAccount.Balance += transfer.Amount;
            AccountManager.UpdateAccountBalance(transfer.ReceiverAccount);
            
            // Marks the transfer as processed in the database.
            using (var connection = new SQLiteConnection(Database._connectionString))
            {
                connection.Open();
                string query = "UPDATE Transfers SET processed = @processed WHERE ID = @ID";
                var affectedRows = connection.Execute(query, new { processed = 1,transfer.ID });
                transfer.Processed = 1;
            }
        }
    
        // Adds a tranfer to the database.
        public static bool AddTransfer(Transfer transfer)
        {
            if (transfer.SenderAccount.Balance < transfer.Amount)
            {
                return false;
            }
            // Deducts the money from the senders account and savess the transfer in the database.
            // This ensures that the transfer continues to exist even if the application is closed.
            transfer.SenderAccount.Balance -= transfer.Amount;
            AccountManager.UpdateAccountBalance(transfer.SenderAccount);
            TransferQueue.Enqueue(transfer);
            using (var connection = new SQLiteConnection(Database._connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Transfers 
                 (sender_user_id, sender_account_id, receiver_user_id, receiver_account_id, transfer_date, amount, processed)
                 VALUES(@SenderUser, @SenderAccount, @ReceiverUser, @ReceiverAccount, @TransferDate, @Amount, @Processed)";

                var affectedRows = connection.Execute(query, new
                {
                    SenderUser = transfer.SenderUser.ID,
                    SenderAccount = transfer.SenderAccount.ID,
                    ReceiverUser = transfer.ReceiverUser.ID,
                    ReceiverAccount = transfer.ReceiverAccount.ID,
                    TransferDate = transfer.TransferDate,
                    Amount = transfer.Amount,
                    Processed = transfer.Processed
                });
                // Sets the ID of the transfer object.
                transfer.ID = connection.ExecuteScalar<int>("SELECT last_insert_rowid()");
            }
            return true;
        }
    
        // Creates a transfer object.
        public static Transfer CreateTransferObject(Account senderAccount, Account receiverAccount, int amount)
        {
            Transfer transfer = new()
            {
                SenderUser = senderAccount.User,
                SenderAccount = senderAccount,
                ReceiverUser = receiverAccount.User,
                ReceiverAccount = receiverAccount,
                Amount = amount,
                TransferDate = DateTime.Now,
                Processed = 0
            };
            return transfer;
        }

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
