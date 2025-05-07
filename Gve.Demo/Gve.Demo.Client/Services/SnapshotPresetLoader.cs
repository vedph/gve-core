using System.Reflection;
using System.Text;

namespace Gve.Demo.Client.Services;

public static class SnapshotPresetLoader
{
    /// <summary>
    /// Loads the snapshot presets from the embedded resource file.
    /// </summary>
    /// <returns>Presets list.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<SnapshotPreset> Load()
    {
        using StreamReader streamReader = new(
            Assembly.GetAssembly(typeof(SnapshotPresetLoader))!
            .GetManifestResourceStream("Gve.Demo.Client.Assets.Presets.txt")!,
            Encoding.UTF8);

        HeaderedTextReader reader = new();
        List<SnapshotPreset> presets = [];
        SnapshotPreset? preset = null;

        foreach (HeaderedText text in reader.Read(streamReader))
        {
            switch (text.Header)
            {
                case "title":
                    if (preset != null) presets.Add(preset);
                    preset = new(text.Header);
                    break;
                case "base":
                    if (preset == null)
                        throw new InvalidOperationException("base without title");
                    preset.BaseText = text.Text;
                    break;
                case "operations":
                    if (preset == null)
                        throw new InvalidOperationException("operations without title");
                    preset.OperationText = text.Text;
                    break;
                case "dsc":
                    if (preset == null)
                        throw new InvalidOperationException("dsc without title");
                    preset.Description = text.Text;
                    break;
            }
        }
        if (preset != null) presets.Add(preset);

        return presets;
    }
}
