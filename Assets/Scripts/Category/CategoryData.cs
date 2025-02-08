using System;
using System.Collections.Generic;

/// <summary>
/// Container class for JSON serialization of categories.
/// </summary>
[Serializable]
public class CategoryData
{
    /// <summary>
    /// Gets or sets the list of category names.
    /// </summary>
    public List<string> categories;
}
