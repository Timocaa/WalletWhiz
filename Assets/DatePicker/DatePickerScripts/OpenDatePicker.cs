using UnityEngine;

/// <summary>
/// The OpenDatePicker class provides functionality to open a date picker panel in a Unity UI.
/// When the panel is opened, it becomes visible and interactable for the user.
/// </summary>
public class OpenDatePicker : MonoBehaviour
{
    [Header("Components:")] 
    [SerializeField] private GameObject datePickerPanel;  // The UI panel that contains the date picker. This will be shown or hidden based on user interaction.
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                  Private Methods                                                   //
//--------------------------------------------------------------------------------------------------------------------//

    // No private methods are needed for this simple functionality.
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                   Public Methods                                                   //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Opens the date picker panel by making it active in the UI.
    /// This method is typically called when a user interacts with a button or another UI element.
    /// </summary>
    public void OpenPanel()
    {
        datePickerPanel.SetActive(true);
    }
}
