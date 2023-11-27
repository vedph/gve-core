using System;

namespace Gve.Text;

/// <summary>
/// Add text to the right of the specified position.
/// </summary>
/// <remarks>Syntax: <c>AT+>"VALUE"^RANK@GROUP#TAG,TAG {NOTE}</c>.</remarks>
/// <seealso cref="VarOperation" />
public sealed class AddRightVarOperation : AddVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRightVarOperation"/>
    /// class.
    /// </summary>
    public AddRightVarOperation() : base(VarOperationType.AddRight)
    {
        IsRight = true;
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
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
