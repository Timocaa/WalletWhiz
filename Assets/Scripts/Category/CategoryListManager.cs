using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages the list of categories by loading, saving, adding, removing, and updating categories,
/// and handles their JSON serialization.
/// </summary>
public class CategoryListManager : MonoBehaviour
{
    /// <summary>
    /// The name of the JSON file used for storing the categories.
    /// </summary>
    [Header("Attributes:")]
    [Tooltip("Name of the JSON file used for storing categories.")]
    [SerializeField] private string categoryDbName = "/categories.json";
    
    /// <summary>
    /// The full file path of the category JSON file.
    /// </summary>
    private string _categoryDbPath;
    
    /// <summary>
    /// Gets or sets the list of categories.
    /// </summary>
    public List<string> Categories;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the category file path and loads the category list.
    /// </summary>
    private void Awake()
    {
        _categoryDbPath = Application.persistentDataPath + "/" + categoryDbName;
        LoadCategoriesList();
    }

    /// <summary>
    /// Loads the category list from the JSON file if it exists; otherwise, initializes a default list.
    /// </summary>
    private void LoadCategoriesList()
    {
        if (File.Exists(_categoryDbPath))
        {
            try
            {
                // Read JSON from the file and deserialize it.
                string json = File.ReadAllText(_categoryDbPath);
                CategoryData data = JsonUtility.FromJson<CategoryData>(json);
                if (data != null && data.categories != null)
                    Categories = data.categories;
                else
                    Categories = new List<string>();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                // In case of error, initialize an empty list.
                Categories = new List<string>();
            }
        }
        else
        {
            // Initialize a default list of categories.
            Categories = new List<string>()
            {
                "Alimentation",
                "Transports",
                "Loisirs",
                "Logement"
            };
            RecordCategories();
        }
    }

    /// <summary>
    /// Serializes and writes the category list to the JSON file.
    /// </summary>
    private void RecordCategories()
    {
        try
        {
            CategoryData data = new CategoryData();
            data.categories = Categories;
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_categoryDbPath, json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// Inserts a new category into the list if it does not already exist, then saves the list.
    /// </summary>
    /// <param name="newCategory">The new category to insert.</param>
    private void InsertCategory(string newCategory)
    {
        if (!Categories.Contains(newCategory))
        {
            Categories.Add(newCategory);
            RecordCategories();
        }
    }

    /// <summary>
    /// Deletes the specified category from the list if it exists, then saves the list.
    /// </summary>
    /// <param name="category">The category to delete.</param>
    private void DeleteCategory(string category)
    {
        if (Categories.Contains(category))
        {
            Categories.Remove(category);
            RecordCategories();
        }
    }

    /// <summary>
    /// Updates an existing category with a new name and saves the list.
    /// </summary>
    /// <param name="oldCategory">The category name to update.</param>
    /// <param name="newCategory">The new category name.</param>
    private void UpdateList(string oldCategory, string newCategory)
    {
        int index = Categories.IndexOf(oldCategory);
        if (index >= 0)
        {
            Categories[index] = newCategory;
            RecordCategories();
        }
    }

//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Loads the categories from the JSON file.
    /// </summary>
    public void LoadCategories()
    {
        LoadCategoriesList();
    }
    
    /// <summary>
    /// Saves the current list of categories to the JSON file.
    /// </summary>
    public void SaveCategories()
    {
        RecordCategories();
    }
    
    /// <summary>
    /// Adds a new category to the list.
    /// </summary>
    /// <param name="newCategory">The new category to add.</param>
    public void AddCategory(string newCategory)
    {
        InsertCategory(newCategory);
    }
    
    /// <summary>
    /// Removes a category from the list.
    /// </summary>
    /// <param name="category">The category to remove.</param>
    public void RemoveCategory(string category)
    {
        DeleteCategory(category);
    }
    
    /// <summary>
    /// Updates an existing category with a new name.
    /// </summary>
    /// <param name="oldCategory">The existing category name.</param>
    /// <param name="newCategory">The new category name.</param>
    public void UpdateCategory(string oldCategory, string newCategory)
    {
        UpdateList(oldCategory, newCategory);
    }
}
