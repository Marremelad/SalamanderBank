namespace SalamanderBank
{
    public class Transfer
    {
        // Unique identifier for the transfer
        public int Id;

        // Sender user details
        public User? SenderUser;

        // Sender account details
        public Account? SenderAccount;

        // Receiver user details
        public User? ReceiverUser;

        // Receiver account details
        public Account? ReceiverAccount;

        // Currency code for the transfer (e.g., SEK, USD)
        public string? CurrencyCode;

        // Amount being transferred
        public decimal Amount;

        // Date and time when the transfer was made
        public DateTime TransferDate;

        // Status of the transfer (processed or not)
        public int Processed;
    }
}