using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class AddRightAnnOperation : AddAnnOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRightVarOperation"/>
    /// class.
    /// </summary>
    public AddRightAnnOperation() : base(VarOperationType.AddRight)
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
