using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class AddLeftAnnOperation : AddAnnOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLeftVarOperation"/>
    /// class.
    /// </summary>
    public AddLeftAnnOperation() : base(VarOperationType.AddLeft)
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
