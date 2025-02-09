using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the home screen display by calculating and animating the balance based on transactions.
/// </summary>
public class HomeManager : MonoBehaviour
{
    /// <summary>
    /// Color used to display a positive balance.
    /// </summary>
    [Header("Attributes:")] 
    [Tooltip("Color used for displaying a positive balance.")]
    [SerializeField] private Color positiveColor;

    /// <summary>
    /// Color used to display a negative balance.
    /// </summary>
    [Tooltip("Color used for displaying a negative balance.")]
    [SerializeField] private Color negativeColor;
    
    /// <summary>
    /// Duration (in seconds) over which the balance is animated.
    /// </summary>
    [Tooltip("Duration (in seconds) of the balance animation.")]
    [SerializeField] private float duration = 5f;
    
    /// <summary>
    /// Reference to the DatabaseManager for retrieving transaction data.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Reference to the DatabaseManager used for accessing transaction data.")]
    [SerializeField] private DatabaseManager databaseManager;
    
    /// <summary>
    /// Text component for displaying the balance amount.
    /// </summary>
    [Tooltip("Text component used for displaying the balance.")]
    [SerializeField] private TextMeshProUGUI amountText;
    
    /// <summary>
    /// Text component for displaying the month provisional amount.
    /// </summary>
    [Tooltip("Text component used for displaying the month provisional amount.")]
    [SerializeField] private TextMeshProUGUI monthProvisionalAmountText;
    
    /// <summary>
    /// The CultureInfo for French culture, used for date formatting.
    /// </summary>
    private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
    
    /// <summary>
    /// Supported date formats for parsing transaction dates.
    /// </summary>
    private static readonly string[] DateFormats = { "dddd dd MMMM yyyy", "dddd d MMMM yyyy" };

    /// <summary>
    /// Stores the total calculated balance.
    /// </summary>
    private float _balance;
    
    /// <summary>
    /// Stores the provisional amount for the current month.
    /// </summary>
    private float _provisionalAmount;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Retrieves transactions, calculates the balance, and starts the balance display animation.
    /// </summary>
    void OnEnable()
    {
        // Retrieve the list of transactions from the database.
        List<Transaction> transactions = databaseManager.GetTransactions();
        transactions = AddRehearsalTrades(transactions);
        // Check if any transactions exist.
        if (transactions.Count > 0)
        {
            // Calculate the balance from the transactions.
            CalculateBalance(transactions);
            // Set the text color based on whether the balance is positive or negative.
            amountText.color = _balance >= 0 ? positiveColor : negativeColor;
            // Start the coroutine to animate the display of the balance.
            StartCoroutine(DisplayBalances());
        }
        else
            amountText.text = "0.00 \u20ac";  // If no transactions exist, display a zero balance.
    }

    /// <summary>
    /// Adds rehearsal transactions to the provided list by generating future occurrences.
    /// </summary>
    /// <param name="transactions">The initial list of transactions.</param>
    /// <returns>The updated list of transactions including rehearsals.</returns>
    private List<Transaction> AddRehearsalTrades(List<Transaction> transactions)
    {
        DateTime endDate = GetLastDayOfCurrentMonth();
        List<Transaction> rehearsalsTransactions = databaseManager.GetRehearsalsTrades();
        foreach (var rehearsalElement in rehearsalsTransactions)
        {
            // Build the description marker for recurring transactions.
            string toCompare = rehearsalElement.describe + "#Répétition#";
            // Determine the last known date of the recurring transaction.
            DateTime.TryParseExact(rehearsalElement.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime lastDateConverted);
            foreach (var trade in transactions)
            {
                if (String.Equals(toCompare, trade.describe, StringComparison.Ordinal))
                {
                    DateTime.TryParseExact(trade.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime dateConverted);
                    if (dateConverted > lastDateConverted)
                        lastDateConverted = dateConverted;
                }
            }
            // Calculate the next occurrence date based on the periodicity.
            DateTime nextDate = GetNextDate(lastDateConverted, rehearsalElement.rehearsalsPeriod);
            // Generate provisional transactions until the end date.
            while (nextDate <= endDate)
            {
                // Create a provisional transaction.
                Transaction t = GenerateTransaction(rehearsalElement, nextDate);
                // Add the provisional transaction to the list.
                transactions.Add(t);
                // Calculate the next occurrence date.
                nextDate = GetNextDate(nextDate, rehearsalElement.rehearsalsPeriod);
            }
        }
        return transactions;
    }
    
    /// <summary>
    /// Parses a date string into a DateTime object based on predefined formats.
    /// </summary>
    /// <returns>The last day of the current month.</returns>
    private static DateTime GetLastDayOfCurrentMonth()
    {
        DateTime today = DateTime.Today;
        int lastDay = DateTime.DaysInMonth(today.Year, today.Month);
        return new DateTime(today.Year, today.Month, lastDay);
    }

    /// <summary>
    /// Generates a transaction with the same attributes as the given one but with a new date.
    /// </summary>
    /// <param name="tradeToCopy">The transaction to copy.</param>
    /// <param name="nextDate">The new date for the generated transaction.</param>
    /// <returns>A new transaction instance with updated date and description.</returns>
    private Transaction GenerateTransaction(Transaction tradeToCopy, DateTime nextDate)
    {
        // Append the repetition marker to the original description to indicate it's a recurring transaction.
        string describe = tradeToCopy.describe + "#Répétition#"; 
        // Format the next date using the French culture format.
        string newDate = nextDate.ToString("dddd dd MMMM yyyy", FrenchCulture); 
        // Create a provisional transaction with an id of -1 to denote that it's not stored in the database.
        Transaction t = new Transaction
        {
            id = -1,
            amount = tradeToCopy.amount,
            category = tradeToCopy.category,
            type = tradeToCopy.type,
            date = newDate,
            isRehearsals = false,
            rehearsalsPeriod = "",
            describe = describe,
        };
        return t;
    }
    
    /// <summary>
    /// Determines the next occurrence date based on the given periodicity.
    /// </summary>
    /// <param name="date">The starting date.</param>
    /// <param name="periodicity">The recurrence period ("Day", "Week", "Month", "Year").</param>
    /// <returns>The computed next occurrence date.</returns>
    private DateTime GetNextDate(DateTime date, string periodicity)
    {
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
        return date;
    }

    /// <summary>
    /// Animates the display of the final balance over a specified duration.
    /// </summary>
    /// <returns>An IEnumerator used for coroutine control.</returns>
    private IEnumerator DisplayBalances()
    {
        float elapsedTime = 0f;
        // Gradually interpolate the displayed balance from 0 to finalBalance.
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentBalance = Mathf.Lerp(0f, _balance, elapsedTime / duration);
            float currentMonthBalance = Mathf.Lerp(0f, _provisionalAmount, elapsedTime / duration);
            // Update the texts colors based on calculate.
            amountText.color = currentBalance >= 0 ? positiveColor : negativeColor;
            monthProvisionalAmountText.color = currentMonthBalance >= 0 ? positiveColor : negativeColor;
            // Display the current balances with two decimal places.
            amountText.text = currentBalance.ToString("F2") + " \u20ac";
            monthProvisionalAmountText.text = currentMonthBalance.ToString("F2") + " \u20ac";
            yield return null;
        }
        // Ensure the final balance is displayed accurately.
        amountText.color = _balance >= 0 ? positiveColor : negativeColor;
        monthProvisionalAmountText.color = _provisionalAmount >= 0 ? positiveColor : negativeColor;
        amountText.text = _balance.ToString("F2") + " \u20ac";
        monthProvisionalAmountText.text = _provisionalAmount.ToString("F2") + " \u20ac";
    }

    /// <summary>
    /// Parses and processes transactions to compute the balance and provisional amount.
    /// </summary>
    /// <param name="transactions">List of transactions to process.</param>
    private void CalculateBalance(List<Transaction> transactions)
    {
        DateTime today = DateTime.Today;
        DateTime endDate = GetLastDayOfCurrentMonth();
        _balance = 0f;
        _provisionalAmount = 0f;
        // Process each transaction.
        foreach (var transaction in transactions)
        {
            // Parse the transaction date using the specified formats and French culture.
            if (DateTime.TryParseExact(transaction.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime transactionDate))
            {
                // Only consider transactions dated up to today.
                if (transactionDate <= today)
                {
                    // Subtract amount for expenses; add amount for incomes.
                    if (string.Equals(transaction.type, "expense", StringComparison.Ordinal))
                        _balance -= transaction.amount;
                    else
                        _balance += transaction.amount;
                }

                if (transactionDate > today && transactionDate <= endDate)
                {
                    // Subtract amount for expenses; add amount for incomes.
                    if (string.Equals(transaction.type, "expense", StringComparison.Ordinal))
                        _provisionalAmount -= transaction.amount;
                    else
                        _provisionalAmount += transaction.amount;
                }
            }
        }
        _provisionalAmount = _balance + _provisionalAmount;
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//
    
}
