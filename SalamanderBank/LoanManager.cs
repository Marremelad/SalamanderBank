using Dapper;
using Microsoft.AspNetCore.Identity;
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
        public static decimal LoanAmountAllowed(User user)
        {
            // Fetch the user object based on the provided userID
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return 0;
            }

            // Calculate the total balance of all the accounts for the user
            decimal totalBalance = GetTotalBalance(user);

            // Retrieve the total sum of all outstanding loans for the user
            decimal totalLoans = GetTotalLoans(user);

            // Calculate the available amount for new loans based on the total balance
            decimal availableToLoan = (totalBalance * 5) - totalLoans;

            // Return true if the current loans do not exceed the available loan limit
            return Math.Max(availableToLoan, 0);
        }

        //Metod to fetch the total sum of loans for the given user
        public static decimal GetTotalLoans(User user)
        {
            decimal totalLoans = 0;

            // Query to fetch the sum of all loans for the user (assuming loans are stored in a separate table)
            string query = "SELECT SUM(Amount) FROM Loans WHERE UserID = @UserID";

            //Using Dapper and SQLite to execute the query
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                //Execute query and get the sum of loans 
                totalLoans = connection.ExecuteScalar<decimal>(query, new { UserID = user.ID });
            }

            return totalLoans;
        }

        // Method to get the total balance across all accounts for the user
        public static decimal GetTotalBalance(User user)
        {
            decimal totalBalance = 0;

            // Query to sum up all account balances for the user
            string query = "SELECT SUM(Balance) FROM Accounts WHERE UserID = @UserID";

            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                //Execute query and get the total balance
                totalBalance = connection.ExecuteScalar<decimal>(query, new { UserID = user.ID });
            }

            return totalBalance;
        }

        //Method to add a loan to the database for the given user
        public static Loan? CreateLoan(User user, decimal loanAmount, decimal interestRate = 3)
        {

            // Check if the loan is allowed based on the available balance and outstanding loans
            decimal amountAllowedToLoan = LoanAmountAllowed(user);
            if (amountAllowedToLoan <= 0)
            {
                Console.WriteLine($"With your current balance you are only allowed to loan {amountAllowedToLoan}.");
                return null;
            }
            //---------------------------------

            // SQL query to insert the loan into the Loans table 
            string insertLoanQuery = @"
                INSERT INTO Loans (UserID, Amount, InterestRate)
                VALUES (@UserID, @Amount, @InterestRate, @Status)
                SELECT * FROM Users WHERE Id = last_insert_rowid();";

            using (SQLiteConnection connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                // If email doesn't exist, proceed with insertion
                var parameters = new
                {
                    UserID = user.ID,
                    Amount = loanAmount,
                    InterestRate = interestRate
                };
            Loan loan = connection.QuerySingle<Loan>(insertLoanQuery, parameters);
            return loan;
            } 
        }
    }
}
