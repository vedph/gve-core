namespace Gve.Demo.Client.Services;

public interface IStateService
{
    /// <summary>
    /// Gets a value by key.
    /// </summary>
    /// <returns>Value or default if not found.</returns>
    Task<T?> GetAsync<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Stores a value with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    Task SetAsync<T>(string key, T value);

    /// <summary>
    /// Removes a value with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in storage.
    /// </summary>
    /// <param name="key">The key.</param>
    Task<bool> ContainsKeyAsync(string key);

    /// <summary>
    /// Clears all values.
    /// </summary>
    Task ClearAsync();
}
