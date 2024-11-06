using Pastel;

namespace SalamanderBank;

public static class Logo
{
    public const string FireLogo = @"⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⢀⠄⠀⠀⠀⠀⠀⠙⠛⢿⣷⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⣿⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⣿⣆⠀⠀⠀⠀⠀⠀⠀⣼⣿⣿⡿⠁⠀⠀⠀⠀⠀⢤⣀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠙⣿⣿⣶⣤⣤⣤⣴⣾⣿⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⢻⣷⣄⠀⠀⠀
⠀⠀⠀⠀⠀⠈⢿⣿⣿⠟⠋⢉⣉⣉⣙⣿⣷⣤⣾⡀⠀⠀⠀⠀⣸⣿⣿⣦⠀⠀
⠀⠀⢠⡄⠀⠀⣸⡿⠁⢀⣴⣿⣿⡿⢿⠿⣿⣿⣿⣷⣀⣀⣀⣴⣿⣿⣿⣿⡆⠀
⠀⠀⢸⣿⣷⣾⣿⡇⠀⠸⣿⣿⣿⡇⣀⠐⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⠀
⠀⠀⢸⣿⣿⣿⣿⣿⡀⠀⠙⠿⣿⣿⣿⠀⢸⣿⣿⣿⣿⡉⠛⢻⣿⣿⣿⣿⣿⠀
⠀⠀⠈⣿⣿⣿⣿⣿⣷⣄⠀⠀⠈⠉⠁⠀⠘⠛⠿⠿⢉⣴⣤⣿⣿⣿⣿⣿⡿⠀
⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⣷⣦⡀⠀⠀⠀⠀⠀⠀⠀⠘⠿⠁⠈⠙⠛⢿⣿⠃⠀
⠀⠀⠀⠀⠹⣿⣿⣿⣿⡟⢻⠟⠁⣠⣦⣤⡀⠀⣀⣀⣀⡀⠀⠀⠀⢀⣾⠏⠀⠀
⠀⠀⠀⠀⠀⠈⠻⣿⣧⡄⢠⣴⣾⣿⣿⣿⡄⠺⢿⣿⣿⣿⣶⣄⣴⡿⠃⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⠿⣿⣿⣿⣿⣯⣤⣀⣿⣿⣿⣿⡿⠟⠉⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠉⠛⠛⠛⠛⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀";

    public const string TextLogo = @"
███████  █████  ██       █████  ███    ███  █████  ███    ██ ██████  ███████ ██████  
██      ██   ██ ██      ██   ██ ████  ████ ██   ██ ████   ██ ██   ██ ██      ██   ██ 
███████ ███████ ██      ███████ ██ ████ ██ ███████ ██ ██  ██ ██   ██ █████   ██████  
     ██ ██   ██ ██      ██   ██ ██  ██  ██ ██   ██ ██  ██ ██ ██   ██ ██      ██   ██ 
███████ ██   ██ ███████ ██   ██ ██      ██ ██   ██ ██   ████ ██████  ███████ ██   ██ 
--Ignite Your Wealth--" + "\n";

    private const int LeftPadding = 90;

    public static void DisplayFullLogo()
    {
        var fireLines = FireLogo.Split(Environment.NewLine);
        foreach (var line in fireLines)
        {
            double padding = (Console.WindowWidth - line.Length) / 1.35;
            Console.WriteLine($"{new string(' ', (int)padding)}{line.Pastel(System.Drawing.Color.FromArgb(255, 69, 0))}");
        }
        
        var textLines = TextLogo.Split(Environment.NewLine);
        foreach (var line in textLines)
        {
            int padding = (Console.WindowWidth - line.Length) / 2;
            Console.WriteLine(line.PadLeft(line.Length + padding));
        }
        
    }

    public static void DisplayFireLogo()
    {
        int leftPadding = LeftPadding;
        
        var fireLines = FireLogo.Split(Environment.NewLine);
        foreach (var line in fireLines)
        {
            Console.WriteLine(line.PadLeft(line.Length + leftPadding));
        }
    }

    public static void DisplayTextLogo()
    {
        var textLines = TextLogo.Split(Environment.NewLine);
        foreach (var line in textLines)
        {
            int padding = (Console.WindowWidth - line.Length) / 2;
            Console.WriteLine(line.PadLeft(line.Length + padding));
        }
    }
}