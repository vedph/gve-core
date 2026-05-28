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
    #region Constants for features    
    /// <summary>
    /// The staged version feature. Its value is the staged version feature tag,
    /// which is any human-friendly identifier conventionally used to design it.
    /// </summary>
    public const string F_VERSION = "version";

    /// <summary>
    /// The prefix assigned to all tracing features.
    /// </summary>
    public const string F_TRACE_PREFIX = "$";

    /// <summary>
    /// The name for the input tagged segment tracing feature. This is added
    /// to all the nodes belonging to an input segment, i.e. a sequence
    /// of contiguous nodes affected by an operation.
    /// <para>The value is <c>OPID TAGIN:TAGOUT NR</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag, <c>NR</c>=ordinal
    /// number of node in sequence starting from 1.
    /// For instance, <c>9499e8e0c6 v2:v3 1</c>=1st node of the segment for
    /// version tag v3, derived from version tag v2 by operation ID 9499e8e0c6.
    /// </para>
    /// </summary>
    public const string F_TRACE_SEG_IN = "$seg-in";

    /// <summary>
    /// The name for the output tagged segment tracing feature. This is added
    /// to all the nodes belonging to an output segment, i.e. a sequence
    /// of contiguous nodes affected by an operation.
    /// <para>The value is <c>OPID TAGIN:TAGOUT NR</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag, <c>NR</c>=ordinal
    /// number of node in sequence starting from 1.
    /// For instance, <c>9499e8e0c6 v2:v3 1</c>=1st node of the segment for
    /// version tag v3, derived from version tag v2 by operation ID 9499e8e0c6.
    /// </para>
    /// </summary>
    public const string F_TRACE_SEG_OUT = "$seg-out";

    /// <summary>
    /// The name for the input tagged second segment tracing feature. This
    /// is added to all the nodes belonging to the second input segment, i.e.
    /// a sequence of contiguous nodes affected by the swap operation.
    /// <para>The value is <c>OPID TAGIN:TAGOUT NR</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag, <c>NR</c>=ordinal
    /// number of node in sequence starting from 1.
    /// For instance, <c>9499e8e0c6 v2:v3 1</c>=1st node of the segment for
    /// version tag v3, derived from version tag v2 by operation ID 9499e8e0c6.
    /// </para>
    /// </summary>
    public const string F_TRACE_SEG2_IN = "$seg2-in";

    /// <summary>
    /// The name for the output tagged second segment tracing feature. This
    /// is added to all the nodes belonging to the second output segment,
    /// i.e. a sequence of contiguous nodes affected by the swap operation.
    /// <para>The value is <c>OPID TAGIN:TAGOUT NR</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag, <c>NR</c>=ordinal
    /// number of node in sequence starting from 1.
    /// For instance, <c>9499e8e0c6 v2:v3 1</c>=1st node of the segment for
    /// version tag v3, derived from version tag v2 by operation ID 9499e8e0c6.
    /// </para>
    /// </summary>
    public const string F_TRACE_SEG2_OUT = "$seg2-out";

    /// <summary>
    /// The name for the anchor tracing feature. This is added to the node
    /// used as the anchor for an add before/after or move before/after operation.
    /// <para>The value is <c>OPID TAGIN:TAGOUT</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag.
    /// For instance, <c>9499e8e0c6 v2:v3</c>=version tag v3, derived from
    /// version tag v2 by operation ID 9499e8e0c6.</para>
    /// </summary>
    public const string F_TRACE_ANCHOR = "$anchor";

    /// <summary>
    /// The name for the left anchor tracing feature. This is added to the node
    /// immediately before the first node of a deleted segment.
    /// <para>The value is <c>OPID TAGIN:TAGOUT IDLIST</c> where <c>OPID</c>
    /// =operation ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag,
    /// <c>IDLIST</c>=comma-delimited list of the IDs of the deleted nodes.
    /// For instance, <c>9499e8e0c6 v2:v3 2,3</c>=version tag v3, derived from
    /// version tag v2 by operation ID 9499e8e0c6, deleting nodes with IDs
    /// 2 and 3.</para>
    /// </summary>
    public const string F_TRACE_LEFT_ANCHOR = "$left-anchor";

    /// <summary>
    /// The name for the right anchor tracing feature. This is added to the node
    /// immediately after the last node of a deleted segment.
    /// <para>The value is <c>OPID TAGIN:TAGOUT IDLIST</c> where <c>OPID</c>
    /// =operation ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag,
    /// <c>IDLIST</c>=comma-delimited list of the IDs of the deleted nodes.
    /// For instance, <c>9499e8e0c6 v2:v3 2,3</c>=version tag v3, derived from
    /// version tag v2 by operation ID 9499e8e0c6, deleting nodes with IDs
    /// 2 and 3.</para>
    /// </summary>
    public const string F_TRACE_RIGHT_ANCHOR = "$right-anchor";

    /// <summary>
    /// The name for the deleted tracing feature. This is added to all the nodes
    /// removed by a delete operation.
    /// </summary>
    /// <para>The value is <c>OPID TAGIN:TAGOUT NR</c> where <c>OPID</c>=operation
    /// ID, <c>TAGIN</c>=input tag, <c>TAGOUT</c>=output tag, <c>NR</c>=ordinal
    /// number of node in sequence starting from 1.
    /// For instance, <c>9499e8e0c6 v2:v3 1</c>=1st node of the segment for
    /// version tag v3, derived from version tag v2 by operation ID 9499e8e0c6.
    /// </para>
    public const string F_TRACE_DELETED = "$del";
    #endregion

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
        @"^(?<t>\!)?(?<g>\*)?(?<n>[^^:=]+)(?<s>\^)?(?:(?<p>[:=])?=(?<v>(?:""(?:[^""\\]|\\.)*"")|(?:[^\s]+)))?$")]
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
        bool isShortLived = false) : base(name, value, setPolicy)
    {
        IsNegated = isNegated;
        IsGlobal = isGlobal;
        IsShortLived = isShortLived;
    }

    /// <summary>
    /// Determines whether this feature is a trace feature.
    /// </summary>
    /// <returns><c>true</c> if is trace feature; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTraceFeature() =>
        Name.StartsWith(F_TRACE_PREFIX, StringComparison.Ordinal);

    private static string ExtractFeatureValue(string raw) =>
        raw.Length > 0 && raw[0] == '"'
            ? UnescapeFeatureValue(raw[1..^1])
            : raw;

    private static string UnescapeFeatureValue(string value)
    {
        if (!value.Contains('\\')) return value;

        StringBuilder sb = new(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '\\' && i + 1 < value.Length)
            {
                char next = value[i + 1];
                if (next == '"' || next == '\\')
                {
                    sb.Append(next);
                    i++;
                }
                else
                {
                    sb.Append(value[i]);
                }
            }
            else
            {
                sb.Append(value[i]);
            }
        }
        return sb.ToString();
    }

    private static string EscapeFeatureValue(string value) =>
        value.Replace(@"\", @"\\").Replace("\"", "\\\"");

    /// <summary>
    /// Parses the specified text representing a feature, with syntax
    /// <c>name=value or name="value"</c>, where <c>=</c> can be replaced by
    /// <c>:=</c> for a single feature or by <c>==</c> for a single-first
    /// feature. When value is null, the text just contains the feature name.
    /// A quoted value may contain <c>\"</c> for a literal double-quote and
    /// <c>\\</c> for a literal backslash.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Feature.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Invalid feature</exception>
    public static new OperationFeature Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = OpNameValueRegex().Match(text);
        if (!m.Success) throw new FormatException($"Invalid feature: {text}");

        return new OperationFeature(
            m.Groups["n"].Value,
            m.Groups["v"].Success
                ? ExtractFeatureValue(m.Groups["v"].Value)
                : null,
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

            if (Value.Any(c => char.IsWhiteSpace(c) || c == '"'))
                sb.Append('"').Append(EscapeFeatureValue(Value)).Append('"');
            else
                sb.Append(Value);
        }

        return sb.ToString();
    }
}
