namespace SalamanderBank
{
    public class Account
    {
        // Account properties
        public int Id; // Unique identifier for the account
        public User? User; // Reference to the User associated with this account
        public string? CurrencyCode; // Currency code used for this account (e.g., USD, EUR)
        public string? AccountName; // Name of the account (e.g., Checking, Savings)
        public decimal Balance; // Current balance in the account
        public int Status; // Status of the account (could represent active, inactive, etc.)
        public int Type; // Type of account (e.g., 0 for basic, 1 for premium)
        public float Interest; // Interest rate associated with the account
        public List<Transfer> TransferList = []; // List of transfers made from/to this account

        // Static dictionary to hold account types and their respective interest rates
        public static Dictionary<int, float> AccountTypes = new Dictionary<int, float>
        {
            { 0, 0.00f }, // Basic account type with no interest
            { 2, 20.0f } // Other account type with 20% interest
        };

        // Override ToString method to return the account name
        public override string? ToString()
        {
            return AccountName; // Return the name of the account
        }
    }
}