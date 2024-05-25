using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A feature attached to a text variant generation operation. This adds
/// metadata to the feature to drive its application to the operation context.
/// </summary>
/// <seealso cref="Feature" />
public partial record OperationFeature : Feature
{
    /// <summary>
    /// Gets or sets a value indicating whether this feature is negated.
    /// A negated feature means that any existing feature with the same name
    /// should be removed.
    /// </summary>
    public bool IsNegated { get; init; }

    /// <summary>
    /// Gets a value indicating whether this feature globally refers to the
    /// operation context as a whole, rather than to its affected nodes.
    /// </summary>
    public bool IsGlobal { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is short-lived.
    /// When true, the feature is removed on the next update.
    /// </summary>
    public bool IsShortLived { get; init; }

    [GeneratedRegex(
        @"^(?<t>\!)?(?<g>\*)?(?<n>[^^:=]+)(?<s>\^)?(?:(?<p>[:=])?=(?<v>(?:""[^""]+"")|(?:[^\s]+)))?$")]
    private static partial Regex OpNameValueRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationFeature"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="setPolicy">The set policy.</param>
    /// <param name="isNegated">if set to <c>true</c> is negated.</param>
    /// <param name="isGlobal">if set to <c>true</c> is global.</param>
    /// <param name="isShortLived">if set to <c>true</c> is short-lived.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public OperationFeature(string name,
        string? value,
        FeatureSetPolicy setPolicy = FeatureSetPolicy.Multiple,
        bool isNegated = false,
        bool isGlobal = false,
        bool isShortLived = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
        SetPolicy = setPolicy;
        IsNegated = isNegated;
        IsGlobal = isGlobal;
        IsShortLived = isShortLived;
    }

    /// <summary>
    /// Parses the specified text representing a feature, with syntax
    /// <c>name=value or name="value"</c>, where <c>=</c> can be replaced by
    /// <c>:=</c> for a single feature or by <c>==</c> for a single-first
    /// feature. When value is null, the text just contains the feature name.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Feature.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Invalid feature</exception>
    public static OperationFeature Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = OpNameValueRegex().Match(text);
        if (!m.Success) throw new FormatException($"Invalid feature: {text}");

        return new OperationFeature(
            m.Groups["n"].Value,
            m.Groups["v"].Success ? m.Groups["v"].Value.Trim('"') : null,
            m.Groups["p"].Value switch
            {
                ":" => FeatureSetPolicy.Single, // :=
                "=" => FeatureSetPolicy.SingleFirst, // ==
                _ => FeatureSetPolicy.Multiple  // =
            },
            m.Groups["t"].Success,
            m.Groups["g"].Success,
            m.Groups["s"].Success);
    }

    /// <summary>
    /// Tries parsing the specified feature.
    /// </summary>
    /// <param name="text">The text representing the feature.</param>
    /// <param name="feature">The feature.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse(string text, out OperationFeature? feature)
    {
        ArgumentNullException.ThrowIfNull(text);

        feature = null;
        try
        {
            feature = Parse(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Updates <paramref name="targets"/> features with <paramref name="source"/>
    /// feature. If <paramref name="source"/> is negated, the feature is removed
    /// from <paramref name="targets"/>; otherwise, it is added or updated,
    /// handling existing features with the same name according to the
    /// <paramref name="source"/> set policy.
    /// </summary>
    /// <param name="source">The source feature.</param>
    /// <param name="targets">The target features.</param>
    /// <param name="first">True if <paramref name="source"/> is the first
    /// feature being added with that name in the context of a features set
    /// update.</param>
    /// <returns>The parsed name of <paramref name="source"/> when it is
    /// doomed, i.e. a short-lived feature which should be removed on the next
    /// update; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">source or targets</exception>
    public static void UpdateFeature(OperationFeature source,
        IList<OperationFeature> targets, bool first)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targets);

        // if feature is negated, remove it from targets
        if (source.IsNegated)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i].Name == source.Name) targets.RemoveAt(i);
            }
        }
        // else add or update
        else
        {
            // if single or single and first, remove all the features
            // with the same name
            if (source.SetPolicy == FeatureSetPolicy.Single ||
                (first && source.SetPolicy == FeatureSetPolicy.SingleFirst))
            {
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i].Name == source.Name) targets.RemoveAt(i);
                }
            }
            targets.Add(source);
        }
    }

    /// <summary>
    /// Converts to a parsable string with format <c>!*NAME^=VALUE</c> where
    /// <c>!</c>=negated, <c>*</c>=global, and operator is <c>=</c> for
    /// multiple, <c>:=</c> for single, and <c>==</c> for single-first.
    /// Value is enclosed in double quotes if it contains white spaces.
    /// Suffix <c>^</c> is used for short-lived features.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        if (IsNegated) sb.Append('!');

        if (IsGlobal) sb.Append('*');
        sb.Append(Name);
        if (IsShortLived) sb.Append('^');

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
