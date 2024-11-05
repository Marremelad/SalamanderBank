using DotNetEnv;

namespace SalamanderBank;

class Program
{
    public static async Task Main(string[] args)
    {
        Database.InitializeDatabase();
        await CurrencyManager.UpdateCurrenciesAsync();
        Ui.DisplayMainMenu();
    }
}