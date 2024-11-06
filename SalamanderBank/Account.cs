using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    internal class Account
    {
        public int ID;
        public User User;
        public string CurrencyCode;
        public string AccountName;
        public decimal Balance;
        public int Status;
        public int Type;
        public float Interest;
    }
}
