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
        public string Currency_code;
        public string Account_name;
        public decimal Balance;
        public int Status;
        public int Type;
        public float Interest;
    }
}
