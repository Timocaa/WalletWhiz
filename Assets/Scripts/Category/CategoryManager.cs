using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the category UI, including displaying, adding, updating, and removing categories.
/// </summary>
public class CategoryManager : MonoBehaviour
{
    /// <summary>
    /// Panel that holds the list of category items.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Panel that holds the list of category items.")]
    [SerializeField] private GameObject gridPanel;
    
    /// <summary>
    /// Prefab for an individual category line.
    /// </summary>
    [Tooltip("Prefab for an individual category line.")]
    [SerializeField] private GameObject categoryLine;
    
    /// <summary>
    /// Panel used for adding or modifying a category.
    /// </summary>
    [Tooltip("Panel used for adding or modifying a category.")]
    [SerializeField] private GameObject changeCategoryPanel;
    
    /// <summary>
    /// Manager that handles loading and maintaining category data.
    /// </summary>
    [Tooltip("Manager that handles loading and maintaining category data.")]
    [SerializeField] private CategoryListManager categoryListManager;
    
    /// <summary>
    /// List that stores instantiated category line GameObjects.
    /// </summary>
    private List<GameObject> _categoryList;

//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the GameObject becomes enabled and active.
    /// Initializes the internal category list, sorts the category data, and displays the categories.
    /// </summary>
    private void OnEnable()
    {
        // Initialize the internal list if it is not already created.
        if (_categoryList == null)
            _categoryList = new List<GameObject>();
        // Sort the available categories.
        categoryListManager.Categories.Sort();
        // Display the sorted categories on the grid panel.
        DisplayCategories();
    }

    /// <summary>
    /// Called when the GameObject becomes disabled or inactive.
    /// Clears all displayed category items.
    /// </summary>
    private void OnDisable()
    {
        ResetData();
    }

    /// <summary>
    /// Instantiates and displays a category line for each category in the list.
    /// </summary>
    private void DisplayCategories()
    {
        // Loop through each category provided by the category list manager.
        foreach (var element in categoryListManager.Categories)
        {
            // Instantiate a new category line as a child of the grid panel.
            GameObject newLine = Instantiate(categoryLine, gridPanel.transform);
            // Proceed if the instantiation was successful.
            if (newLine != null)
            {
                // Add the new line to the internal list.
                _categoryList.Add(newLine);
                // Get the CategoryLineManager component to set up the line.
                CategoryLineManager categoryLineManager = newLine.GetComponent<CategoryLineManager>();
                // If the component exists, set the category text.
                if (categoryLineManager != null)
                    categoryLineManager.SetCategory(element);
            }
        }
    }

    /// <summary>
    /// Destroys all instantiated category line GameObjects and clears the internal list.
    /// </summary>
    private void ResetData()
    {
        // Destroy each instantiated category line.
        foreach (var element in _categoryList)
            Destroy(element);
        // Clear the list of category lines.
        _categoryList.Clear();
    }

    /// <summary>
    /// Reloads category data and updates the UI display.
    /// </summary>
    private void UpdateDisplay()
    {
        // Reload the categories from the manager.
        categoryListManager.LoadCategories();
        // Remove currently displayed category lines.
        ResetData();
        // Instantiate and display updated category lines.
        DisplayCategories();
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Opens the panel for adding a new category.
    /// Deselects any currently selected UI element.
    /// </summary>
    public void AddCategory()
    {
        // Clear the current UI selection.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        // Activate the panel for category changes (adding/modifying).
        changeCategoryPanel.SetActive(true);
    }

    /// <summary>
    /// Adds a new category to the list and updates the display.
    /// </summary>
    /// <param name="category">The new category to record.</param>
    public void RecordNewCategory(string category)
    {
        // Add the new category using the category list manager.
        categoryListManager.AddCategory(category);
        // Refresh the display with the updated category list.
        UpdateDisplay();
    }

    /// <summary>
    /// Updates an existing category with a new name and refreshes the display.
    /// </summary>
    /// <param name="oldCategory">The current name of the category to be updated.</param>
    /// <param name="newCategory">The new name for the category.</param>
    public void UpdateCategory(string oldCategory, string newCategory)
    {
        // Update the category in the category list manager.
        categoryListManager.UpdateCategory(oldCategory, newCategory);
        // Refresh the category display.
        UpdateDisplay();
    }

    /// <summary>
    /// Removes a specified category and updates the display.
    /// </summary>
    /// <param name="categoryName">The name of the category to remove.</param>
    /// <param name="category">The GameObject associated with the category (unused in this implementation).</param>
    public void RemoveCategory(string categoryName, GameObject category)
    {
        // Remove the category from the category list manager.
        categoryListManager.RemoveCategory(categoryName);
        // Refresh the display to reflect the removal.
        UpdateDisplay();
    }

    /// <summary>
    /// Opens the modification panel pre-populated with the existing category data.
    /// </summary>
    /// <param name="oldCategory">The category to modify.</param>
    public void ModifyCategory(string oldCategory)
    {
        // Activate the panel used for modifying categories.
        changeCategoryPanel.SetActive(true);
        // Get the ChangeCategoryManager component from the change category panel.
        ChangeCategoryManager changeCategoryManager = changeCategoryPanel.GetComponent<ChangeCategoryManager>();
        // If the ChangeCategoryManager component is found, populate it with the current category.
        if (changeCategoryPanel != null)
            changeCategoryManager.SetCategory(oldCategory);
    }
}
