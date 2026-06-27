using Gve.Text;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Client.Components;

public partial class OpFeatureDictionaryView
{
    private string? _selectedTag;

    [Parameter]
    public Dictionary<string, List<OperationFeature>> Features { get; set; } = [];

    [Parameter]
    public Func<int, string>? NodeGetter { get; set; }

    public List<string> Tags { get; set; } = [];

    public string? SelectedTag
    {
        get => _selectedTag;
        set
        {
            if (_selectedTag != value)
            {
                _selectedTag = value;
                UpdateTagFeatures();
            }
        }
    }

    public Dictionary<string, List<OperationFeature>> TagFeatures { get; set; } = [];

    private void UpdateTagFeatures()
    {
        if (_selectedTag != null)
        {
            Dictionary<string, List<OperationFeature>> feats = [];
            string prefix = $"{_selectedTag}_";

            foreach (var kvp in Features
                .Where(p => p.Key.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(p => int.Parse(p.Key.AsSpan(p.Key.IndexOf('_') + 1))))
            {
                feats[kvp.Key] = kvp.Value;
            }
            TagFeatures = feats;
        }
        else
        {
            TagFeatures = [];
        }
    }

    protected override void OnParametersSet()
    {
        // tags
        _selectedTag = null;
        Tags = [.. Features.Keys
            .Select(s => s[..s.IndexOf('_')])
            .Distinct()
            .OrderBy(t => int.TryParse(t.AsSpan(1), out int n) ? n : int.MaxValue)
            .ThenBy(t => t)];

        // tag features, filtered by tag and sorted by node ID
        UpdateTagFeatures();
    }

    public void ClearFeatures()
    {
        SelectedTag = null;
        TagFeatures = [];
    }

    public string GetNodeLabel(string key)
    {
        int id = int.Parse(key.AsSpan(key.IndexOf('_') + 1));
        return NodeGetter?.Invoke(id) ?? "";
    }
}
