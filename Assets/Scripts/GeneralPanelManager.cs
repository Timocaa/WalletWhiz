using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the switching between different UI panels and provides functionality to open a selected panel or exit the application.
/// </summary>
public class GeneralPanelManager : MonoBehaviour
{
    /// <summary>
    /// Array of panel GameObjects that can be displayed. Only one panel is active at any time.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Array of panels to be managed. Only one panel will be active at a time.")]
    [SerializeField] private GameObject[] panels;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called on the first frame when the script is enabled.
    /// Initializes the panel display by opening the first panel.
    /// </summary>
    void Start()
    {
        // Open the first panel at startup.
        SwitchPanels(0);
    }

    /// <summary>
    /// Activates the panel at the specified index and deactivates all other panels.
    /// </summary>
    /// <param name="index">Index of the panel to activate.</param>
    private void SwitchPanels(int index)
    {
        // Deactivate all panels.
        ResetPanels();
        // Activate the panel at the specified index.
        panels[index].SetActive(true);
    }

    /// <summary>
    /// Deactivates all panels.
    /// </summary>
    private void ResetPanels()
    {
        // Loop through each panel and deactivate it.
        foreach (var currentPanel in panels)
            currentPanel.SetActive(false);
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Opens the panel corresponding to the given index.
    /// </summary>
    /// <param name="index">Index of the panel to open.</param>
    public void OpenSelectedPanel(int index)
    {
        // Clear the currently selected UI element.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Switch to the selected panel.
        SwitchPanels(index);
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void ExitButton()
    {
        // Clear the currently selected UI element.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Quit the application.
        Application.Quit();
    }
}
