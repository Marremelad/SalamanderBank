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
        // Method to fetch thé user by their ID
        public static User GetUserByID (int userID)
        {
            // Delegates to USerManager to fetch user deatils by ID
            return UserManager.GetUserByID(userID);
        }

        // Method to check if a loan is allowed based on the current balance and outstanding loans
        public static bool IsLoanAllowed(int userID)
        {
            // Fetch the user object based on the provided userID
            User user = GetUserByID(userID);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return false;
            }

            // Calculate the total balance of all the accounts for the user
            decimal totalBalance = GetTotalBalance(user);

            // Retrieve the total sum of all outstanding loans for the user
            decimal totalLoans = GetTotalLoans(user);

            // Calculate the available amount for new loans based on the total balance
            decimal availableToLoan = (totalBalance * 5) - totalLoans;

            // Return true if the current loans do not exceed the available loan limit
            return totalLoans <= availableToLoan;
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
        public static bool AddLoan(int userID, decimal loanAmount, decimal interestRate)
        {
            // Fetch the user object based on the provided userID
            User user = GetUserByID(userID);
            if (user == null)
            {
                Console.WriteLine("Användare ej funnen.");
                return false;
            }

            // Fetch the user's account information (Assuming the user has at least one account)
            var account = user.Accounts.FirstOrDefault();
            if (account == null)
            {
                Console.WriteLine("No account exists for user.");
                return false;
            }

            // Check if the loan is allowed based on the available balance and outstanding loans
            if (!IsLoanAllowed(userID))
            {
                Console.WriteLine("Loan exceeds allowable limit.");
                return false;
            }


            //---------------------------------

            // SQL query to insert the loan into the Loans table 
            string insertLoanQuery = @"
        INSERT INTO Loans (UserID, Amount, InterestRate)
        VALUES (@UserID, @Amount, @InterestRate, @Status)";

            // using Dapper to execute the insert operation            
            using (var connection = new SQLiteConnection(DB._connectionString))
            {
                connection.Open();
                // Excute the insert query with paramenters 
                int affectedRows = connection.Execute(insertLoanQuery, new
                {
                    UserID = user.ID,
                    Amount = loanAmount,
                    InterestRate = interestRate,
                    Status = 0 // Loan is pending (status 0 typically means pending or inactive) 
                });

                // Check if the loan was added successfully
                if (affectedRows > 0)
                {
                    Console.WriteLine("Loan successfully added.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to add loan.");
                    return false;
                }
            }
        }


        // New method to collect loan input from the user
        public static void CollectLoanInput()
        {
            // Prompt the user for their ID
            Console.WriteLine("Enter your user ID:");
            int userId;
            while (!int.TryParse(Console.ReadLine(), out userId) || userId <= 0)
            {
                Console.WriteLine("Please enter a valid positive integer for the user ID.");
            }

            // Create a User object
            User user = new User { ID = userId }; // Assume User object is created based on input

            // Prompt for loan amount
            Console.WriteLine("Enter loan amount:");
            decimal loanAmount;
            while (!decimal.TryParse(Console.ReadLine(), out loanAmount) || loanAmount <= 0)
            {
                Console.WriteLine("Please enter a valid positive amount for the loan.");
            }

            // Prompt for interest rate
            Console.WriteLine("Enter interest rate (e.g., 5.5 for 5.5%):");
            decimal interestRate;
            while (!decimal.TryParse(Console.ReadLine(), out interestRate) || interestRate <= 0)
            {
                Console.WriteLine("Please enter a valid positive interest rate.");
            }

            // Call AddLoan to add the loan to the database
            bool loanAdded = AddLoan(userId, loanAmount, interestRate);

            if (loanAdded)
            {
                Console.WriteLine("Loan has been successfully added!");
            }
            else
            {
                Console.WriteLine("Failed to add the loan.");
            }
        }
    }
}
