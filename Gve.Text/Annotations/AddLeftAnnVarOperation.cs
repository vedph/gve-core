using System;
using Gve.Text.Annotations;

namespace Gve.Text;

/// <summary>
/// Add left annotation operation.
/// Syntax: <c>AT+&lt;"VALUE"^RANK@GROUP [TAG ...] {NOTE}</c>.
/// </summary>
/// <seealso cref="AddAnnVarOperation" />
public sealed class AddLeftAnnVarOperation : AddAnnVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLeftAnnVarOperation"/>
    /// class.
    /// </summary>
    public AddLeftAnnVarOperation() : base(VarOperationType.AddBefore)
    {
        IsRight = false;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddLeftAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddLeftAnnVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
