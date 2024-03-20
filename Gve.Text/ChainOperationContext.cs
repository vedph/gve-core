using System.Collections.Generic;
using System.Globalization;

namespace Gve.Text;

/// <summary>
/// The data context for a chain operation.
/// </summary>
/// <typeparam name="T">The type of chain node data.</typeparam>
public class ChainOperationContext<T> : ChainOperationContextFeats
{
    /// <summary>
    /// Gets the chain. This is the target of all the operations for this
    /// context.
    /// </summary>
    public Chain<T> Chain { get; }

    /// <summary>
    /// Gets or sets the version tag of the last operation executed on this
    /// context.
    /// </summary>
    public string LastTag { get; set; }

    /// <summary>
    /// Gets the steps executed on the chain, each consisting of an operation
    /// and input and output version tags.
    /// </summary>
    public List<ChainOperationContextStep> Steps { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContext{T}"/>
    /// class.
    /// </summary>
    public ChainOperationContext()
    {
        Chain = new Chain<T>();
        LastTag = "";
        Steps = [];
    }

    /// <summary>
    /// Gets the maximum tag number among version tags in this context.
    /// </summary>
    /// <returns>Max number.</returns>
    public int GetMaxTagNumber()
    {
        int max = 0;
        foreach (string tag in Chain.GetDistinctTags())
        {
            // parse the tail of tag when ending with digits
            int i = tag.Length - 1;
            while (i >= 0 && char.IsDigit(tag[i]))
            {
                i--;
            }
            if (i < tag.Length - 1)
            {
                int n = int.Parse(tag[(i + 1)..], CultureInfo.InvariantCulture);
                if (n > max) max = n;
            }
        }

        return max;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"{Chain} ({Steps.Count})";
    }
}
