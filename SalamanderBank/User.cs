namespace SalamanderBank
{
    public class User
    {
        // Unique identifier for the user
        public int Id { get; set; }

        // Type of user (e.g., admin, regular user)
        public int Type { get; set; }

        // First name of the user
        public string? FirstName { get; set; }

        // Last name of the user
        public string? LastName { get; set; }

        // Email address of the user
        public string? Email { get; set; }

        // Password for the user's account
        public string? Password { get; set; }

        // Phone number of the user
        public string? PhoneNumber { get; set; }

        // Verification status of the user's account
        public string? Verified { get; set; }

        // Account lock status (e.g., whether the account is locked)
        public int Locked { get; set; }

        // List of accounts associated with the user
        public List<Account>? Accounts = new List<Account>();

        // List of loans associated with the user
        public List<Loan>? Loans = new List<Loan>();
    }
}