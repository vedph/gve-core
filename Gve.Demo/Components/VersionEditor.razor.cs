namespace Gve.Demo.Components;

public record TaggedText
{
    public required string Tag { get; set; }
    public required string Text { get; set; }

    public override string ToString()
    {
        return $"({Tag}) {Text}";
    }
}

public partial class VersionEditor
{
    private readonly List<TaggedText> _versions = [];

    public string? NewVersion { get; set; }
    public IQueryable<TaggedText> Versions => _versions.AsQueryable();

    private string GetNextVersionTag()
    {
        int max = 0;
        foreach (TaggedText version in Versions)
        {
            if (int.TryParse(version.Tag[1..], out var n) && max < n)
                max = n;
        }
        return $"v{max + 1}";
    }

    public void AddVersion()
    {
        if (string.IsNullOrEmpty(NewVersion) ||
            (_versions.Count > 0 && _versions[^1].Text == NewVersion))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(NewVersion))
        {
            _versions.Add(new TaggedText
            {
                Tag = GetNextVersionTag(), Text = NewVersion
            });
        }
    }

    public void RemoveVersion(TaggedText version)
    {
        _versions.Remove(version);
    }

    public void MoveVersionUp(TaggedText version)
    {
        int index = _versions.IndexOf(version);
        if (index > 0)
        {
            _versions.RemoveAt(index);
            _versions.Insert(index - 1, version);
        }
    }

    public void MoveVersionDown(TaggedText version)
    {
        int index = _versions.IndexOf(version);
        if (index < _versions.Count - 1)
        {
            _versions.RemoveAt(index);
            _versions.Insert(index + 1, version);
        }
    }
}
