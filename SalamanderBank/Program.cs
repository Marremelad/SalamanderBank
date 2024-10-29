namespace SalamanderBank;

class Program
{
    public static void Main(string[] args)
    {
        Database db = new Database("salamanderbank.db");
        db.InitializeDatabase();
        db.AddUser(007, 1, "123abc", "james@bond.com", "James", "Bond");
    }
}