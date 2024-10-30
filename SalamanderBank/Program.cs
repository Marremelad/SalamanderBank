using DotNetEnv;

namespace SalamanderBank;

class Program
{
    // rasmuswenngren@hotmail.com
    // matheus-torrico@outlook.com
    // onni.bucht@me.com
    // anton.dahlstrom@hotmail.com
    public static void Main(string[] args)
    {
        for (int i = 0; i < 3; i++)
        {
            EmailService.SendEmail("Anton", "anton.dahlstrom@hotmail.com", "Verification",EmailService.VerificationText("Anton"));
            EmailService.SendEmail("Mauricio", "mauricio.corte@chasacademy.se", "Verification",EmailService.VerificationText("Mauricio"));
            Thread.Sleep(5000);
        }
        // Logo.DisplayFullLogo();
    }
}