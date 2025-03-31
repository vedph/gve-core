using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Client.Components;

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
    [Parameter]
    public List<TaggedText> Versions { get; set; } = [];

    public string? NewVersion { get; set; }

    private string GetNextVersionTag()
    {
        int max = 0;
        foreach (TaggedText version in Versions)
        {
            if (int.TryParse(version.Tag[1..], out int n) && max < n)
                max = n;
        }
        return $"v{max + 1}";
    }

    public void AddVersion()
    {
        if (string.IsNullOrEmpty(NewVersion) ||
            (Versions.Count > 0 && Versions[^1].Text == NewVersion))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(NewVersion))
        {
            Versions = [
                .. Versions,
                new TaggedText { Tag = GetNextVersionTag(), Text = NewVersion }
            ];
        }
    }

    public void RemoveVersion(TaggedText version)
    {
        List<TaggedText> newVersions = [.. Versions];
        newVersions.Remove(version);
        Versions = newVersions;
    }

    private static List<TaggedText> AssignTags(List<TaggedText> versions)
    {
        int n = 1;
        List<TaggedText> newVersions = [];
        foreach (TaggedText version in versions)
        {
            newVersions.Add(new TaggedText { Tag = $"v{n++}", Text = version.Text });
        }
        return newVersions;
    }

    public void MoveVersionUp(TaggedText version)
    {
        int index = Versions.IndexOf(version);
        if (index > 0)
        {
            List<TaggedText> newVersions = [.. Versions];
            newVersions.RemoveAt(index);
            newVersions.Insert(index - 1, version);
            Versions = AssignTags(newVersions);
        }
    }

    public void MoveVersionDown(TaggedText version)
    {
        int index = Versions.IndexOf(version);
        if (index < Versions.Count - 1)
        {
            List<TaggedText> newVersions = [.. Versions];
            newVersions.RemoveAt(index);
            newVersions.Insert(index + 1, version);
            Versions = AssignTags(newVersions);
        }
    }
}
