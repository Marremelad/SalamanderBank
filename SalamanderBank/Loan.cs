using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
   
    public class Loan
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public string Status { get; set; }

        // Constructor for initializing Loan objects
        public Loan(int userId, decimal amount, decimal interestRate)
        {
            UserId = userId;
            Amount = amount;
            InterestRate = interestRate;
            Status = "Pending";  // Default status
        }
    }
}
