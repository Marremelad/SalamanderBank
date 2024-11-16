using System.Data.SQLite;
using Dapper;

namespace SalamanderBank
{
    internal class TransferManager
    {
        // Queue to hold transfers that need to be processed
        public static Queue<Transfer?> TransferQueue = new();

        // Populates the queue from the database by selecting unprocessed transfers
        public static void PopulateQueueFromDb()
        {
            // SQL query to select unprocessed transfers and order by TransferDate
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

            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                // Fetch transfers from the database
                var transferList = connection.Query<Transfer, User, Account, User, Account, Transfer>(
                    queryUnprocessedTransfers,
                    (transfer, senderUser, senderAccount, receiverUser, receiverAccount) =>
                    {
                        // Set up Transfer object with related User and Account data
                        transfer.SenderUser = senderUser;
                        transfer.SenderAccount = senderAccount;
                        transfer.ReceiverUser = receiverUser;
                        transfer.ReceiverAccount = receiverAccount;
                        return transfer;
                    },
                    new { Processed = 0 },
                    splitOn: "id"
                ).ToList();
                // Populate the queue with the fetched transfers
                TransferQueue = new Queue<Transfer?>(transferList);
            } 
        }

        // Processes transfers in the queue
        public static async void ProcessQueue()
        {
            while (true)
            {
                int timeToSleep = 5000; // Default sleep time (5 seconds)

                if (TransferQueue.TryDequeue(out Transfer? transfer))
                {
                    // Adjust sleep time if there is a transfer to process
                    if (transfer != null)
                        timeToSleep = (int)(DateTime.Now.AddMilliseconds(timeToSleep) - transfer.TransferDate)
                            .TotalMilliseconds;
                }

                // If the time to sleep is positive, sleep the thread
                if (timeToSleep > 0) { Thread.Sleep(timeToSleep); }

                // Process the transfer if available
                if (transfer != null)
                {
                    await ProcessTransfer(transfer);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        // Processes a single transfer
        private static async Task ProcessTransfer(Transfer? transfer)
        {
            // Check if receiver exists and return money to sender if not
            if(transfer?.ReceiverUser == null || transfer.ReceiverAccount == null)
            {
                // Refund the sender if something goes wrong
                if (transfer?.SenderAccount != null)
                {
                    transfer.SenderAccount.Balance += transfer.Amount;
                    AccountManager.UpdateAccountBalance(transfer.SenderAccount);
                }

                return;
            }
            
            // Convert the transfer amount to the receiver's account currency
            transfer.Amount = CurrencyManager.ConvertCurrency(transfer.Amount, transfer.SenderAccount?.CurrencyCode, transfer.ReceiverAccount.CurrencyCode);
            transfer.ReceiverAccount.Balance += transfer.Amount;
            AccountManager.UpdateAccountBalance(transfer.ReceiverAccount);
            
            // Mark the transfer as processed in the database
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                string query = "UPDATE Transfers SET Processed = @processed WHERE ID = @ID";
                await connection.ExecuteAsync(query, new { processed = 1, ID = transfer.Id });
                transfer.Processed = 1;

                // Notify the receiver via SMS
                await SmsService.SendSms(transfer.ReceiverUser.PhoneNumber, $"Hello {transfer.ReceiverUser.FirstName}! {transfer.Amount:f2} {transfer.ReceiverAccount.CurrencyCode} was sent to your '{transfer.ReceiverAccount.AccountName}' account.");
            }
        }
    
        // Adds a transfer to the database
        private static bool AddTransferToDb(Transfer? transfer)
        {
            // Check if sender has enough balance
            if (transfer?.SenderAccount != null && transfer.SenderAccount.Balance < transfer.Amount)
            {
                return false;
            }

            // Deducts the transfer amount from sender's account and saves the transfer
            if (transfer?.SenderAccount != null)
            {
                transfer.SenderAccount.Balance -= transfer.Amount;
                AccountManager.UpdateAccountBalance(transfer.SenderAccount);
                TransferQueue.Enqueue(transfer);

                // Insert transfer details into the database
                using (var connection = new SQLiteConnection(Db.ConnectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Transfers 
                 (SenderUserID, SenderAccountID, ReceiverUserID, ReceiverAccountID, CurrencyCode, TransferDate, Amount, Processed)
                 VALUES(@SenderUser, @SenderAccount, @ReceiverUser, @ReceiverAccount, @CurrencyCode, @TransferDate, @Amount, @Processed)";

                    if (transfer is { SenderUser: not null, ReceiverUser: not null, ReceiverAccount: not null })
                    {
                        connection.Execute(query, new
                        {
                            SenderUser = transfer.SenderUser.Id,
                            SenderAccount = transfer.SenderAccount.Id,
                            ReceiverUser = transfer.ReceiverUser.Id,
                            ReceiverAccount = transfer.ReceiverAccount.Id,
                            transfer.CurrencyCode,
                            transfer.TransferDate,
                            transfer.Amount,
                            transfer.Processed
                        });
                    }

                    // Retrieve and set the ID of the newly inserted transfer
                    transfer.Id = connection.ExecuteScalar<int>("SELECT last_insert_rowid()");
                }
            }

            return true;
        }

        // Creates a new transfer object
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

            if (AddTransferToDb(transfer))
            {
                // Refresh the account transfer history
                AccountManager.GetAccountTransferHistory(senderAccount);
                AccountManager.GetAccountTransferHistory(receiverAccount);
                return transfer;
            }

            return null;
        }

        // Retrieves a transfer by its ID
        public static Transfer? GetTransfer(int transferId)
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
                WHERE t.id = @ID";

            using (var connection = new SQLiteConnection(Db.ConnectionString))
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
                    splitOn: "id"
                ).FirstOrDefault(); // Returns the first result or null
                return transfer;
            }
        }
    }
}
