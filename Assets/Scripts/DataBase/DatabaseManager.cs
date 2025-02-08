using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Mono.Data.SqliteClient;
using UnityEngine;

/// <summary>
/// Manages the SQLite database used for storing transactions and recurring trades.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    [Header("Attributes:")]
    [Tooltip("The name (and relative path) of the database file. This will be appended to Application.persistentDataPath.")]
    [SerializeField] private string databaseName = "/BudgetDB.db";  // The name (and relative path) of the database file. This will be appended to Application.persistentDataPath.
    
    /// <summary>
    /// The connection path string for the SQLite database.
    /// </summary>
    private string _connectionPath;
    
    /// <summary>
    /// The CultureInfo used for French culture.
    /// </summary>
    private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
    
    /// <summary>
    /// The set of acceptable date formats for parsing dates.
    /// </summary>
    private static readonly string[] DateFormats = { "dddd dd MMMM yyyy", "dddd d MMMM yyyy" };
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Initializes the database connection path, creates the database if it doesn't exist, and updates recurring transactions.
    /// </summary>
    private void Awake()
    {
        // Build the database connection path.
        _connectionPath = "URI=file:" + Application.persistentDataPath + databaseName;
        // Create the database if it doesn't exist.
        CreateDatabase();
        // Update the database with any pending recurring transactions.
        UpdateDatabase();
    }

    /// <summary>
    /// Updates the database by processing recurring (rehearsals) transactions.
    /// </summary>
    /// <remarks>
    /// The method finds all transactions and recurring transactions (rehearsals trades). For each recurring transaction,
    /// it determines the most recent date for that transaction (by checking all transactions with the same description).
    /// It then calculates the next recurrence date based on the given periodicity (Day, Week, Month, or Year) and
    /// records new transactions until the next date is after today's date.
    /// </remarks>
    private void UpdateDatabase()
    {
        // Retrieve all transactions and recurring transactions.
        List<Transaction> transactions = FindTransactions();
        List<Transaction> rehearsalsTrades = FindRehearsalsTrades();
        // Get today's date.
        DateTime today = DateTime.Today;
        // Process each recurring transaction.
        foreach (var rehearsalElement in rehearsalsTrades)
        {
            // Prepare the description used for recurring transactions.
            string toCompare = rehearsalElement.describe + "#Répétition#";
            // Parse the date of the recurring transaction using French culture.
            DateTime.TryParseExact(rehearsalElement.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime lastDateConverted);
            // Find the most recent date among the transactions with matching description.
            foreach (var trade in transactions)
            {
                if (String.Equals(toCompare, trade.describe, StringComparison.Ordinal))
                {
                    DateTime.TryParseExact(trade.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime dateConverted);
                    if (dateConverted > lastDateConverted)
                        lastDateConverted = dateConverted;
                }
            }
            // Calculate the next recurrence date based on the periodicity.
            DateTime nextDate = GetNextDate(lastDateConverted, rehearsalElement.rehearsalsPeriod);
            // Continue recording transactions until the next date is in the future.
            while (nextDate <= today)
            {
                // Construct the recurring transaction description.
                string describe = rehearsalElement.describe + "#Répétition#";
                // Format the next date in French format.
                string newDate = nextDate.ToString("dddd dd MMMM yyyy", FrenchCulture);
                // Record the recurring transaction.
                RecordTransactions(rehearsalElement.amount, rehearsalElement.category, rehearsalElement.type, newDate, false, rehearsalElement.rehearsalsPeriod, describe);
                // Calculate the next recurrence date.
                nextDate = GetNextDate(nextDate, rehearsalElement.rehearsalsPeriod);
            }
        }
    }
    
    /// <summary>
    /// Calculates the next date based on a given date and periodicity.
    /// </summary>
    /// <param name="date">The current date.</param>
    /// <param name="periodicity">The recurrence period ("Day", "Week", "Month", or "Year").</param>
    /// <returns>The next recurrence date.</returns>
    private DateTime GetNextDate(DateTime date, string periodicity)
    {
        // Determine the next date based on the specified periodicity.
        switch (periodicity)
        {
            case "Day":
                return date.AddDays(1);
            case "Week":
                return date.AddDays(7);
            case "Month":
                return date.AddMonths(1);
            case "Year":
                return date.AddYears(1);
        }
        // If the periodicity is not recognized, return the original date.
        return date;
    }
    
    /// <summary>
    /// Creates the database and the transactions table if they do not already exist.
    /// </summary>
    private void CreateDatabase()
    {
        // Open a connection to the database.
        using (var connection = new SqliteConnection(_connectionPath))
        {
            connection.Open();
            // Create the transactions table if it doesn't exist.
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY AUTOINCREMENT, amount REAL, category TEXT, type TEXT, date TEXT, isRehearsals BOOLEAN, rehearsalsPeriod TEXT, describe TEXT)";
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Retrieves all transactions from the database, ordered by date in descending order.
    /// </summary>
    /// <returns>A list of transactions.</returns>
    private List<Transaction> FindTransactions()
    {
        List<Transaction> transactions = new List<Transaction>();
        // Open a connection to the database.
        using (var connection = new SqliteConnection(_connectionPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Query to select all transactions ordered by date descending.
                command.CommandText = "SELECT * FROM transactions ORDER BY date DESC";
                using (IDataReader reader = command.ExecuteReader())
                {
                    // Read each record and convert it into a Transaction object.
                    while (reader.Read())
                    {
                        Transaction t = new Transaction
                        {
                            id = reader.GetInt32(0),
                            amount = reader.GetFloat(1),
                            category = reader.GetString(2),
                            type = reader.GetString(3),
                            date = reader.GetString(4),
                            isRehearsals = reader.GetBoolean(5),
                            rehearsalsPeriod = reader.GetString(6),
                            describe = reader.GetString(7)
                        };
                        transactions.Add(t);
                    }
                }
            }
        }
        return transactions;
    }

    /// <summary>
    /// Records a new transaction in the database.
    /// </summary>
    /// <param name="amount">The amount of the transaction.</param>
    /// <param name="category">The category of the transaction.</param>
    /// <param name="type">The type of the transaction (e.g., income or expense).</param>
    /// <param name="date">The date of the transaction, formatted as a string.</param>
    /// <param name="isRehearsals">Indicates whether the transaction is recurring.</param>
    /// <param name="rehearsalsPeriod">The recurrence period (e.g., "Day", "Week", "Month", or "Year").</param>
    /// <param name="describe">The description of the transaction.</param>
    private void RecordTransactions(float amount, string category, string type, string date, bool isRehearsals,
        string rehearsalsPeriod, string describe)
    {
        // Open a connection to the database.
        using (var connection = new SqliteConnection(_connectionPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Prepare the SQL command to insert a new transaction.
                command.CommandText = "INSERT INTO transactions (amount, category, type, date, isRehearsals, rehearsalsPeriod, describe) VALUES (@amount, @category, @type, @date, @isRehearsals, @rehearsalsPeriod, @describe)";
                command.Parameters.Add(new SqliteParameter("@amount", amount));
                command.Parameters.Add(new SqliteParameter("@category", category));
                command.Parameters.Add(new SqliteParameter("@type", type));
                command.Parameters.Add(new SqliteParameter("@date", date));
                command.Parameters.Add(new SqliteParameter("@isRehearsals", isRehearsals));
                command.Parameters.Add(new SqliteParameter("@rehearsalsPeriod", rehearsalsPeriod));
                command.Parameters.Add(new SqliteParameter("@describe", describe));
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Retrieves all recurring (rehearsals) transactions from the database.
    /// </summary>
    /// <returns>A list of recurring transactions.</returns>
    private List<Transaction> FindRehearsalsTrades()
    {
        List<Transaction> rehearsalsTrades = new List<Transaction>();
        using (var connection = new SqliteConnection(_connectionPath))
        {
            connection.Open();
            // Open a connection to the database.
            using (var command = connection.CreateCommand())
            {
                // Query to select all transactions where isRehearsals is true.
                command.CommandText = "SELECT * FROM transactions WHERE isRehearsals = 1";
                using (IDataReader reader = command.ExecuteReader())
                {
                    // Read each record and convert it into a Transaction object.
                    while (reader.Read())
                    {
                        rehearsalsTrades.Add(new Transaction
                        {
                            id = reader.GetInt32(0),
                            amount = reader.GetFloat(1),
                            category = reader.GetString(2),
                            type = reader.GetString(3),
                            date = reader.GetString(4),
                            isRehearsals = reader.GetBoolean(5),
                            rehearsalsPeriod = reader.GetString(6),
                            describe = reader.GetString(7)
                        });
                    }
                }
            }
        }
        return rehearsalsTrades;
    }

    /// <summary>
    /// Stops a recurring transaction by updating its isRehearsals flag and clearing its rehearsalsPeriod.
    /// </summary>
    /// <param name="tradeId">The unique identifier of the transaction to stop recurring.</param>
    private void StopRehearsalsTrade(int tradeId)
    {
        // Open a connection to the database.
        using (var connection = new SqliteConnection(_connectionPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Prepare the SQL command to stop the recurring transaction.
                command.CommandText = "UPDATE transactions SET isRehearsals = 0, rehearsalsPeriod = '' WHERE id = @id";
                command.Parameters.Add(new SqliteParameter("@id", tradeId));
                command.ExecuteNonQuery();
            }
        }
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Retrieves all transactions from the database.
    /// </summary>
    /// <returns>A list of all transactions.</returns>
    public List<Transaction> GetTransactions()
    {
        return FindTransactions();
    }
    
    /// <summary>
    /// Retrieves all recurring (rehearsals) transactions from the database.
    /// </summary>
    /// <returns>A list of recurring transactions.</returns>
    public List<Transaction> GetRehearsalsTrades()
    {
        return FindRehearsalsTrades();
    }

    /// <summary>
    /// Adds a new transaction to the database and updates recurring transactions if necessary.
    /// </summary>
    /// <param name="amount">The amount of the transaction.</param>
    /// <param name="category">The category of the transaction.</param>
    /// <param name="type">The type of the transaction (e.g., income or expense).</param>
    /// <param name="date">The date of the transaction as a formatted string.</param>
    /// <param name="isRehearsals">Indicates whether the transaction is recurring.</param>
    /// <param name="rehearsalsPeriod">The recurrence period (e.g., "Day", "Week", "Month", or "Year").</param>
    /// <param name="describe">The description of the transaction.</param>
    public void AddTransaction(float amount, string category, string type, string date, bool isRehearsals, string rehearsalsPeriod, string describe)
    {
        // Record the new transaction.
        RecordTransactions(amount, category, type, date, isRehearsals, rehearsalsPeriod, describe);
        // Update the database in case recurring transactions need to be processed.
        UpdateDatabase();
    }

    /// <summary>
    /// Stops a recurring transaction by its ID.
    /// </summary>
    /// <param name="idTrade">The unique identifier of the transaction to stop.</param>
    public void StopTrade(int idTrade)
    {
        StopRehearsalsTrade(idTrade);
    }
}
