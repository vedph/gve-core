using Microsoft.AspNetCore.Components;
using Gve.Demo.Client.Services;

namespace Gve.Demo.Client.Components;

public partial class SnapshotPresetPicker
{
    [Parameter]
    public List<SnapshotPreset> Presets { get; set; }

    public SnapshotPreset? SelectedPreset { get; set; }

    [Parameter]
    public EventCallback<SnapshotPreset> OnPresetSelected { get; set; }

    public SnapshotPresetPicker()
    {
        Presets = SnapshotPresetLoader.Load();
    }

    public void PickPreset()
    {
        if (SelectedPreset != null)
            OnPresetSelected.InvokeAsync(SelectedPreset);
    }
}
