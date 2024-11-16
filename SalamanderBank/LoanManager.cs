using Dapper;
using System.Data.SQLite;

namespace SalamanderBank
{
    public class LoanManager
    {
        // Method to check if a loan is allowed based on the current balance and outstanding loans
        public static decimal LoanAmountAllowed(User? user, Account account)
        {
            // Calculate the total balance of all the accounts for the user
            decimal totalBalance = GetTotalBalance(user, account.CurrencyCode);

            // Retrieve the total sum of all outstanding loans for the user
            decimal totalLoans = GetTotalLoans(user, account.CurrencyCode);

            // Calculate the available amount for new loans based on the total balance
            decimal availableToLoan = ((totalBalance - totalLoans) * 5) - totalLoans;

            // Return the maximum available loan amount (ensuring it's not negative)
            return Math.Round(Math.Max(availableToLoan, 0), 4);
        }

        // Method to fetch the total sum of loans for the given user
        public static decimal GetTotalLoans(User? user, string? currencyCode)
        {
            decimal totalLoans = 0;

            // Loop through the user's loans and calculate the total outstanding amount
            if (user?.Loans != null)
                foreach (Loan loan in user.Loans)
                {
                    totalLoans += CurrencyManager.ConvertCurrency(loan.Amount, loan.CurrencyCode, currencyCode);
                }

            // Return the total loans rounded to two decimal places
            return Math.Round(totalLoans, 2);
        }

        // Method to get the total balance across all accounts for the user
        public static decimal GetTotalBalance(User? user, string? currencyCode)
        {
            decimal totalBalance = 0;

            // Loop through the user's accounts and calculate the total balance
            if (user?.Accounts != null)
                foreach (var account in user.Accounts)
                {
                    totalBalance += CurrencyManager.ConvertCurrency(account.Balance, account.CurrencyCode, currencyCode);
                }

            // Return the total balance rounded to two decimal places
            return Math.Round(totalBalance, 2);
        }

        // Method to add a loan to the database for the given user
        public static Loan? CreateLoan(User? user, Account account, decimal loanAmount, decimal interestRate = 3, int status = 0)
        {
            // Check if the loan is allowed based on the available balance and outstanding loans
            decimal amountAllowedToLoan = LoanAmountAllowed(user, account);
            if (amountAllowedToLoan <= 0 || loanAmount > amountAllowedToLoan)
            {
                // Return null if the loan amount exceeds the allowed limit
                return null;
            }

            // SQL query to insert the loan into the Loans table
            string insertLoanQuery = @"
                INSERT INTO Loans (UserID, Amount, CurrencyCode, InterestRate, Status, LoanDate)
                VALUES (@UserID, @Amount, @CurrencyCode, @InterestRate, @Status, @LoanDate );
                SELECT * FROM Loans WHERE ID = last_insert_rowid();";

            using (SQLiteConnection connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                // Parameters for the SQL query
                var parameters = new
                {
                    UserID = user!.Id,
                    Amount = loanAmount,
                    account.CurrencyCode,
                    InterestRate = interestRate,
                    Status = 0,
                    LoanDate = DateTime.Now
                };
                // Insert the loan into the database and retrieve the loan details
                Loan loan = connection.QuerySingle<Loan>(insertLoanQuery, parameters);
                
                // Reload the user's loans from the database
                GetLoansFromUser(user);
                // Update the account balance after loan disbursement
                account.Balance += loanAmount;
                
                // Update the account balance in the database
                AccountManager.UpdateAccountBalance(account);
                return loan;
            }
        }

        // Method to retrieve all loans for a user from the database
        public static void GetLoansFromUser(User? user)
        {
            using (var connection = new SQLiteConnection(Db.ConnectionString))
            {
                connection.Open();
                // SQL query to retrieve loans for the user
                var sql = @"SELECT l.*, u.*
                        FROM Loans l
                        INNER JOIN Users u on u.ID = l.UserID
                        WHERE l.UserID = @UserID";
                
                if (user != null)
                {
                    // Execute the query and map the result to the Loan and User objects
                    var loans = connection.Query<Loan, User, Loan>(
                        sql,
                        (loan, loanUser) =>
                        {
                            loan.User = loanUser;  // Assign the User object to the Loan
                            return loan;
                        },
                        new { UserID = user.Id },
                        splitOn: "ID"  // Split the result based on the 'ID' column
                    ).ToList();
                    // Assign the list of loans to the user's Loans property
                    user.Loans = loans;
                }
            }
        }
    }
}
