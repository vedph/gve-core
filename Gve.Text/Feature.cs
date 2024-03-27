using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A generic name=value (or just name) feature.
/// </summary>
public partial record Feature
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value. Usually there is a value, but in some cases a feature
    /// is used just as a flag, in which case the value is null.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the feature is unique in a set
    /// i.e. if it already exists in a set, it should be replaced rather than
    /// just added.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this annotation is unique; otherwise, <c>false</c>.
    /// </value>
    public bool IsUnique { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    public Feature()
    {
        Name = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="isUnique"><c>true</c> if feature is unique.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Feature(string name, string? value, bool isUnique = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
        IsUnique = isUnique;
    }

    [GeneratedRegex(@"^(?<n>[^:=]+)(?<u>:)?(?:=(?<v>(?:""[^""]+"")|(?:[^\s]+)))?$")]
    private static partial Regex NameValueRegex();

    /// <summary>
    /// Parses the specified text representing a feature, with syntax
    /// <c>name=value or name="value"</c>, where <c>=</c> can be replaced by
    /// <c>:=</c> for a unique feature. When value is null, the text is
    /// just its name.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Feature.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Invalid feature</exception>
    public static Feature Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = NameValueRegex().Match(text);
        if (!m.Success) throw new FormatException($"Invalid feature: {text}");

        return new(m.Groups["n"].Value,
            m.Groups["v"].Success? m.Groups["v"].Value.Trim('"') : null,
            m.Groups["u"].Success);
    }

    /// <summary>
    /// Tries parsing the specified feature.
    /// </summary>
    /// <param name="text">The text representing the feature.</param>
    /// <param name="feature">The feature.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse(string text, out Feature? feature)
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
    /// feature. This assumes that global features have been preprocessed thus
    /// losing their <c>*</c> prefix. Callers will use this helper method against
    /// different targets according to whether the feature is global or not.
    /// </summary>
    /// <param name="source">The source feature.</param>
    /// <param name="targets">The target features.</param>
    /// <returns>The parsed name of <paramref name="source"/> when it is
    /// doomed, i.e. a short-lived feature which should be removed on the next
    /// update; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">source or targets</exception>
    public static string? UpdateFeature(Feature source, IList<Feature> targets)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targets);

        string name = source.Name;
        bool doomed = false;

        // if feature name ends with ^, it is a doomed (short-lived) feature
        if (source.Name.EndsWith('^'))
        {
            name = source.Name[..^1];
            doomed = true;
        }

        // if feature name ends with !, remove it from the features
        if (name.EndsWith('!'))
        {
            name = name[..^1];
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i].Name == name) targets.RemoveAt(i);
            }
        }
        else
        {
            // else if unique, first remove all the features with the same name
            if (source.IsUnique)
            {
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i].Name == name) targets.RemoveAt(i);
                }
            }
            targets.Add(source.Name != name
                ? new Feature(name, source.Value, source.IsUnique) : source);
        }

        return doomed ? name : null;
    }

    /// <summary>
    /// Updates the target features with the source features. Features whose
    /// name ends with <c>!</c> are removed from the target features; features
    /// marked as unique imply removing all the features with the same name
    /// before adding them.
    /// </summary>
    /// <param name="sources">The source features.</param>
    /// <param name="targets">The target features.</param>
    /// <returns>The list of doomed feature names from <paramref name="sources"/>,
    /// if any.</returns>
    /// <exception cref="ArgumentNullException">sources or targets</exception>
    public static HashSet<string> UpdateFeatures(IList<Feature> sources,
        IList<Feature> targets)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(targets);

        HashSet<string> doomed = [];

        foreach (Feature feature in sources)
        {
            string? n = UpdateFeature(feature, targets);
            if (n != null) doomed.Add(n);
        }

        return doomed;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        if (Value == null) return Name;

        string op = IsUnique ? ":=" : "=";
        return Value.Any(char.IsWhiteSpace)
            ? $"{Name}{op}\"{Value}\""
            : Name + op + Value;
    }
}
