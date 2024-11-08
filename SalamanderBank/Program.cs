namespace SalamanderBank;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("initializing db");
        Database.InitializeDatabase();

        Console.WriteLine("creating user");
        Database.AddUser(0, "password", "john.doe@example.com", "John", "Doe", "112");

        Console.WriteLine("logging in");
        User user = Database.Login("john.doe@example.com", "password");
        if (user != null)
        {
            Console.WriteLine($"Logged in as {user.FirstName} {user.LastName}");
        }

        Console.WriteLine("adding account");
        AccountManager.CreateAccount(user, "USD", "Dollar account", 0, 0);

		Console.WriteLine("adding the same account again");
		AccountManager.CreateAccount(user, "USD", "account 2", 0, 0);
	}
}