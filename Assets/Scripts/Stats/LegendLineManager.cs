using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the display of a legend line by setting its color and associated category name.
/// </summary>
public class LegendLineManager : MonoBehaviour
{
    /// <summary>
    /// Image component that displays the color associated with the category.
    /// </summary>
    [Header("Components:")]
    [Tooltip("Image component that displays the color associated with the category.")]
    [SerializeField] private Image colorImage;
    
    /// <summary>
    /// Text component that displays the name of the category.
    /// </summary>
    [Tooltip("Text component that displays the name of the category.")]
    [SerializeField] private TextMeshProUGUI categoryName;
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                Private Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    
    
//--------------------------------------------------------------------------------------------------------------------//
//                                                 Public Methods                                                     //
//--------------------------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Sets the legend line's color and category name.
    /// </summary>
    /// <param name="newColor">The color to assign to the legend line.</param>
    /// <param name="newCategoryName">The category name to display.</param>
    public void SetColorAndCategoryName(Color newColor, string newCategoryName)
    {
        colorImage.color = newColor;
        categoryName.text = newCategoryName;
    }
}
