## Salamander Bank

<img src="https://github.com/Marremelad/AssetsAndImages/raw/main/SalamanderBannerText.png" height="200" width="900" alt="Salamander Banner">

### Contributors
* [Mauricio Corte](https://github.com/Marremelad)
* [Anton Dahlström](https://github.com/Anton-Dahlstrom)
* [Onni Bucht](https://github.com/onni82)
* [Rasmus Wenngren](https://github.com/RasmusWenngren92)
* [Matheus Torico](https://github.com/ikariLain)

### Features

#### As user
* Create Account.
* Receive verification email.
* Sign in with existing account.
* View existing bank accounts.
* Create new bank accounts.
* Transfer funds to different accounts.
* Receive SMS message after funds have been received.
* Exchange currency.
* View transaction history.

#### As admin 
* Create new users.

## Application Demo
<img src="https://github.com/Marremelad/AssetsAndImages/raw/main/SalamanderBankDemoGif.gif" height="500" width="900" alt="Salamander Demo Gif">

<table>
  <tr>
    <td align="center" valign="top">
      <p>Verification Email</p>
      <img src="https://github.com/Marremelad/AssetsAndImages/raw/main/SalamanderBankEmailScreenShot.png" height="300" width="500" alt="Screenshot Of Verification Email">
    </td>
    <td align="center" valign="top">
      <p>Transfer SMS</p>
      <img src="https://github.com/Marremelad/AssetsAndImages/raw/main/SalamanderBankSms.jpg" height="500" width="300" alt="Screenshot Of Transfer SMS">
    </td>
  </tr>
</table>

## Structure
In this application we have chosen to structure the code in the following way.
* Program.cs is the entry point of the program and is only used to start the application.
* The Ui class displays user options and is largely responsible for message output and formatting.
* The object classes: User, Account, Transfer, Loan and Currency, stores information that can be processed by the manager classes.
* The Manager classes: UserManager, AccountManager, TransferManager, LoanManager, CurrencyManager and Auth, processes the information of objects and sends the data to the Ui class and also handles database querying.
* The DB class initializes the database and creates the database tables.
* The classes, EmailService and SmsService Handles the formatting and sending of email and SMS messages respectively.

#### Download UML diagram
<img src="https://github.com/Marremelad/AssetsAndImages/raw/main/SalamanderBankUML.pdf" height="300" width="500" alt="UML Diagram">

## Requirements
To run the app successfully make sure that you are using the following.

Runtime:
* [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

Packages:
* [MimeKit](https://github.com/jstedfast/MimeKit)
* [MailKit](https://github.com/jstedfast/MailKit)
* [Pastel](https://www.nuget.org/packages/Pastel)
* [DotNetEnv](https://www.nuget.org/packages/DotNetEnv/)
* [SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
* [Dapper](https://www.learndapper.com/)

Files:
* In your entry point directory, create a file named Credentials.env and add the environmental variables EMAIL and EMAIL_PASSWORD for sending emails, SMS_API_USERNAME and SMS_API_PASSWORD for sending SMS messages and CURRENCY_API_KEY for fetching currency data.
  <img src="https://github.com/Marremelad/AssetsAndImages/raw/main/ENVScreenShot.png" height="200" width="300" alt="UML Diagram">

### Notes
* Keep in mind that some email providers require SMTP authentication for third party applications. This means that your regular password might not work when trying to send emails through this app. To fix this, access your email account and generate a third party app password and set it as the value for the EMAIL_PASSWORD variable.
  Alternatively contact one of the contributors to use Team Salamanders SMTP credentials.

* This application uses the [46elks](https://46elks.se/) API for sending SMS messages and [currencyapi.com](https://currencyapi.com/) for fetching currency data. To access these API's you will have to get a subscription or ask Team Salamander for the API keys and passwords.

### Get started
If you are using Visual Studio or any other C# IDE with an integrated repo-cloning function use it with this URL - https://github.com/Marremelad/SalamanderBank.git
then open the solution and run the program.

Else, do the following.
* Open the terminal on your computer.
* Navigate to the directory where you keep your repositories.
* Run the following command
```console
git clone https://github.com/Marremelad/SalamanderBank.git      
```
* Navigate into the directory that holds the projects entry point and run the following command.
```console
dotnet run
```
