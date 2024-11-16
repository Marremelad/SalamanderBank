
namespace SalamanderBank
{
    public class Account
    {
        public int Id;
        public User? User;
        public string? CurrencyCode;
        public string? AccountName;
        public decimal Balance;
        public int Status;
        public int Type;
        public float Interest;
        public List<Transfer> TransferList = [];
        public static Dictionary<int, float> AccountTypes = new Dictionary<int, float>
        {
            { 0, 0.00f },
            { 1, 30.00f },
            { 2, 20.0f }
        };

        public override string? ToString()
        {
            return AccountName;
        }
    }
}
