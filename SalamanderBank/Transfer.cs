using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    public class Transfer
    {
        public int ID;
        public User SenderUser;
        public Account SenderAccount;
        public User ReceiverUser;
        public Account ReceiverAccount;
        public decimal Amount;
        public DateTime TransferDate;
        public int Processed;
    }
}
