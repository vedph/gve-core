using System;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Base class for add operations.
/// </summary>
/// <seealso cref="VarOperation" />
public abstract class AddVarOperation : VarOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is add right
    /// or add left.
    /// </summary>
    protected bool IsRight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddVarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AddVarOperation(VarOperationType type) : base(type)
    {
        // not used for insertions
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
        sb.Insert(IsRight ? At : At - 1, Value);

        return CreateResult(sb.ToString(), text.Annotations);
    }
}
