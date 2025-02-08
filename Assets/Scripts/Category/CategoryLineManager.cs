using TMPro;
using UnityEngine;

/// <summary>
/// Manages the display and interaction of a single category line in the UI.
/// </summary>
public class CategoryLineManager : MonoBehaviour
{
    /// <summary>
    /// Text component that displays the category name.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Text component that displays the category name.")]
    [SerializeField] private TextMeshProUGUI categoryText;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Opens the panel for modifying the selected category.
    /// </summary>
    private void OpenChangeCategoryPanel()
    {
        // Find the first instance of CategoryManager in the scene.
        CategoryManager categoryManager = FindFirstObjectByType<CategoryManager>();
        if (categoryManager != null)
            categoryManager.ModifyCategory(categoryText.text);  // Invoke the ModifyCategory method with the current category text.
    }

    /// <summary>
    /// Deletes the current category by informing the CategoryManager.
    /// </summary>
    private void DeleteCategory()
    {
        // Find the first instance of CategoryManager in the scene.
        CategoryManager categoryManager = FindFirstObjectByType<CategoryManager>();
        if (categoryManager != null)
            categoryManager.RemoveCategory(categoryText.text, gameObject);  // Invoke the RemoveCategory method with the current category text and this GameObject.
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called by the UI button to modify the category.
    /// </summary>
    public void ModifyButton()
    {
        OpenChangeCategoryPanel();
    }

    /// <summary>
    /// Called by the UI button to delete the category.
    /// </summary>
    public void DeleteButton()
    {
        DeleteCategory();
    }

    /// <summary>
    /// Sets the displayed category text.
    /// </summary>
    /// <param name="category">The category name to display.</param>
    public void SetCategory(string category)
    {
        categoryText.text = category;
    }
}
