using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the display and manipulation of trading (recurring transaction) list elements in the UI.
/// </summary>
public class TradingListManager : MonoBehaviour
{
    /// <summary>
    /// The panel that displays the trading UI.
    /// </summary>
    [Header("Components:")]
    [Tooltip("The panel that displays the trading UI.")]
    [SerializeField] private GameObject tradingPanel;
    
    /// <summary>
    /// The grid panel that serves as a container for trading line elements.
    /// </summary>
    [Tooltip("The grid panel that serves as a container for trading line elements.")]
    [SerializeField] private GameObject gridPanel;
    
    /// <summary>
    /// The prefab for a single trading line element in the list.
    /// </summary>
    [Tooltip("The prefab for a single trading line element in the list.")]
    [SerializeField] private GameObject tradingLine;
    
    /// <summary>
    /// Reference to the DatabaseManager that handles database operations.
    /// </summary>
    [Tooltip("Reference to the DatabaseManager that handles database operations.")]
    [SerializeField] private DatabaseManager databaseManager;

    /// <summary>
    /// List of recurring transactions retrieved from the database.
    /// </summary>
    private List<Transaction> _rehearsalsTrades;
    
    /// <summary>
    /// List of instantiated GameObjects representing the recurring transaction lines in the UI.
    /// </summary>
    private List<GameObject> _rehearsalsList;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Initializes the trading list by hiding the trading panel, retrieving recurring trades, and displaying them.
    /// </summary>
    private void OnEnable()
    {
        // Disable the trading panel to ensure only the grid panel is shown.
        tradingPanel.SetActive(false);
        // Retrieve the list of recurring transactions from the database.
        _rehearsalsTrades = databaseManager.GetRehearsalsTrades();
        // Initialize the list for instantiated trading line GameObjects.
        _rehearsalsList = new List<GameObject>();
        // Populate the grid panel with trading line elements.
        DisplayList();
    }

    /// <summary>
    /// Called when the object becomes disabled or inactive.
    /// Clears the trading list data.
    /// </summary>
    private void OnDisable()
    {
        ResetData();
    }

    /// <summary>
    /// Instantiates UI elements for each recurring transaction and populates them with data.
    /// </summary>
    private void DisplayList()
    {
        // Loop through each recurring trade element.
        foreach (var element in _rehearsalsTrades)
        {
            // Instantiate a new trading line UI element under the grid panel.
            GameObject newLine = Instantiate(tradingLine, gridPanel.transform);
            // Add the newly created UI element to the rehearsals list.
            _rehearsalsList.Add(newLine);
            // Retrieve the TradingLineManager component to set the UI texts.
            TradingLineManager tradingLineManager = newLine.GetComponent<TradingLineManager>();
            if (tradingLineManager != null)
            {
                tradingLineManager.SetDescribeText(element.describe, element.id, element.type);
                tradingLineManager.SetPeriodicityText(element.rehearsalsPeriod);
                tradingLineManager.SetAmountText(element.amount);
                tradingLineManager.SetDateText(element.date);
            }
        }
    }

    /// <summary>
    /// Resets the trading list data by clearing stored lists and destroying instantiated UI elements.
    /// </summary>
    private void ResetData()
    {
        // Clear the list of recurring trades if it exists.
        _rehearsalsTrades?.Clear();
        // If there are instantiated UI elements, destroy each and clear the list.
        if (_rehearsalsList != null && _rehearsalsList.Count > 0)
        {
            foreach (var element in _rehearsalsList)
                Destroy(element);
            _rehearsalsList.Clear();
        }
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Handles the event when the previous button is pressed.
    /// Clears the current trading list, shows the trading panel, and hides this panel.
    /// </summary>
    public void PreviousButton()
    {
        // If the EventSystem is active, deselect any selected UI elements.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Clear all trading list data and UI elements.
        ResetData();
        // Show the trading panel.
        tradingPanel.SetActive(true);
        // Hide the current panel.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Deletes a single trading line element from the UI and the tracking list.
    /// </summary>
    /// <param name="element">The GameObject representing the trading line to delete.</param>
    public void DeleteLine(GameObject element)
    {
        // Remove the specified element from the list.
        _rehearsalsList.Remove(element);
        // Destroy the GameObject.
        Destroy(element);
    }
}
