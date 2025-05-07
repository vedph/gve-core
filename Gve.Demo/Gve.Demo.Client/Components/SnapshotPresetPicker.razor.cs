using Microsoft.AspNetCore.Components;
using Gve.Demo.Client.Pages;
using Gve.Demo.Client.Services;

namespace Gve.Demo.Client.Components;

public partial class SnapshotPresetPicker
{
    [Parameter]
    public List<SnapshotPreset> Presets { get; set; } = [];

    public SnapshotPreset? SelectedPreset { get; set; }
}
