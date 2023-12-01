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
    /// Initializes a new instance of the <see cref="BaseVarOperation"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <exception cref="ArgumentNullException">set</exception>
    protected BaseVarOperation(VarOperationType type) : base(type)
    {
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations"/> if required.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="annotations">The annotations.</param>
    public abstract void Execute(CharNodeSet set, IList<Feature> annotations);

    /// <summary>
    /// Parses the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Parsing error</exception>
    public static T Parse<T>(string text) where T : BaseVarOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = LocOpRegex().Match(text);
        if (!m.Success)
            throw new FormatException($"Invalid operation: {text}");

        VarOperationType type = GetOperationType(m.Groups[1].Value);
        return type switch
        {
            VarOperationType.Replace
                => (ReplaceBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.AddLeft
                => (AddLeftBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.AddRight
                => (AddRightBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Annotate
                => (AnnotateBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Move
                => (MoveBaseVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Swap
                => (SwapBaseVarOperation.ParseOperation(text) as T)!,
            _ => throw new FormatException(
                $"Invalid operation type: {GetOperationType(text)}")
        };
    }

    /// <summary>
    /// Tries to parsing the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="operation">The parsed operation.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse<T>(string text, out T? operation)
        where T : BaseVarOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            operation = Parse<T>(text);
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
