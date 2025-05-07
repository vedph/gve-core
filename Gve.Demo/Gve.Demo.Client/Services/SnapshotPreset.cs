namespace Gve.Demo.Client.Services;

/// <summary>
/// Initializes a new instance of the <see cref="SnapshotPreset"/> class.
/// </summary>
/// <param name="name">The name.</param>
/// <exception cref="ArgumentNullException">name</exception>
public class SnapshotPreset(string name)
{
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Base text.
    /// </summary>
    public string BaseText { get; set; } = "";

    /// <summary>
    /// Operations text.
    /// </summary>
    public string OperationText { get; set; } = "";

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The string representation of the <see cref="SnapshotPreset"/> class.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return Name;
    }
}
