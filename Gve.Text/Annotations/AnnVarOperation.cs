using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Gve.Text.Annotations;

/// <summary>
/// A variant generation operation on the annotations plane.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
[JsonDerivedType(typeof(ReplaceAnnVarOperation), nameof(VarOperationType.Replace))]
[JsonDerivedType(typeof(AddLeftAnnVarOperation), nameof(VarOperationType.AddLeft))]
[JsonDerivedType(typeof(AddRightAnnVarOperation), nameof(VarOperationType.AddRight))]
[JsonDerivedType(typeof(AnnotateAnnVarOperation), nameof(VarOperationType.Annotate))]
[JsonDerivedType(typeof(MoveAnnVarOperation), nameof(VarOperationType.Move))]
[JsonDerivedType(typeof(SwapAnnVarOperation), nameof(VarOperationType.Swap))]
public abstract class AnnVarOperation : VarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnnVarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AnnVarOperation(VarOperationType type) : base(type)
    {
    }

    /// <summary>
    /// Deeply clones the nodes into a new list.
    /// </summary>
    /// <param name="nodes">The nodes to clone.</param>
    /// <returns>A new list with cloned nodes.</returns>
    protected static List<CharNode> CloneNodes(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        List<CharNode> newNodes = new(nodes.Count);
        foreach (CharNode node in nodes) newNodes.Add(node.Clone());
        return newNodes;
    }

    /// <summary>
    /// Parses the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Parsing error</exception>
    public static T Parse<T>(string text) where T : AnnVarOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = LocOpRegex().Match(text);
        if (!m.Success)
            throw new FormatException($"Invalid operation: {text}");

        return GetOperationType(m.Groups[1].Value) switch
        {
            VarOperationType.Replace
                => (ReplaceAnnVarOperation.ParseOperation(text) as T)!,
            VarOperationType.AddLeft
                => (AddLeftAnnVarOperation.ParseOperation(text) as T)!,
            VarOperationType.AddRight
                => (AddRightAnnVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Annotate
                => (AnnotateAnnVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Move
                => (MoveAnnVarOperation.ParseOperation(text) as T)!,
            VarOperationType.Swap
                => (SwapAnnVarOperation.ParseOperation(text) as T)!,
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
        where T : AnnVarOperation
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
