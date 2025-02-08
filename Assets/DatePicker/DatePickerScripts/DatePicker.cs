using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// The DatePicker class provides functionality to display and select dates on a calendar interface.
/// It manages the display of months, years, and days, and allows the user to navigate between months and years.
/// The selected date is formatted and displayed using TextMeshProUGUI components.
/// </summary>
public class DatePicker : MonoBehaviour
{
    [Header("TextMeshPro to fill:")] 
    [Tooltip("UI element to display the selected date.")]
    [SerializeField] private TextMeshProUGUI dateArea; // UI element to display the selected date.
    
    [Header("Components:")] 
    [Tooltip("UI element to display the current month.")]
    [SerializeField] private TextMeshProUGUI monthText; // UI element to display the current month.
    [Tooltip("UI element to display the current year.")]
    [SerializeField] private TextMeshProUGUI yearText; // UI element to display the current year.
    [Tooltip("List of WeekLine objects representing the weeks in the calendar.")]
    [SerializeField] private List<WeekLine> lines; // List of WeekLine objects representing the weeks in the calendar.
    
    private DateTime _today; // Stores today's date.
    private DateTime _modifiedDate; // Stores the currently displayed date.
    private TextInfo _textInfo; // Stores text information for proper case formatting.
    private int _dayCounter; // Counter to track the current day being displayed.
    private DateTime _dateSelected; // Stores the date selected by the user.

//--------------------------------------------------------------------------------------------------------------------//
//                                                  Private Methods                                                   //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Start is called before the first frame update. Initializes the calendar with today's date and displays it.
    /// </summary>
    void Start()
    {
        _textInfo = new CultureInfo("fr-FR", false).TextInfo;   // Initialize text info for French culture.
        _today = DateTime.Today;   // Set _today to the current date.
        _modifiedDate = _today;   // Initialize _modifiedDate with today's date.
        DisplayCalendar(_today);   // Display the calendar for the current date.
    }

    /// <summary>
    /// Returns the name of the month in title case for a given date.
    /// </summary>
    /// <param name="date">The date for which the month name is needed.</param>
    /// <returns>The name of the month in title case.</returns>
    private string GetMonth(DateTime date)
    {
        string month = date.ToString("MMMM", new CultureInfo("fr-FR"));   // Get the month name in French.
        return (_textInfo.ToTitleCase(month));  // Return the month name in title case.
    }

    /// <summary>
    /// Displays the calendar for the specified date. It updates the month, year, and days of the month.
    /// </summary>
    /// <param name="currentDate">The date for which the calendar should be displayed.</param>
    private void DisplayCalendar(DateTime currentDate)
    {
        ResetLines();  // Reset the display lines before updating the calendar.
        monthText.text = GetMonth(currentDate);  // Set the month text.
        yearText.text = currentDate.ToString("yyyy");  // Set the year text.
        DateTime firstWeek = new DateTime(currentDate.Year, currentDate.Month, 1);  // Get the first day of the month.
        DayOfWeek firstDay = firstWeek.DayOfWeek;  // Get the day of the week for the first day of the month.
        int index = GetFirstDayIndex(firstDay);  // Get the index for the first day.
        DisplayDaysNumber(index, currentDate);  // Display the days in the calendar.
    }

    /// <summary>
    /// Returns the index corresponding to the day of the week. 
    /// This is used to position the first day of the month correctly in the calendar.
    /// </summary>
    /// <param name="firstDay">The day of the week for the first day of the month.</param>
    /// <returns>The index representing the position of the first day in the calendar week.</returns>
    private int GetFirstDayIndex(DayOfWeek firstDay)
    {
        switch (firstDay)
        {
            case DayOfWeek.Monday:
                return 1;
            case DayOfWeek.Tuesday:
                return 2;
            case DayOfWeek.Wednesday:
                return 3;
            case DayOfWeek.Thursday:
                return 4;
            case DayOfWeek.Friday:
                return 5;
            case DayOfWeek.Saturday:
                return 6;
        }
        return 0;  // Sunday (or any unhandled day) starts at index 0.
    }

    /// <summary>
    /// Displays the numbers of the days in the current month, starting from the correct index.
    /// Handles the visibility and text for each day cell in the calendar.
    /// </summary>
    /// <param name="indexOfFirstDay">The index at which the first day of the month should be placed.</param>
    /// <param name="currentDate">The current date context for the calendar.</param>
    private void DisplayDaysNumber(int indexOfFirstDay, DateTime currentDate)
    {
        _dayCounter = 1;  // Initialize the day counter to 1.
        bool isFirstLine = true;  // Flag to identify the first week line.
        // Loop through each week line in the calendar.
        foreach (var currentLine in lines)
        {
            // Loop through each day in the week line.
            for (int i = 0; i < 7; i++)
            {
                // If it's the first line and the index is before the start of the month, hide the day cell.
                if (isFirstLine && i < indexOfFirstDay)
                    currentLine.daysObject[i].SetActive(false);
                // If the current day is within the valid days of the month, set the day number.
                else if (_dayCounter <= DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
                    currentLine.daysText[i].text = _dayCounter++.ToString();
                // If the current day is beyond the valid days of the month, hide the day cell.
                else
                    currentLine.daysObject[i].SetActive(false);
            }
            isFirstLine = false;  // After the first line is processed, reset the flag.
        }
    }

    /// <summary>
    /// Resets the internal data and display to the initial state with today's date.
    /// </summary>
    private void ResetData()
    {
        ResetLines();  // Reset the display lines.
        _today = _modifiedDate = DateTime.Today;  // Reset the date to today's date.
        _dayCounter = 1;  // Reset the day counter.
    }

    /// <summary>
    /// Resets all the lines in the calendar. Makes all day cells active and clears their text.
    /// </summary>
    private void ResetLines()
    {
        // Loop through each week line.
        foreach (var currentLine in lines)
        {
            // Activate all day objects in the week line.
            foreach (var currentObject in currentLine.daysObject)
                currentObject.SetActive(true);
            // Clear the text for all day text objects in the week line.
            foreach (var currentText in currentLine.daysText)
                currentText.text = "";
        }
    }

    /// <summary>
    /// Fills the selected date into the date area. The selected date is formatted and displayed in the UI.
    /// </summary>
    /// <param name="dayText">The day that has been selected.</param>
    private void FillDateSelected(string dayText)
    {
        // Parse the selected day and create a DateTime object for the selected date.
        DateTime selected = new DateTime(_modifiedDate.Year, _modifiedDate.Month, int.Parse(dayText));
        // Format the selected day as a string.
        string dayFormated = selected.ToString("dddd", new CultureInfo("fr-FR"));
        // Display the selected date in the date area with proper formatting.
        dateArea.text = _textInfo.ToTitleCase(dayFormated) + " " + selected.Day + " " + GetMonth(selected) + " " + selected.Year;
        // Deactivate the DatePicker UI after selection.
        gameObject.SetActive(false);
    }
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                   Public Methods                                                   //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Displays the previous month in the calendar.
    /// </summary>
    public void PreviousMonth()
    {
        _modifiedDate = _modifiedDate.AddMonths(-1);  // Subtract one month from the current displayed date.
        DisplayCalendar(_modifiedDate);  // Refresh the calendar with the new date.
    }

    /// <summary>
    /// Displays the previous year in the calendar.
    /// </summary>
    public void PreviousYear()
    {
        _modifiedDate = _modifiedDate.AddYears(-1);  // Subtract one year from the current displayed date.
        DisplayCalendar(_modifiedDate);  // Refresh the calendar with the new date.
    }

    /// <summary>
    /// Displays the next month in the calendar.
    /// </summary>
    public void NextMonth()
    {
        _modifiedDate = _modifiedDate.AddMonths(1);  // Add one month to the current displayed date.
        DisplayCalendar(_modifiedDate);  // Refresh the calendar with the new date.
    }

    /// <summary>
    /// Displays the next year in the calendar.
    /// </summary>
    public void NextYear()
    {
        _modifiedDate = _modifiedDate.AddYears(1);  // Add one year to the current displayed date.
        DisplayCalendar(_modifiedDate);  // Refresh the calendar with the new date.
    }

    /// <summary>
    /// Resets the entire panel, reverting the calendar to the initial state with today's date.
    /// </summary>
    public void ResetPanel()
    {
        ResetData();
    }

    /// <summary>
    /// Selects the date from the calendar and displays it in the date area.
    /// </summary>
    /// <param name="dateText">The UI element containing the day that was selected.</param>
    public void SelectDate(TextMeshProUGUI dateText)
    {
        FillDateSelected(dateText.text);
    }

    /// <summary>
    /// Closes the current panel by hiding it and displaying the calendar for the current date.
    /// </summary>
    public void ClosePanel()
    {
        // Display the calendar view for today's date.
        DisplayCalendar(_today);
        // Deactivate the game object to hide the panel.
        gameObject.SetActive(false);
    }
}
