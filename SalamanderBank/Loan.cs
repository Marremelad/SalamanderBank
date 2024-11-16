namespace SalamanderBank
{
    public class Loan
    {
        // Represents the loan's unique identifier
        public int Id;

        // Represents the user who took out the loan
        public User? User;

        // Represents the loan amount
        public decimal Amount;

        // Represents the currency code for the loan (e.g., SEK, USD)
        public string? CurrencyCode;

        // Represents the interest rate for the loan
        public decimal InterestRate;

        // Represents the loan's status (e.g., active, closed)
        public int Status;

        // Represents the date the loan was taken out, defaulted to the current date and time
        public DateTime LoanDate = DateTime.Now;

        // Returns a string representation of the loan, including amount, currency, and interest rate
        public override string ToString()
        {
            return $"Loan Amount: {Amount} {CurrencyCode} | Interest Rate: {InterestRate}%";
        }
    }
}