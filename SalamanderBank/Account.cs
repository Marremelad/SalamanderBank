using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    public class Account
    {
        public int ID;
        public User User;
        public string CurrencyCode;
        public string AccountName;
        public decimal Balance;
        public int Status;
        public int Type;
        public float Interest;
        public List<Transfer> TransferList = [];

        // Creates a dictionary that stores an account type and an interest rate
        public static Dictionary<int, float> AccountTypes = new Dictionary<int, float>
        {
            { 0, 0.01f },
            { 1, 30.00f },
            { 2, 20.0f }
        };
    }
}
