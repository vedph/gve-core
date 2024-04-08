namespace Gve.Api.Models;

/// <summary>
/// The result of some task, which can be either a result or an error message.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
public class ResultWrapperModel<T>
{
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// The result.
    /// </summary>
    public T? Result { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultWrapperModel{T}"/>
    /// class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="error">The error.</param>
    public ResultWrapperModel(T? result, string? error = null)
    {
        Result = result;
        Error = error;
    }
}
