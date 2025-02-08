using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the panel for adding or modifying a category.
/// Provides functionality to validate input, update existing categories, or record new categories.
/// </summary>
public class ChangeCategoryManager : MonoBehaviour
{
    /// <summary>
    /// Input field for entering or editing the category name.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Input field for entering or editing the category name.")]
    [SerializeField] private TMP_InputField categoryText;
    
    /// <summary>
    /// Reference to the CategoryManager that handles category updates and recordings.
    /// </summary>
    [Tooltip("Reference to the CategoryManager that handles category updates and recordings.")]
    [SerializeField] private CategoryManager categoryManager;
    
    /// <summary>
    /// Stores the current category being edited.
    /// </summary>
    private string _currentCategory;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Resets the panel data by clearing the current category and input field text.
    /// </summary>
    private void ResetData()
    {
        _currentCategory = null;
        categoryText.text = "";
    }

    /// <summary>
    /// Validates the category input.
    /// If no current category exists, records the new category.
    /// If the input differs from the current category, updates the existing category.
    /// In both cases, the panel is reset and deactivated.
    /// </summary>
    private void Validation()
    {
        // If there is no current category, record a new category.
        if (string.IsNullOrEmpty(_currentCategory))
        {
            categoryManager.RecordNewCategory(categoryText.text);
            ResetPanel();
            gameObject.SetActive(false);
        }
        // If the input category is different from the current category, update it.
        else if (string.CompareOrdinal(_currentCategory, categoryText.text) != 0)
        {
            categoryManager.UpdateCategory(_currentCategory, categoryText.text);
            ResetPanel();
            gameObject.SetActive(false);
        }
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Resets the panel to its default state by clearing all data.
    /// </summary>
    public void ResetPanel()
    {
        ResetData();
    }

    /// <summary>
    /// Pre-populates the input field with the provided category and stores it as the current category.
    /// </summary>
    /// <param name="category">The category to display in the panel.</param>
    public void SetCategory(string category)
    {
        categoryText.text = category;
        _currentCategory = category;
    }

    /// <summary>
    /// Handles the validate button click.
    /// Clears the current UI selection and validates the input.
    /// If the input is valid, either records a new category or updates an existing one.
    /// </summary>
    public void ValidateButton()
    {
        // Deselect the current UI element.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Do not proceed if the input field is empty.
        if (string.IsNullOrEmpty(categoryText.text))
            return;
        // Validate the category input.
        Validation();
    }

    /// <summary>
    /// Handles the cancel button click.
    /// Clears any entered data, deselects the UI, and deactivates the panel.
    /// </summary>
    public void CancelButton()
    {
        // Deselect the current UI element.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Reset the panel data.
        ResetData();
        // Deactivate the panel.
        gameObject.SetActive(false);
    }
}
