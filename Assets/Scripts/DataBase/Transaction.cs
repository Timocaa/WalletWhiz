using System;

/// <summary>
/// Represents a financial transaction record, including details such as amount, category, type, date,
/// and information about whether it is a recurring (rehearsals) transaction.
/// </summary>
[Serializable]
public class Transaction
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public int id;
    
    /// <summary>
    /// Gets or sets the monetary amount of the transaction.
    /// </summary>
    public float amount;
    
    /// <summary>
    /// Gets or sets the category of the transaction (e.g., Food, Utilities, Salary).
    /// </summary>
    public string category;
    
    /// <summary>
    /// Gets or sets the type of the transaction, such as "expense" or "income".
    /// </summary>
    public string type;
    
    /// <summary>
    /// Gets or sets the date of the transaction as a formatted string.
    /// </summary>
    public string date;
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction is a recurring (rehearsals) transaction.
    /// </summary>
    public bool isRehearsals;
    
    /// <summary>
    /// Gets or sets the recurrence period for the transaction (e.g., "Day", "Week", "Month", or "Year").
    /// </summary>
    public string rehearsalsPeriod;
    
    /// <summary>
    /// Gets or sets the description of the transaction.
    /// </summary>
    public string describe;
}
