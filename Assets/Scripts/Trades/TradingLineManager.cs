using System;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages a single trading line in the UI, including displaying transaction details
/// and handling actions such as stopping a transaction.
/// </summary>
public class TradingLineManager : MonoBehaviour
{
    /// <summary>
    /// Color used to display positive transaction amounts.
    /// </summary>
    [Header("Attributes:")]
    [Tooltip("Color used to display positive transaction amounts.")]
    [SerializeField] private Color positiveColor;
    
    /// <summary>
    /// Color used to display negative transaction amounts.
    /// </summary>
    [Tooltip("Color used to display negative transaction amounts.")]
    [SerializeField] private Color negativeColor;
    
    /// <summary>
    /// Text component that displays the transaction description.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Text component that displays the transaction description.")]
    [SerializeField] private TextMeshProUGUI describeText;
    
    /// <summary>
    /// Text component that displays the periodicity of the transaction.
    /// </summary>
    [Tooltip("Text component that displays the periodicity of the transaction.")]
    [SerializeField] private TextMeshProUGUI periodicityText;
    
    /// <summary>
    /// Text component that displays the transaction amount.
    /// </summary>
    [Tooltip("Text component that displays the transaction amount.")]
    [SerializeField] private TextMeshProUGUI amountText;
    
    /// <summary>
    /// Text component that displays the next scheduled transaction date.
    /// </summary>
    [Tooltip("Text component that displays the next scheduled transaction date.")]
    [SerializeField] private TextMeshProUGUI dateText;

    /// <summary>
    /// Unique identifier for the trading transaction.
    /// </summary>
    private int _tradingId;
    
    /// <summary>
    /// Indicates whether the transaction is an expense.
    /// </summary>
    private bool _isExpense;
    
    /// <summary>
    /// Stores the periodicity (e.g., "Day", "Week", "Month", "Year") for recurring transactions.
    /// </summary>
    private string _periodicity;
    
    /// <summary>
    /// Reference to the DatabaseManager that handles transaction persistence.
    /// </summary>
    private DatabaseManager _databaseManager;
    
    /// <summary>
    /// Reference to the TradingListManager that manages the list of trading lines.
    /// </summary>
    private TradingListManager _tradingListManager;
    
    /// <summary>
    /// Array of date formats used to parse transaction dates.
    /// </summary>
    private static readonly string[] DateFormats = { "dddd dd MMMM yyyy", "dddd d MMMM yyyy" };
    
    /// <summary>
    /// Culture information for French, used for date parsing.
    /// </summary>
    private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");

    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes references to the DatabaseManager and TradingListManager.
    /// </summary>
    private void Awake()
    {
        _databaseManager = FindFirstObjectByType<DatabaseManager>();
        _tradingListManager = FindFirstObjectByType<TradingListManager>();
    }

    /// <summary>
    /// Sets the amount text with appropriate formatting and color based on whether the transaction is an expense.
    /// </summary>
    /// <param name="amount">The transaction amount to display.</param>
    private void FillAmountText(float amount)
    {
        // Set the text color: negativeColor for expenses, positiveColor otherwise.
        amountText.color = _isExpense ? negativeColor : positiveColor;
        // Format the amount to display two decimal places.
        amountText.text = amount.ToString("F2");
    }

    /// <summary>
    /// Parses the provided date string and sets the date text to the next valid transaction date.
    /// </summary>
    /// <param name="date">The date string to parse.</param>
    private void FillDateText(string date)
    {
        // Try to parse the date using predefined formats and French culture.
        if (DateTime.TryParseExact(date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime transactionDate))
        {
            // Calculate the next date for the transaction based on its periodicity.
            DateTime nextDate = GetNextDate(transactionDate);
            dateText.text = nextDate.ToString("dd/MM/yyyy");
        }
        else
            dateText.text = "??/??/????";  // If parsing fails, display a placeholder.
    }

    /// <summary>
    /// Calculates the next valid transaction date based on the provided date and the transaction's periodicity.
    /// </summary>
    /// <param name="date">The starting transaction date.</param>
    /// <returns>The next date when the transaction should occur.</returns>
    private DateTime GetNextDate(DateTime date)
    {
        DateTime today = DateTime.Today;
        // If the transaction date is in the future or no periodicity is defined, return the original date.
        if (today < date || string.IsNullOrEmpty(_periodicity))
            return date;
        // Increment the date until it is after today.
        while (date <= today)
        {
            switch (_periodicity)
            {
                case "Day":
                    date = date.AddDays(1);
                    break;
                case "Week":
                    date = date.AddDays(7);
                    break;
                case "Month":
                    date = date.AddMonths(1);
                    break;
                case "Year":
                    date = date.AddYears(1);
                    break;
            }
        }
        return date;
    }

    /// <summary>
    /// Translates the English periodicity string into its French equivalent.
    /// </summary>
    /// <param name="period">The periodicity string in English.</param>
    /// <returns>The translated periodicity string in French.</returns>
    private string FrenchTranslatedText(string period)
    {
        switch (period)
        {
            case "Day":
                return "Journalier";
            case "Week":
                return "Hebdomadaire";
            case "Month":
                return "Mensuel";
            case "Year":
                return "Annuel";
        }
        return "";
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Sets the transaction description, assigns a unique identifier, and determines the transaction type.
    /// </summary>
    /// <param name="text">The description of the transaction.</param>
    /// <param name="id">The unique identifier of the transaction.</param>
    /// <param name="type">The type of transaction (e.g., "expense").</param>
    public void SetDescribeText(string text, int id, string type)
    {
        describeText.text = text;
        _tradingId = id;
        // If the type is null, empty, or "expense", mark the transaction as an expense.
        if (string.IsNullOrEmpty(type) || string.CompareOrdinal("expense", type) == 0)
            _isExpense = true;
        else
            _isExpense = false;
    }

    /// <summary>
    /// Sets the periodicity text and stores the periodicity value.
    /// The text is translated to French before being displayed.
    /// </summary>
    /// <param name="text">The periodicity of the transaction (e.g., "Day", "Week").</param>
    public void SetPeriodicityText(string text)
    {
        _periodicity = text;
        periodicityText.text = FrenchTranslatedText(text);
    }

    /// <summary>
    /// Sets the transaction amount text using a helper method for formatting.
    /// </summary>
    /// <param name="amount">The transaction amount to display.</param>
    public void SetAmountText(float amount)
    {
        FillAmountText(amount);
    }

    /// <summary>
    /// Sets the transaction date text by parsing and formatting the provided date string.
    /// </summary>
    /// <param name="date">The date string of the transaction.</param>
    public void SetDateText(string date)
    {
        FillDateText(date);
    }

    /// <summary>
    /// Stops the trading transaction by marking it as stopped in the database
    /// and removing the corresponding trading line from the UI.
    /// </summary>
    public void StopTransaction()
    {
        if (_databaseManager != null && _tradingListManager != null)
        {
            _databaseManager.StopTrade(_tradingId);
            _tradingListManager.DeleteLine(gameObject);
        }
    }
}
