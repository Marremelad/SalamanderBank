using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
   
    public class Loan
    {
        public int ID;
        public User User;
        public decimal Amount;
        public decimal InterestRate;
        public int Status;
        public string CurrencyCode;

        public DateTime LoanDate = DateTime.Now;
    }
}
