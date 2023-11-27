using System;

namespace Gve.Text;

/// <summary>
/// Add text to the left of the specified position.
/// </summary>
/// <remarks>Syntax: <c>AT+&lt;"VALUE"^RANK@GROUP#TAG,TAG {NOTE}</c>.</remarks>
/// <seealso cref="VarOperation" />
public sealed class AddLeftVarOperation : AddVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLeftVarOperation"/>
    /// class.
    /// </summary>
    public AddLeftVarOperation() : base(VarOperationType.AddLeft)
    {
        IsRight = false;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddLeftVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddLeftVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
