using Dapper;
using Microsoft.AspNetCore.Identity;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalamanderBank
{
    public class LoanManager
    {
        // Method to check if a loan is allowed based on the current balance and outstanding loans
        public static decimal LoanAmountAllowed(User user, Account account)
        {
            // Calculate the total balance of all the accounts for the user
            decimal totalBalance = GetTotalBalance(user, account.CurrencyCode);

            // Retrieve the total sum of all outstanding loans for the user
            decimal totalLoans = GetTotalLoans(user, account.CurrencyCode);

            // Calculate the available amount for new loans based on the total balance
            decimal availableToLoan = ((totalBalance - totalLoans)*5) - totalLoans;

            // Return true if the current loans do not exceed the available loan limit
            return Math.Round(Math.Max(availableToLoan, 0), 4);
        }

        //Metod to fetch the total sum of loans for the given user
        public static decimal GetTotalLoans(User user, string currencyCode)
        {
            decimal totalLoans = 0;
            foreach (Loan loan in user.Loans)
            {
                totalLoans += CurrencyManager.ConvertCurrency(loan.Amount, loan.CurrencyCode, currencyCode);
            }
            return Math.Round(totalLoans, 2);
        }

        // Method to get the total balance across all accounts for the user
        public static decimal GetTotalBalance(User user, string currencyCode)
        {
            decimal totalBalance = 0;

            foreach (var account in user.Accounts)
            {
                totalBalance += CurrencyManager.ConvertCurrency(account.Balance, account.CurrencyCode, currencyCode);
            }

            return Math.Round(totalBalance, 2);
        }

        //Method to add a loan to the database for the given user
        public static Loan? CreateLoan(User user, Account account, decimal loanAmount, decimal interestRate = 3, int status = 0)
        {

            // Check if the loan is allowed based on the available balance and outstanding loans
            decimal amountAllowedToLoan = LoanAmountAllowed(user, account);
            if (amountAllowedToLoan <= 0)
            {
                Console.WriteLine($"With your current balance you are only allowed to loan {amountAllowedToLoan}{account.CurrencyCode}.");
                return null;
            }
            //---------------------------------

            // SQL query to insert the loan into the Loans table 
            string insertLoanQuery = @"
                INSERT INTO Loans (UserID, Amount, CurrencyCode, InterestRate, Status, LoanDate)
                VALUES (@UserID, @Amount, @CurrencyCode, @InterestRate, @Status, @LoanDate );
                SELECT * FROM Loans WHERE ID = last_insert_rowid();";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                // If email doesn't exist, proceed with insertion
                var parameters = new
                {
                    UserID = user.ID,
                    Amount = loanAmount,
                    CurrencyCode = account.CurrencyCode,
                    InterestRate = interestRate,
                    Status = 0,
                    LoanDate = DateTime.Now
                };
                Loan loan = connection.QuerySingle<Loan>(insertLoanQuery, parameters);
                GetLoansFromUser(user);
                account.Balance += loanAmount;
                AccountManager.UpdateAccountBalance(account);
                return loan;
            }
        }
        public static void GetLoansFromUser(User? user)
        {
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                var sql = @"SELECT l.*, u.*
                        FROM Loans l
                        INNER JOIN Users u on u.ID = l.UserID
                        WHERE l.UserID = @UserID";
                var loans = connection.Query<Loan, User, Loan>(
                    sql,
                    (loan, user) =>
                    {
                        loan.User = user;  // Assuming Account has a property User to hold User info
                        return loan;
                    },
                    new { UserID = user.ID },
                    splitOn: "ID"  // Use the `ID` column to indicate where the User object starts
                    ).ToList();
                user.Loans = loans;
            }
        }
    }
}