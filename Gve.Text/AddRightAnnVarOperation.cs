using System;

namespace Gve.Text;

/// <summary>
/// Add right annotation operation.
/// </summary>
/// <seealso cref="AddAnnVarOperation" />
public sealed class AddRightAnnVarOperation : AddAnnVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRightAnnVarOperation"/>
    /// class.
    /// </summary>
    public AddRightAnnVarOperation() : base(VarOperationType.AddRight)
    {
        IsRight = true;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddRightAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddRightAnnVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
