using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the display of a financial graph based on transaction data,
/// including generating slices and legend lines to represent various categories.
/// </summary>
public class GraphDisplayManager : MonoBehaviour
{
    /// <summary>
    /// List of colors used for graph slices when displaying all categories.
    /// </summary>
    [Header("Attributes:")]
    [Tooltip("List of colors for graph slices when displaying all categories.")]
    [SerializeField] private List<Color> sliceColors;
    
    /// <summary>
    /// List of colors used for graph slices when displaying a single category.
    /// </summary>
    [Tooltip("List of colors for graph slices when displaying one category.")]
    [SerializeField] private List<Color> oneCategoryColors;
    
    /// <summary>
    /// The area where the graph slices are displayed.
    /// </summary>
    [Header("Components:")]
    [Tooltip("GameObject representing the graph display area.")]
    [SerializeField] private GameObject graphArea;
    
    /// <summary>
    /// Prefab for a single graph slice (should be a RectTransform with an Image component).
    /// </summary>
    [Tooltip("Prefab for a single graph slice.")]
    [SerializeField] private RectTransform slicePrefab;
    
    /// <summary>
    /// Grid layout used to display legend lines.
    /// </summary>
    [Tooltip("GameObject for the grid layout that holds legend lines.")]
    [SerializeField] private GameObject gridLayout;
    
    /// <summary>
    /// Prefab for a legend line.
    /// </summary>
    [Tooltip("Prefab for a legend line.")]
    [SerializeField] private GameObject legendLine;
    
    /// <summary>
    /// List of instantiated slice objects.
    /// </summary>
    private List<RectTransform> _sliceObjects;
    
    /// <summary>
    /// List of instantiated legend line objects.
    /// </summary>
    private List<GameObject> _legendLineObjects;
    
    /// <summary>
    /// Total expense amount calculated from the transactions.
    /// </summary>
    private float _expenseAmount;
    
    /// <summary>
    /// Total income amount calculated from the transactions.
    /// </summary>
    private float _incomeAmount;
    
    /// <summary>
    /// Total sum of income and expense amounts.
    /// </summary>
    private float _totalAmount;
    
    /// <summary>
    /// List of lists containing transactions grouped by category.
    /// </summary>
    private List<List<Transaction>> _categoryTradesList;
    
    /// <summary>
    /// List of subtotal amounts per category.
    /// </summary>
    private List<float> _subtotals;
    
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the object becomes enabled. Initializes lists used for graph display.
    /// </summary>
    private void OnEnable()
    {
        _sliceObjects = new List<RectTransform>();
        _legendLineObjects = new List<GameObject>();
        _categoryTradesList = new List<List<Transaction>>();
        _subtotals = new List<float>();
    }

    /// <summary>
    /// Called when the object becomes disabled. Resets the graph data and cleans up instantiated objects.
    /// </summary>
    private void OnDisable()
    {
        ResetData();
    }

    /// <summary>
    /// Resets the graph display data by destroying all instantiated slices and legend lines,
    /// and clearing the grouping and subtotal lists.
    /// </summary>
    private void ResetData()
    {
        // Destroy all slice objects.
        if (_sliceObjects != null)
        {
            foreach (var currentSlice in _sliceObjects)
                Destroy(currentSlice.gameObject);
            _sliceObjects.Clear();
        }
        // Destroy all legend line objects.
        if (_legendLineObjects != null)
        {
            foreach (var currentLine in _legendLineObjects)
                Destroy(currentLine);
            _legendLineObjects.Clear();
        }
        // Clear each category trade list.
        if (_categoryTradesList != null)
        {
            foreach (var categoryTrades in _categoryTradesList)
                categoryTrades.Clear();
            _categoryTradesList.Clear();
        }
        _subtotals.Clear();
        _totalAmount = 0f;
        _expenseAmount = 0f;
        _incomeAmount = 0f;
    }

    /// <summary>
    /// Groups transactions by their category into separate lists.
    /// </summary>
    /// <param name="transactions">The list of transactions to be grouped.</param>
    private void GetAllCategoryTrades(List<Transaction> transactions)
    {
        foreach (var trade in transactions)
        {
            // If no groups exist, create the first group.
            if (_categoryTradesList.Count == 0)
            {
                List<Transaction> newList = new List<Transaction> { trade };
                _categoryTradesList.Add(newList);
            }
            else
            {
                int i = 0;
                // Look for an existing group matching the transaction's category.
                while (i < _categoryTradesList.Count)
                {
                    if (string.Equals(_categoryTradesList[i][0].category, trade.category, StringComparison.Ordinal))
                    {
                        _categoryTradesList[i].Add(trade);
                        break;
                    }
                    i++;
                }
                // If no matching group is found, create a new group.
                if (i == _categoryTradesList.Count)
                {
                    List<Transaction> newList = new List<Transaction> { trade };
                    _categoryTradesList.Add(newList);
                }
            }
        }
    }

    /// <summary>
    /// Filters transactions for a specific category and separates them into expenses and incomes.
    /// </summary>
    /// <param name="transactions">The list of transactions to filter.</param>
    /// <param name="category">The specific category to filter by.</param>
    private void GetOneCategoryTrades(List<Transaction> transactions, string category)
    {
        // Create two new lists: one for expenses and one for incomes.
        _categoryTradesList.Add(new List<Transaction>());
        _categoryTradesList.Add(new List<Transaction>());
        foreach (var trade in transactions)
        {
            if (string.Equals(category, trade.category, StringComparison.Ordinal))
            {
                if (string.Equals("expense", trade.type, StringComparison.Ordinal))
                    _categoryTradesList[0].Add(trade);
                else
                    _categoryTradesList[1].Add(trade);
            }
        }
    }

    /// <summary>
    /// Groups transactions by category or filters for one category based on the provided category parameter.
    /// </summary>
    /// <param name="transactions">The list of transactions to group.</param>
    /// <param name="category">The category filter; if "Toutes les catégories", all transactions are grouped by category.</param>
    private void GetCategoryTrade(List<Transaction> transactions, string category)
    {
        if (string.IsNullOrEmpty(category) || string.Equals(category, "Toutes les catégories", StringComparison.Ordinal))
            GetAllCategoryTrades(transactions);
        else
            GetOneCategoryTrades(transactions, category);
    }

    /// <summary>
    /// Calculates total expense and income amounts and computes subtotals for each group of transactions.
    /// </summary>
    private void CalculateAmounts()
    {
        _expenseAmount = 0f;
        _incomeAmount = 0f;
        _subtotals.Clear();
        // Iterate through each group of transactions.
        foreach (var categoryTrade in _categoryTradesList)
        {
            float subtotal = 0f;
            foreach (var trade in categoryTrade)
            {
                // Sum up expenses and incomes.
                if (string.Equals(trade.type, "expense", StringComparison.Ordinal))
                    _expenseAmount += trade.amount;
                else
                    _incomeAmount += trade.amount;
                subtotal += trade.amount;
            } 
            _subtotals.Add(subtotal);
        }
        _totalAmount = _expenseAmount + _incomeAmount;
    }

    /// <summary>
    /// Displays the graph slices based on the computed subtotals and the specified category filter.
    /// </summary>
    /// <param name="category">The category filter used to determine the slice colors.</param>
    private void DisplaySlices(string category)
    {
        // Calculate the amounts and subtotals.
        CalculateAmounts();
        float currentRotation = 0f;
        for (int i = 0; i < _subtotals.Count ; i++)
        {
            // Determine the fraction of the total for the current category.
            float fraction = _subtotals[i] / _totalAmount;
            float sliceAngle = fraction * 360f;
            // Instantiate a slice from the prefab.
            RectTransform sliceInstance = Instantiate(slicePrefab, graphArea.transform);
            _sliceObjects.Add(sliceInstance);
            sliceInstance.anchoredPosition = Vector2.zero;
            sliceInstance.localRotation = Quaternion.Euler(0, 0, -currentRotation);
            Image sliceImage = sliceInstance.GetComponent<Image>();
            if (sliceImage != null)
            {
                // Set the slice's fill amount and color.
                sliceImage.fillAmount = fraction;
                sliceImage.color = string.Equals(category, "Toutes les catégories", StringComparison.Ordinal) ? sliceColors[i % sliceColors.Count] : oneCategoryColors[i % oneCategoryColors.Count];
            }
            currentRotation += sliceAngle;
        }
    }

    /// <summary>
    /// Displays legend lines corresponding to the graph slices, including category names and percentages.
    /// </summary>
    /// <param name="category">The category filter used to determine the legend text and colors.</param>
    private void DisplayLegendLine(string category)
    {
        for (int i = 0; i < _subtotals.Count; i++)
        {
            float fraction = _subtotals[i] / _totalAmount;
            // Instantiate a new legend line.
            GameObject newLine = Instantiate(legendLine, gridLayout.transform);
            _legendLineObjects.Add(newLine);
            if (newLine != null)
            {
                LegendLineManager legendLineManager = newLine.GetComponent<LegendLineManager>();
                if (legendLineManager != null)
                {
                    
                    if (string.Equals(category, "Toutes les catégories", StringComparison.Ordinal))
                    {
                        // Display the category name and percentage for all categories.
                        string text = " " + _categoryTradesList[i][0].category + " " + (fraction * 100f).ToString("F0") + "%";
                        legendLineManager.SetColorAndCategoryName(sliceColors[i % sliceColors.Count], text);
                    }
                    else
                    {
                        // Display separate legend lines for expenses and incomes.
                        string text;
                        if (i == 0)
                            text = " Dépenses " + (fraction * 100f).ToString("F0") + "%";
                        else
                            text = " Revenus " + (fraction * 100f).ToString("F0") + "%";
                        legendLineManager.SetColorAndCategoryName(oneCategoryColors[i % oneCategoryColors.Count], text);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Displays the graph by resetting data, grouping transactions by category, and generating both slices and legend lines.
    /// </summary>
    /// <param name="transactions">The list of transactions to display.</param>
    /// <param name="category">The category filter to apply when grouping transactions.</param>
    private void Display(List<Transaction> transactions, string category)
    {
        ResetData();
        GetCategoryTrade(transactions, category);
        DisplaySlices(category);
        DisplayLegendLine(category);
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Displays the graph with the specified transactions and category filter.
    /// </summary>
    /// <param name="transactions">The list of transactions to display in the graph.</param>
    /// <param name="category">The category filter to apply (e.g., "Toutes les catégories" for all categories).</param>
    public void DisplayGraph(List<Transaction> transactions, string category)
    {
        Display(transactions, category);
    }

    /// <summary>
    /// Resets the graph panel by clearing all data and destroying instantiated objects.
    /// </summary>
    public void ResetPanel()
    {
        ResetData();
    }

    /// <summary>
    /// Gets the total expense amount computed from the transactions.
    /// </summary>
    /// <returns>The total expense amount as a float.</returns>
    public float GetExpenseAmount()
    {
        return _expenseAmount;
    }

    /// <summary>
    /// Gets the total income amount computed from the transactions.
    /// </summary>
    /// <returns>The total income amount as a float.</returns>
    public float GetIncomeAmount()
    {
        return _incomeAmount;
    }
}
