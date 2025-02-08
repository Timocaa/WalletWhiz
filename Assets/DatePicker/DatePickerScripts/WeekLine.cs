using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The WeekLine class represents a single line of days in a calendar week.
/// Each line contains a list of day objects (buttons or other UI elements) and their associated text components.
/// </summary>
[System.Serializable]
public class WeekLine
{
    /// <summary>
    /// A list of GameObjects representing each day in the week.
    /// These objects might be buttons or other interactive elements in the calendar UI.
    /// </summary>
    public List<GameObject> daysObject;

    /// <summary>
    /// A list of TextMeshProUGUI components corresponding to each day in the week.
    /// These components display the numerical day or other relevant text for the day.
    /// </summary>
    public List<TextMeshProUGUI> daysText;
}
