using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    internal class Transfer
    {
        public int ID;
        public User Sender_user_id;
        public Account Sender_account_id;
        public User Reciever_user_id;
        public Account Reciever_account_id;
        public decimal Amount;
        public int Processed;
    }
}
