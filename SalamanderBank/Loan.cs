
namespace SalamanderBank
{

    public class Loan
    {
        public int Id;
        public User? User;
        public decimal Amount;
        public string? CurrencyCode;
        public decimal InterestRate;
        public int Status;

        public DateTime LoanDate = DateTime.Now;

        public override string ToString()
        {
            return $"Loan Amount: {Amount} {CurrencyCode} | Interest Rate: {InterestRate}%";
        }
    }
}