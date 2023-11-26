using System;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Add text to the right of the specified position.
/// </summary>
/// <remarks>Syntax: <c>AT+>"VALUE"^RANK@GROUP#TAG,TAG {NOTE}</c>.</remarks>
/// <seealso cref="VarOperation" />
public sealed class AddRightVarOperation : VarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRightVarOperation"/>
    /// class.
    /// </summary>
    public AddRightVarOperation() : base(VarOperationType.AddRight)
    {
        Run = 0;
    }

    /// <summary>
    /// Execute this operation on the specified text.
    /// </summary>
    /// <param name="text">Input annotated text.</param>
    /// <returns>
    /// Annotated text.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrEmpty(Value)) return text;

        StringBuilder sb = new(text.Value);
        if (Run > 0) sb.Remove(At, Run);
        sb.Insert(At, Value);

        return CreateResult(sb.ToString(), text.Annotations);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddRightVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddRightVarOperation op = new();
        // "value"
        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

        return op;
    }
}
