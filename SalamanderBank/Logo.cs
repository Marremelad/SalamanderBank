using Pastel;

namespace SalamanderBank;

// Static class for displaying logos and branding for SalamanderBank.
public static class Logo
{
    // ASCII art representing the SalamanderBank logo.
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

    // ASCII art text logo for SalamanderBank.
    public const string TextLogo = @"
███████  █████  ██       █████  ███    ███  █████  ███    ██ ██████  ███████ ██████  
██      ██   ██ ██      ██   ██ ████  ████ ██   ██ ████   ██ ██   ██ ██      ██   ██ 
███████ ███████ ██      ███████ ██ ████ ██ ███████ ██ ██  ██ ██   ██ █████   ██████  
     ██ ██   ██ ██      ██   ██ ██  ██  ██ ██   ██ ██  ██ ██ ██   ██ ██      ██   ██ 
███████ ██   ██ ███████ ██   ██ ██      ██ ██   ██ ██   ████ ██████  ███████ ██   ██ 
--Ignite Your Wealth--" + "\n";

    // Padding for aligning logos on the screen.
    private const int LeftPadding = 90;

    // Displays the full logo, with fire and text, centered on the console.
    public static void DisplayFullLogo()
    {
        var fireLines = FireLogo.Split(Environment.NewLine);
        foreach (var line in fireLines)
        {
            // Calculate padding for fire logo based on console width.
            double padding = (Console.WindowWidth - line.Length) / 1.35;
            Console.WriteLine($"{new string(' ', (int)padding)}{line.Pastel(System.Drawing.Color.FromArgb(255, 69, 0))}");
        }

        var textLines = TextLogo.Split(Environment.NewLine);
        foreach (var line in textLines)
        {
            // Calculate padding for text logo to center it.
            int padding = (Console.WindowWidth - line.Length) / 2;
            Console.WriteLine(line.PadLeft(line.Length + padding));
        }
    }

    // Displays only the fire logo with predefined left padding.
    public static void DisplayFireLogo()
    {
        int leftPadding = LeftPadding;

        var fireLines = FireLogo.Split(Environment.NewLine);
        foreach (var line in fireLines)
        {
            Console.WriteLine(line.PadLeft(line.Length + leftPadding));
        }
    }

    // Displays only the text logo, centered on the console.
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
