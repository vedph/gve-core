using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Gve.Text.Base;

/// <summary>
/// Base for base text variant operations.
/// </summary>
/// <seealso cref="VarOperation" />
public abstract class BaseVarOperation : VarOperation
{
    /// <summary>
    /// Gets the set of nodes representing the base text.
    /// </summary>
    protected CharNodeSet Set { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseVarOperation"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="set">The nodes set to use, representing the base text.</param>
    /// <exception cref="ArgumentNullException">set</exception>
    protected BaseVarOperation(VarOperationType type, CharNodeSet set) : base(type)
    {
        Set = set ?? throw new ArgumentNullException(nameof(set));
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations"/> if required.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The annotations.</param>
    public abstract void Execute(IList<CharNode> nodes,
        IList<Feature> annotations);

    /// <summary>
    /// Parses the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="set">The nodes set to use, representing the base text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Parsing error</exception>
    public static T Parse<T>(string text, CharNodeSet set) where T : BaseVarOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = LocOpRegex().Match(text);
        if (!m.Success)
            throw new FormatException($"Invalid operation: {text}");

        VarOperationType type = GetOperationType(m.Groups[1].Value);
        if (type != VarOperationType.Annotate && set == null)
            throw new ArgumentNullException(nameof(set));

        return type switch
        {
            VarOperationType.Replace
                => (ReplaceBaseVarOperation.ParseOperation(text, set) as T)!,
            VarOperationType.AddLeft
                => (AddLeftBaseVarOperation.ParseOperation(text, set) as T)!,
            VarOperationType.AddRight
                => (AddRightBaseVarOperation.ParseOperation(text, set) as T)!,
            VarOperationType.Annotate
                => (AnnotateBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Move
                => (MoveBaseVarOperation.ParseOperation(text, set) as T)!,
            VarOperationType.Swap
                => (SwapBaseVarOperation.ParseOperation(text, set) as T)!,
            _ => throw new FormatException(
                $"Invalid operation type: {GetOperationType(text)}")
        };
    }

    /// <summary>
    /// Tries to parsing the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="set">The nodes set to use, representing the base text.</param>
    /// <param name="operation">The parsed operation.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse<T>(string text, CharNodeSet set, out T? operation)
        where T : BaseVarOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            operation = Parse<T>(text, set);
            return true;
        }
        catch (FormatException ex)
        {
            Debug.WriteLine(ex.ToString());
            operation = null;
            return false;
        }
    }
}
