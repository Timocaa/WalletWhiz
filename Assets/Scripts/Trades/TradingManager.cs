using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the trading form for recording new transactions, including recurring transactions.
/// </summary>
public class TradingManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the DatabaseManager used for performing database operations.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Reference to the DatabaseManager used for performing database operations.")]
    [SerializeField] private DatabaseManager databaseManager;
    
    /// <summary>
    /// Reference to the CategoryListManager that handles loading and providing category data.
    /// </summary>
    [Tooltip("Reference to the CategoryListManager that handles category data.")]
    [SerializeField] private CategoryListManager categoryListManager;
    
    /// <summary>
    /// Dropdown component for selecting a transaction category.
    /// </summary>
    [Tooltip("Dropdown for selecting a transaction category.")]
    [SerializeField] private TMP_Dropdown categoryDropdown;
    
    /// <summary>
    /// Dropdown component for selecting the periodicity of a recurring transaction.
    /// </summary>
    [Tooltip("Dropdown for selecting the periodicity of a recurring transaction.")]
    [SerializeField] private TMP_Dropdown periodicityDropdown;
    
    /// <summary>
    /// Toggle to indicate if the transaction is an expense.
    /// </summary>
    [Tooltip("Toggle to indicate if the transaction is an expense.")]
    [SerializeField] private Toggle expenseToggle;
    
    /// <summary>
    /// Toggle to indicate if the transaction is an income.
    /// </summary>
    [Tooltip("Toggle to indicate if the transaction is an income.")]
    [SerializeField] private Toggle incomeToggle;
    
    /// <summary>
    /// Input field for entering the transaction amount.
    /// </summary>
    [Tooltip("Input field for entering the transaction amount.")]
    [SerializeField] private TMP_InputField amountText;
    
    /// <summary>
    /// Input field for entering the transaction description.
    /// </summary>
    [Tooltip("Input field for entering the transaction description.")]
    [SerializeField] private TMP_InputField describeText;
    
    /// <summary>
    /// Text element displaying the transaction date.
    /// </summary>
    [Tooltip("Text element displaying the transaction date.")]
    [SerializeField] private TextMeshProUGUI dateText;
    
    /// <summary>
    /// Panel that displays the list of recorded transactions.
    /// </summary>
    [Tooltip("Panel that displays the list of recorded transactions.")]
    [SerializeField] private GameObject tradingListPanel;
    
    /// <summary>
    /// The CultureInfo instance for French culture, used for date formatting.
    /// </summary>
    private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//
    
    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Resets the trading form panel.
    /// </summary>
    private void OnEnable()
    {
        ResetPanel();
    }

    /// <summary>
    /// Called when the object becomes disabled or inactive.
    /// Resets the trading form panel.
    /// </summary>
    private void OnDisable()
    {
        ResetPanel();
    }

    /// <summary>
    /// Initializes the category dropdown list by loading available categories.
    /// </summary>
    private void InitCategoryList()
    {
        // Clear existing options.
        categoryDropdown.options.Clear();
        // If categories are not loaded, load them.
        if (categoryListManager.Categories == null || categoryListManager.Categories.Count == 0)
            categoryListManager.LoadCategories();
        // Populate the dropdown with the loaded categories.
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
    /// Resets the trading form panel to its default state.
    /// </summary>
    private void ResetPanel()
    {
        // Initialize the category list in the dropdown.
        InitCategoryList();
        // Reset dropdown selections.
        categoryDropdown.value = 0;
        periodicityDropdown.value = 0;
        // Clear text inputs.
        amountText.text = "";
        describeText.text = "";
        // Set the date to the current date formatted in French.
        dateText.text = DateTime.Now.ToString("dddd dd MMMM yyyy", FrenchCulture);
        // Reset toggles.
        expenseToggle.isOn = false;
        incomeToggle.isOn = false;
    }

    /// <summary>
    /// Checks whether the trading form is completely filled.
    /// Also adjusts the description text for recurring transactions if needed.
    /// </summary>
    /// <returns><c>true</c> if the form is fully filled; otherwise, <c>false</c>.</returns>
    private bool FormIsFull()
    {
        // Ensure the amount field is not empty.
        if (string.IsNullOrEmpty(amountText.text))
            return false;
        // Ensure that exactly one of the toggles is selected.
        if ((expenseToggle.isOn && incomeToggle.isOn) || (!incomeToggle.isOn && !expenseToggle.isOn))
            return false;
        // Ensure the description field is not empty.
        if (string.IsNullOrEmpty(describeText.text))
            return false;
        // If a periodicity is selected (i.e., recurring transaction), adjust the description to make it unique.
        if (periodicityDropdown.value > 0)
        {
            List<Transaction> transactions = databaseManager.GetTransactions();
            int occurence = 1;
            foreach (Transaction trade in transactions)
            {
                // Process only recurring transactions.
                if (trade.isRehearsals)
                {
                    // If the existing trade description matches the entered description exactly.
                    if (string.Equals(trade.describe, describeText.text, StringComparison.Ordinal))
                    {
                        // If the description already ends with the occurrence indicator, remove it.
                        if (describeText.text.EndsWith("#" + occurence))
                            describeText.text =
                                describeText.text.Remove(describeText.text.Length - ("#" + occurence).Length);
                        // Append the occurrence indicator.
                        describeText.text = describeText.text + "#" + occurence;
                        occurence++;
                    }
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the periodicity string based on the selected index.
    /// </summary>
    /// <param name="index">The index from the periodicity dropdown.</param>
    /// <returns>A string representing the periodicity ("Day", "Week", "Month", or "Year").</returns>
    private string GetPeriodicity(int index)
    {
        switch (index)
        {
            case 1:
                return "Day";
            case 2:
                return "Week";
            case 3:
                return "Month";
            case 4:
                return "Year";
        }
        return "";
    }

    /// <summary>
    /// Records the transaction using data from the form inputs.
    /// </summary>
    private void RecordTransaction()
    {
        // Attempt to parse the amount from the input field.
        if (float.TryParse(amountText.text, out float amount))
        {
            // Determine the transaction type based on the toggles.
            string type = expenseToggle.isOn ? "expense" : "income";
            // Determine if the transaction is recurring based on the periodicity dropdown selection.
            bool isRehearsals = periodicityDropdown.value > 0;
            // Record the transaction in the database.
            databaseManager.AddTransaction(
                amount,
                categoryDropdown.options[categoryDropdown.value].text,
                type,
                dateText.text,
                isRehearsals,
                GetPeriodicity(periodicityDropdown.value),
                describeText.text);
        }
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Cancels the current operation and resets the form to its default state.
    /// </summary>
    public void CancelButton()
    {
        ResetPanel();
    }

    /// <summary>
    /// Validates the form; if complete, records the transaction and resets the form.
    /// </summary>
    public void ValidateButton()
    {
        if (FormIsFull())
        {
            RecordTransaction();
            ResetPanel();
        }
    }

    /// <summary>
    /// Advances to the trading list panel by resetting the form, activating the list panel, and deactivating the current panel.
    /// </summary>
    public void NextButton()
    {
        ResetPanel();
        tradingListPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
