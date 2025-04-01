using System;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// A generic name=value (or just named) feature.
/// </summary>
public record Feature : NamedValue
{
    /// <summary>
    /// The feature set policy.
    /// </summary>
    public FeatureSetPolicy SetPolicy { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="setPolicy">The set policy.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Feature(string name, string? value,
        FeatureSetPolicy setPolicy = FeatureSetPolicy.Multiple)
        : base(name, value)
    {
        SetPolicy = setPolicy;
    }

    /// <summary>
    /// Converts to a parsable string with format <c>NAME=VALUE</c> where
    /// operator is <c>=</c> for multiple, <c>:=</c> for single, and
    /// <c>==</c> for single-first.
    /// Value is enclosed in double quotes if it contains white spaces.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Name);

        if (Value != null)
        {
            if (SetPolicy == FeatureSetPolicy.Single) sb.Append(':');
            else if (SetPolicy == FeatureSetPolicy.SingleFirst) sb.Append('=');
            sb.Append('=');

            if (Value.Any(char.IsWhiteSpace))
                sb.Append('"').Append(Value).Append('"');
            else
                sb.Append(Value);
        }

        return sb.ToString();
    }
}

/// <summary>
/// Policy for adding a <see cref="Feature"/> to a set.
/// </summary>
public enum FeatureSetPolicy
{
    /// <summary>
    /// When added, the feature is just added to the set (<c>=</c>).
    /// </summary>
    Multiple = 0,

    /// <summary>
    /// When added and feature(s) already exists, they are replaced (<c>:=</c>).
    /// </summary>
    Single,

    /// <summary>
    /// When adding several features, and features with the same name already
    /// exist, the first one being added replaces all the existing ones, while
    /// the following ones to be added just get added as multiple (<c>==</c>).
    /// This has meaning only when adding several features at once. Otherwise,
    /// it just behaves as <see cref="Single"/>.
    /// </summary>
    SingleFirst,
}
