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
        public static void PopulateQueueFromDB()
        {
            // Selects a transfer based on Processed status and orders the transfers
            // by TransferDate so the oldest transfer is processed first in the Queue.
            string queryUnprocessedTransfers = @"
                SELECT t.*, 
                       su.*, sa.*, 
                       ru.*, ra.*
                FROM Transfers t
                INNER JOIN Users su ON su.ID = t.SenderUserID
                INNER JOIN Accounts sa ON sa.ID = t.SenderAccountID
                INNER JOIN Users ru ON ru.ID = t.ReceiverUserID
                INNER JOIN Accounts ra ON ra.ID = t.ReceiverAccountID
                WHERE t.Processed = @Processed
                ORDER BY t.TransferDate DESC";

            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                // Type hints informs Dapper which classes to use when mapping the information
                // it gets back from the SQL query.
                var transferList = connection.Query<Transfer, User, Account, User, Account, Transfer>(
                    queryUnprocessedTransfers,
                    (transfer, senderUser, senderAccount, receiverUser, receiverAccount) =>
                    {
                        // Sets the attributes of the Transfer object to the objects made from the joins.
                        transfer.SenderUser = senderUser;
                        transfer.SenderAccount = senderAccount;
                        transfer.ReceiverUser = receiverUser;
                        transfer.ReceiverAccount = receiverAccount;
                        return transfer;
                    },
                    new { Processed = 0 },
                    splitOn: "id"
                ).ToList();
                TransferQueue = new Queue<Transfer>(transferList);
            } 
        }
        public static void ProcessQueue()
        {
            while (true)
            {
                int timeToSleep = 5000; // If the queue is empty the thread will sleep for 15 minutes.

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

        private static async Task ProcessTransfer(Transfer transfer)
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
            transfer.Amount = CurrencyManager.ConvertCurrency(transfer.Amount, transfer.SenderAccount.CurrencyCode, transfer.ReceiverAccount.CurrencyCode);
            transfer.ReceiverAccount.Balance += transfer.Amount;
            AccountManager.UpdateAccountBalance(transfer.ReceiverAccount);
            
            // Marks the transfer as processed in the database.
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                string query = "UPDATE Transfers SET Processed = @processed WHERE ID = @ID";
                var affectedRows = connection.Execute(query, new { processed = 1,transfer.ID });
                transfer.Processed = 1;

                await SmsService.SendSms(transfer.ReceiverUser.PhoneNumber, $"Hello {transfer.ReceiverUser.FirstName}! {transfer.Amount:f2} {transfer.ReceiverAccount.CurrencyCode} was sent to your '{transfer.ReceiverAccount.AccountName}' account.");
            }
        }
    
        // Adds a tranfer to the database.
        private static bool AddTransferToDB(Transfer transfer)
        {
            if (transfer.SenderAccount.Balance < transfer.Amount)
            {
                return false;
            }
            // Deducts the money from the senders account and savess the transfer in the database.
            // This ensures THAT The transfer continues to exist even if the application is closed.
            transfer.SenderAccount.Balance -= transfer.Amount;
            AccountManager.UpdateAccountBalance(transfer.SenderAccount);
            TransferQueue.Enqueue(transfer);
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Transfers 
                 (SenderUserID, SenderAccountID, ReceiverUserID, ReceiverAccountID, CurrencyCode, TransferDate, Amount, Processed)
                 VALUES(@SenderUser, @SenderAccount, @ReceiverUser, @ReceiverAccount, @CurrencyCode, @TransferDate, @Amount, @Processed)";

                var affectedRows = connection.Execute(query, new
                {
                    SenderUser = transfer.SenderUser.ID,
                    SenderAccount = transfer.SenderAccount.ID,
                    ReceiverUser = transfer.ReceiverUser.ID,
                    ReceiverAccount = transfer.ReceiverAccount.ID,
                    CurrencyCode = transfer.CurrencyCode,
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
        public static Transfer? CreateTransfer(Account senderAccount, Account receiverAccount, decimal amount)
        {
            Transfer transfer = new()
            {
                SenderUser = senderAccount.User,
                SenderAccount = senderAccount,
                ReceiverUser = receiverAccount.User,
                ReceiverAccount = receiverAccount,
                CurrencyCode = senderAccount.CurrencyCode, 
                Amount = amount,
                TransferDate = DateTime.Now,
                Processed = 0
            };
            if (AddTransferToDB(transfer))
            {
                return transfer;
            };
            return null;
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
                INNER JOIN Users su ON su.ID = t.SenderUserID
                INNER JOIN Accounts sa ON sa.ID = t.SenderAccountID
                INNER JOIN Users ru ON ru.ID = t.ReceiverUserID
                INNER JOIN Accounts ra ON ra.ID = t.ReceiverAccountID
                WHERE t.id = @ID";

            using (var connection = new SQLiteConnection(DB._connectionString))
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
