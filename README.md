## Salamander Bank 🦎

### Roles
* [Mauricio Corte](https://github.com/Marremelad) (CEO) --> Connection Services
* [Anton Dahlström](https://github.com/Anton-Dahlstrom) (CTO) (V-CEO) --> Architecture
* [Onni Bucht](https://github.com/onni82) (V-CTO) --> Database
* [Rasmus Wenngren](https://github.com/RasmusWenngren92) --> UI Design and implementation
* [Matheus Torico](https://github.com/ikariLain) --> UML Design

### Requirements
To run the app successfully make sure that you are using the following.

Runtime:
* [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

Packages:
* [MimeKit](https://github.com/jstedfast/MimeKit)
* [MailKit](https://github.com/jstedfast/MailKit)
* [Pastel](https://www.nuget.org/packages/Pastel)
* [DotNetEnv](https://www.nuget.org/packages/DotNetEnv/)
* [SQLite](https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
  
### Optional
This application uses the [46eLks](https://46elks.se/) API for sending SMS messages. To access the API you will have to get a subscription or ask for Team Salamanders API username and key.

Files:
* In your entry point directory, create a file named Credentials.env and add the environmental variables EMAIL and EMAIL_PASSWORD for sending emails and the environmental variables SMS_API_USERNAME and SMS_API_PASSWORD for sending SMS messages.
* Alternatively contact [Mauricio Corte](https://github.com/Marremelad) to use Team Salamanders SMTP credentials.

Keep in mind that some email providers require SMTP authentication for third party applications. This means that your regular password might not work when trying to send emails through this app. To fix this, access your email account and generate a third party app password and set it as the value for the EMAIL_PASSWORD variable.