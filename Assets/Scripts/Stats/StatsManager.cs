using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the statistics view by filtering transactions by date and category, computing totals,
/// and displaying the data graphically.
/// </summary>
public class StatsManager : MonoBehaviour
{
    /// <summary>
    /// Color used to display positive amounts.
    /// </summary>
    [Header("Attributes:")]
    [Tooltip("Color used for positive values in the statistics.")]
    [SerializeField] private Color positiveColor;
    
    /// <summary>
    /// Color used to display negative amounts.
    /// </summary>
    [Tooltip("Color used for negative values in the statistics.")]
    [SerializeField] private Color negativeColor;
    
    /// <summary>
    /// Text element displaying the start date of the statistics period.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Text element for the start date of the statistics period.")]
    [SerializeField] private TextMeshProUGUI startDateText;
    
    /// <summary>
    /// Text element displaying the end date of the statistics period.
    /// </summary>
    [Tooltip("Text element for the end date of the statistics period.")]
    [SerializeField] private TextMeshProUGUI endDateText;
    
    /// <summary>
    /// Dropdown for selecting a category to filter transactions.
    /// </summary>
    [Tooltip("Dropdown for selecting a category filter.")]
    [SerializeField] private TMP_Dropdown categoryDropdown;
    
    /// <summary>
    /// Text element displaying the total expense amount.
    /// </summary>
    [Tooltip("Text element displaying the total expenses.")]
    [SerializeField] private TextMeshProUGUI expenseAmountText;
    
    /// <summary>
    /// Text element displaying the total income amount.
    /// </summary>
    [Tooltip("Text element displaying the total incomes.")]
    [SerializeField] private TextMeshProUGUI incomeAmountText;
    
    /// <summary>
    /// Text element displaying the overall balance (income minus expenses).
    /// </summary>
    [Tooltip("Text element displaying the overall balance.")]
    [SerializeField] private TextMeshProUGUI balanceAmountText;
    
    /// <summary>
    /// Reference to the DatabaseManager for accessing transaction data.
    /// </summary>
    [Tooltip("Reference to the DatabaseManager for retrieving transactions.")]
    [SerializeField] private DatabaseManager databaseManager;
    
    /// <summary>
    /// Reference to the CategoryListManager for loading category data.
    /// </summary>
    [Tooltip("Reference to the CategoryListManager for loading available categories.")]
    [SerializeField] private CategoryListManager categoryListManager;
    
    /// <summary>
    /// Reference to the GraphDisplayManager that handles the graphical display of statistics.
    /// </summary>
    [Tooltip("Reference to the GraphDisplayManager for displaying graphs.")]
    [SerializeField] private GraphDisplayManager graphDisplayManager;

    /// <summary>
    /// Stores the total expense amount.
    /// </summary>
    private float _expenseAmount;
    
    /// <summary>
    /// Stores the total income amount.
    /// </summary>
    private float _incomeAmount;
    
    /// <summary>
    /// CultureInfo for French culture, used for date formatting.
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
    /// Initializes the statistics panel with default dates, resets amounts, and loads categories.
    /// </summary>
    private void OnEnable()
    {
        // Set the start date to a fixed value.
        startDateText.text = "samedi 1 février 2025";
        // Set the end date to today's date, formatted in French.
        endDateText.text = DateTime.Today.ToString("dddd dd MMMM yyyy", FrenchCulture);
        // Reset the category filter to the first option.
        categoryDropdown.value = 0;
        // Clear displayed amounts.
        expenseAmountText.text = "";
        incomeAmountText.text = "";
        balanceAmountText.text = "";
        // Reset internal amount trackers.
        _expenseAmount = 0f;
        _incomeAmount = 0f;
        // Initialize the category dropdown options.
        InitCategories();
        // Update the displayed amounts.
        expenseAmountText.text = _expenseAmount.ToString("F2");
        incomeAmountText.text = _incomeAmount.ToString("F2");
        balanceAmountText.text = (_incomeAmount - _expenseAmount).ToString("F2");
    }

    /// <summary>
    /// Called when the object becomes disabled or inactive.
    /// Resets the statistics panel data.
    /// </summary>
    private void OnDisable()
    {
        ResetData();
    }

    /// <summary>
    /// Initializes the category dropdown list with a default option and loaded categories.
    /// </summary>
    private void InitCategories()
    {
        // Clear any existing dropdown options.
        categoryDropdown.options.Clear();
        // Load categories if they are not already loaded.
        if (categoryListManager.Categories == null || categoryListManager.Categories.Count == 0)
            categoryListManager.LoadCategories();
        // Add a default option for "All categories".
        TMP_Dropdown.OptionData firstOption = new TMP_Dropdown.OptionData
        {
            text = "Toutes les catégories",
        };
        // Populate dropdown with available categories.
        categoryDropdown.options.Add(firstOption);
        if (categoryListManager.Categories != null)
        {
            foreach (var element in categoryListManager.Categories)
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData
                {
                    text = element,
                };
                categoryDropdown.options.Add(newOption);
            }
        }
    }

    /// <summary>
    /// Retrieves transactions that exist between the given start and end dates.
    /// </summary>
    /// <param name="startDate">The start date for filtering transactions.</param>
    /// <param name="endDate">The end date for filtering transactions.</param>
    /// <returns>A list of transactions within the specified date range.</returns>
    private List<Transaction> GetExistingTrades(DateTime startDate, DateTime endDate)
    {
        List<Transaction> trades = databaseManager.GetTransactions();
        List<Transaction> tradesFound = new List<Transaction>();
        // Filter transactions by date.
        foreach (var trade in trades)
        {
            DateTime.TryParseExact(trade.date, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime dateFormated);
            if (dateFormated >= startDate && dateFormated <= endDate)
                tradesFound.Add(trade);
        }
        return tradesFound;
    }
    
    /// <summary>
    /// Retrieves transactions, including provisional recurring transactions, up to the specified end date.
    /// </summary>
    /// <param name="endDate">The end date to consider for provisional transactions.</param>
    /// <returns>A list of transactions including provisional recurring ones.</returns>
    private List<Transaction> GetTradesWithProvisional(DateTime endDate)
    {
        List<Transaction> transactions = databaseManager.GetTransactions();
        List<Transaction> rehearsalsTrades = databaseManager.GetRehearsalsTrades();
        // Process each recurring transaction.
        foreach (var rehearsalElement in rehearsalsTrades)
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
    /// Generates a provisional transaction based on a given transaction template and a specified new date.
    /// </summary>
    /// <param name="tradeToCopy">The transaction to copy properties from.</param>
    /// <param name="nextDate">The new date for the provisional transaction.</param>
    /// <returns>
    /// A new <see cref="Transaction"/> object with updated date and description, 
    /// marked as a provisional (non-recurring) transaction.
    /// </returns>
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
    /// Calculates the next occurrence date based on the current date and the provided periodicity.
    /// </summary>
    /// <param name="date">The current date.</param>
    /// <param name="periodicity">The periodicity of recurrence ("Day", "Week", "Month", or "Year").</param>
    /// <returns>The next occurrence date.</returns>
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
    /// Retrieves transactions between two dates given as strings.
    /// Determines whether to include provisional recurring transactions based on the end date.
    /// </summary>
    /// <param name="startDate">The start date as a string.</param>
    /// <param name="endDate">The end date as a string.</param>
    /// <returns>A list of transactions between the specified dates.</returns>
    private List<Transaction> GetTransactionsDateToDate(string startDate, string endDate)
    {
        // Parse the start and end dates.
        DateTime.TryParseExact(startDate, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime startDateFormated);
        DateTime.TryParseExact(endDate, DateFormats, FrenchCulture, DateTimeStyles.None, out DateTime endDateFormated);
        List<Transaction> transactions;
        // If the end date is in the future, include provisional recurring transactions.
        if (endDateFormated > DateTime.Today)
            transactions = GetTradesWithProvisional(endDateFormated);
        else
            transactions = GetExistingTrades(startDateFormated, endDateFormated);
        return transactions;
    }

    /// <summary>
    /// Displays the graph by resetting the graph panel, retrieving filtered transactions, and updating the amounts.
    /// </summary>
    private void DisplayGraph()
    {
        // Reset the graph panel.
        graphDisplayManager.ResetPanel();
        // Retrieve transactions between the specified start and end dates.
        List<Transaction> transactions = GetTransactionsDateToDate(startDateText.text, endDateText.text);
        // Display the graph based on the selected category filter.
        graphDisplayManager.DisplayGraph(transactions, categoryDropdown.options[categoryDropdown.value].text);
        // Update the expense, income, and balance amounts.
        expenseAmountText.text = graphDisplayManager.GetExpenseAmount().ToString("F2");
        incomeAmountText.text = graphDisplayManager.GetIncomeAmount().ToString("F2");
        balanceAmountText.text = (graphDisplayManager.GetIncomeAmount() - graphDisplayManager.GetExpenseAmount()).ToString("F2");
    }

    /// <summary>
    /// Resets the statistics panel data to its default state.
    /// </summary>
    private void ResetData()
    {
        // Reset the graph display.
        graphDisplayManager.ResetPanel();
        // Reset dates and dropdown selections.
        startDateText.text = "samedi 1 février 2025";
        endDateText.text = DateTime.Today.ToString("dddd dd MMMM yyyy", FrenchCulture);
        categoryDropdown.value = 0;
        // Clear amount texts.
        expenseAmountText.text = "";
        incomeAmountText.text = "";
        balanceAmountText.text = "";
        // Reset internal totals.
        _expenseAmount = 0f;
        _incomeAmount = 0f;
    }
    

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Validates the current settings and displays the graph with updated statistics.
    /// </summary>
    public void ValidateButton()
    {
        DisplayGraph();
    }
}
