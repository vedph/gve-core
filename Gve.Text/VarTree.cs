using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A tree of text variant nodes, attached to a specific location of the
/// base text.
/// </summary>
public class VarTree
{
    /// <summary>
    /// Gets or sets the location of the tree on the base text.
    /// </summary>
    public string Location { get; set; } = "";

    /// <summary>
    /// Gets or sets the ordinal numbers specifying the order of application
    /// for the trees in a set. This is a dictionary where the key is the
    /// variant tag, and the value is the ordinal for that variant.
    /// </summary>
    public Dictionary<string, int> Ordinals { get; set; } = [];

    /// <summary>
    /// Gets or sets the root operation node for this variants tree.
    /// </summary>
    public VarNode Root { get; set; } = new();

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"[{Location}] {Root}";
    }
}
