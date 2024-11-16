
namespace SalamanderBank
{
    public class Transfer
    {
        public int Id;
        public User? SenderUser;
        public Account? SenderAccount;
        public User? ReceiverUser;
        public Account? ReceiverAccount;
        public String? CurrencyCode;
        public decimal Amount;
        public DateTime TransferDate;
        public int Processed;
    }
}
