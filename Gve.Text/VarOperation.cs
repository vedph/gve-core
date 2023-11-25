using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A variant text editing operation.
/// </summary>
/// <seealso cref="IHasVersionTags" />
public class VarOperation : IHasVersionTags
{
    /// <summary>
    /// The symbols corresponding to <see cref="VarOperationType"/>'s.
    /// </summary>
    public static readonly string[] TYPE_SYMBOLS =
    [
        "=",
        "+<",
        "+>",
        ":",
        ">",
        "<>"
    ];

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public VarOperationType Type { get; set; }

    /// <summary>
    /// Gets or sets the ordinal character number to start at.
    /// </summary>
    public short At { get; set; }

    /// <summary>
    /// Gets or sets the run of characters to include from <see cref="At"/>.
    /// </summary>
    public short Run { get; set; }

    /// <summary>
    /// Gets the value argument for this operation. This is the value of
    /// the argument with the minimum rank, or null if there is no value
    /// argument.
    /// </summary>
    public string? Value
    {
        get
        {
            return Args.Where(a => a.Name == "value")
                .OrderBy(a => a.Rank).FirstOrDefault()?.Value;
        }
    }

    /// <summary>
    /// Gets the arguments.
    /// </summary>
    public IList<VarOperationArg> Args { get; set; }

    /// <summary>
    /// Gets or sets the optional group identifier. When an operation is part
    /// of a group, all the operation in it are considered as a unique virtual
    /// operation.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the version tags.
    /// </summary>
    public IList<string> VersionTags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperation"/> class.
    /// </summary>
    public VarOperation()
    {
        Args = new List<VarOperationArg>();
        VersionTags = new List<string>();
    }

    /// <summary>
    /// Converts to string. This has format
    /// <c>At×RunType:Arg1:Arg2:...@GroupId[VersionTags]{Note}</c>.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(At).Append('×').Append(Run).Append(TYPE_SYMBOLS[(int)Type]);

        if (Args.Count > 0) sb.Append(':').AppendJoin(":", Args);

        if (string.IsNullOrEmpty(GroupId))
            sb.Append('@').Append(GroupId);

        if (VersionTags.Count > 0)
            sb.Append('[').AppendJoin(" ", VersionTags).Append(']');

        if (!string.IsNullOrEmpty(Note))
            sb.Append(" {").Append(Note).Append('}');

        return sb.ToString();
    }

    /// <summary>
    /// Parses the specified text representing a <see cref="VarOperation"/>
    /// with format <c>At×RunType:Arg1:Arg2:...@GroupId[VersionTags]{Note}</c>.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid operation</exception>
    public static VarOperation Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        VarOperation result = new();
        Regex regex = new(
            @"^(\d+)×(\d+)([=+<>]{1,2})([^@]+)(?:@([^[]+))?"+
            @"(?:\[([^\]]+)\])?(?:\{([^\}]*)\})?$", RegexOptions.Compiled);
        Match match = regex.Match(text);
        if (match.Success) throw new FormatException($"Invalid operation: {text}");

        result.Type = match.Groups[3].Value switch
        {
            "=" => VarOperationType.Replace,
            "+<" => VarOperationType.AddLeft,
            "+>" => VarOperationType.AddRight,
            ":" => VarOperationType.Annotate,
            ">" => VarOperationType.Move,
            "<>" => VarOperationType.Swap,
            _ => VarOperationType.Replace
        };
        result.At = short.Parse(match.Groups[1].Value,
            CultureInfo.InvariantCulture);
        result.Run = short.Parse(match.Groups[2].Value,
            CultureInfo.InvariantCulture);
        result.Args = match.Groups[4].Value.Split(':')
            .Select(VarOperationArg.Parse).ToList();
        result.GroupId = match.Groups[5].Value;
        result.VersionTags = match.Groups[6].Value.Split(' ').ToList();
        result.Note = string.IsNullOrEmpty(match.Groups[7].Value)
            ? null : match.Groups[7].Value;
        return result;
    }

    /// <summary>
    /// Tries to parse the specified text representing a <see cref="VarOperation"/>.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="result">The result.</param>
    /// <returns>True if success.</returns>
    public static bool TryParse(string text, out VarOperation? result)
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            result = Parse(text);
            return true;
        }
        catch (FormatException)
        {
            result = null;
            return false;
        }
    }
}

/// <summary>
/// Type of <see cref="VarOperation"/>.
/// </summary>
public enum VarOperationType
{
    /// <summary>The replace operation.</summary>
    Replace = 0,
    /// <summary>The add left operation.</summary>
    AddLeft,
    /// <summary>The add right operation.</summary>
    AddRight,
    /// <summary>The annotate operation.</summary>
    Annotate,
    /// <summary>The move operation.</summary>
    Move,
    /// <summary>The swap operation.</summary>
    Swap
}
