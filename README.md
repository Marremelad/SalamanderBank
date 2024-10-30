## Salamander Bank 🦎

### Roles
* Mauricio Corte (CEO) --> Connection Services
* Anton Dahlström (CTO) (V-CEO) --> Architecture
* Onni Bucht (V-CTO) --> Database
* Rasmus Wenngren --> UI Design and implementation
* Matheus Torico --> UML Design

### Requirements
To run the app successfully make sure that you are using the following.

Runtime:
* .NET 8

Packages:
* MimeKit
* MailKit
* DotNetEnv
* SQLite


Files:
* Create a file named Credentials.env in your entry point directory and add the environmental variables EMAIL and APP_PASSWORD.
* Alternatively contact @Marremelad to use Team Salamanders smpt credentials

Keep in mind that some email providers require smpt authentication for third party applications. This means that your regular password might not work when trying to send emails through this app. To fix this, access your email account and generate a third party app password and set it as the value for the APP_PASSWORD variable.