namespace Gve.Api.Models;

/// <summary>
/// The result of some task, which can be either a result or an error message.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ResultWrapperModel{T}"/>
/// class.
/// </remarks>
/// <param name="result">The result.</param>
/// <param name="error">The error.</param>
public class ResultWrapperModel<T>(T? result, string? error = null)
{
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Error { get; init; } = error;

    /// <summary>
    /// The result.
    /// </summary>
    public T? Result { get; init; } = result;
}
