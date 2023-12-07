using System;

namespace Gve.Text;

/// <summary>
/// A tracked step in generating variant text.
/// </summary>
public record VarGenerationStep
{
    /// <summary>
    /// Gets the version tag.
    /// </summary>
    public string VersionTag { get; }

    /// <summary>
    /// Gets the source operation entry.
    /// </summary>
    public VarOperationEntry OperationEntry { get; }

    /// <summary>
    /// Gets the source operation.
    /// </summary>
    public VarOperation Operation { get; }

    /// <summary>
    /// Gets the operation's result context.
    /// </summary>
    public VarOperationContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarGenerationStep"/> class.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="entry">The entry.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">tag or entry or operation or
    /// context</exception>
    public VarGenerationStep(string tag, VarOperationEntry entry,
        VarOperation operation, VarOperationContext context)
    {
        VersionTag = tag ?? throw new ArgumentNullException(nameof(tag));
        OperationEntry = entry ?? throw new ArgumentNullException(nameof(entry));
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{VersionTag}] {Operation}: {Context}";
    }
}
