using System.Collections.Concurrent;

namespace Gve.Demo.Client.Services;

public class RamStateService : IStateService
{
    private readonly ConcurrentDictionary<string, object> _storage = new();

    public Task<T?> GetAsync<T>(string key, T? defaultValue = default)
    {
        if (_storage.TryGetValue(key, out var value) && value is T typedValue)
        {
            return Task.FromResult(typedValue);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value)
    {
        _storage[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _storage.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string key)
    {
        return Task.FromResult(_storage.ContainsKey(key));
    }

    public Task ClearAsync()
    {
        _storage.Clear();
        return Task.CompletedTask;
    }
}
