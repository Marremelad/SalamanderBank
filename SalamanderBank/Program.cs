using DotNetEnv;

namespace SalamanderBank;

class Program
{
    public static async Task Main(string[] args)
    {
        Database.InitializeDatabase();
        await Database.UpdateCurrenciesAsync();
        Ui.DisplayMainMenu();
    }
}