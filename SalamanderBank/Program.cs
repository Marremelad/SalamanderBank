namespace SalamanderBank;

class Program
{
    // rasmuswenngren@hotmail.com
    // matheus-torrico@outlook.com
    // onni.bucht@me.com
    // anton.dahlstrom@hotmail.com
    public static void Main(string[] args)
    {
        // Database db = new Database("salamanderbank.db");
        // db.InitializeDatabase();
        // db.AddUser(007, 1, "123abc", "james@bond.com", "James", "Bond");
        
        EmailService.SendEmail("Mauricio", "mauricio.corte@chasacademy.se", EmailService.VerificationText("Mauricio"));
        
        // Logo.DisplayFullLogo();
    }
}