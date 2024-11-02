using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    internal class User(string password, string email, string first_name, string last_name)
    {
        private int? _id;
        public int? ID
        {
            get => _id;
            set
            {
                Console.WriteLine("test");
                if (_id != null)
                {
                    throw new InvalidOperationException("MyAttribute can only be set once.");
                }
                _id = value;
            }
        }
        public string Password = password;
        public string Email = email;
        public string First_name = first_name;
        public string Last_name = last_name;
    }
}
