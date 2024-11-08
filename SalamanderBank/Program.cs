using DotNetEnv;

namespace SalamanderBank;

class Program
{
    public static Task Main(string[] args)
    {
        // Database.InitializeDatabase();
        // await Database.UpdateCurrenciesAsync();
        // Ui.DisplayMainMenu();
        Ui.LiveAccount2();
        return Task.CompletedTask;
    }
}
