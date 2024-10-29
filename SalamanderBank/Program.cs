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
        
        // EmailService.SendEmail($"Hello Mauricio and welcome to Salamander Bank! To get started, verify your account using the code below. \n{Guid.NewGuid()}");
        
        EmailService.SendEmail("Mauricio", "Corte.Mauricio98@gmail.com", EmailService.VerificationText("Mauricio"));
    }
}