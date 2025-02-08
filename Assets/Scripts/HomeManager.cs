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
    /// The CultureInfo for French culture, used for date formatting.
    /// </summary>
    private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
    
    /// <summary>
    /// Supported date formats for parsing transaction dates.
    /// </summary>
    private static readonly string[] DateFormats = { "dddd dd MMMM yyyy", "dddd d MMMM yyyy" };

    
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
        // Check if any transactions exist.
        if (transactions.Count > 0)
        {
            // Calculate the balance from the transactions.
            float balance = CalculateBalance(transactions);
            // Set the text color based on whether the balance is positive or negative.
            amountText.color = balance >= 0 ? positiveColor : negativeColor;
            // Start the coroutine to animate the display of the balance.
            StartCoroutine(DisplayBalance(balance));
        }
        else
            amountText.text = "0.00 \u20ac";  // If no transactions exist, display a zero balance.
    }

    /// <summary>
    /// Animates the display of the final balance over a specified duration.
    /// </summary>
    /// <param name="finalBalance">The final balance value to display.</param>
    /// <returns>An IEnumerator used for coroutine control.</returns>
    private IEnumerator DisplayBalance(float finalBalance)
    {
        float elapsedTime = 0f;
        // Gradually interpolate the displayed balance from 0 to finalBalance.
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentBalance = Mathf.Lerp(0f, finalBalance, elapsedTime / duration);
            // Update the text color based on the current balance.
            amountText.color = currentBalance >= 0 ? positiveColor : negativeColor;
            // Display the current balance with two decimal places.
            amountText.text = currentBalance.ToString("F2") + " \u20ac";
            yield return null;
        }
        // Ensure the final balance is displayed accurately.
        amountText.color = finalBalance >= 0 ? positiveColor : negativeColor;
        amountText.text = finalBalance.ToString("F2") + " \u20ac";
    }

    /// <summary>
    /// Calculates the current balance from a list of transactions by summing incomes and subtracting expenses.
    /// Only transactions with dates up to today are considered.
    /// </summary>
    /// <param name="transactions">A list of transactions to process.</param>
    /// <returns>The calculated balance as a float.</returns>
    private float CalculateBalance(List<Transaction> transactions)
    {
        DateTime today = DateTime.Today;
        float balance = 0f;
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
                        balance -= transaction.amount;
                    else
                        balance += transaction.amount;
                }
            }
        }
        return balance;
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//
    
}
