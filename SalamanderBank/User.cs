
namespace SalamanderBank
{
    public class User
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Verified { get; set; }
		public int Locked { get; set; }
		public List<Account>? Accounts = new List<Account>();
		public List<Loan>? Loans = new List<Loan>();
    }
}
