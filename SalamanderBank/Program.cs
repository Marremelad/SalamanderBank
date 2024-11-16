namespace SalamanderBank

{
    class Program
    {
        // Entry point for the application
        public static async Task Main(string[] args)
        {
            // Run the user interface for the program asynchronously
            await Ui.RunProgram();
        }
    }
}