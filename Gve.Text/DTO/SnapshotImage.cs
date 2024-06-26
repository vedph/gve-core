using System.Text;

namespace Gve.Text.DTO;

/// <summary>
/// A background image for a <see cref="Snapshot"/>.
/// </summary>
public class SnapshotImage
{
    /// <summary>
    /// Gets or sets the image source URL.
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Gets or sets the optional canvas to fit the image into. If not
    /// specified, the image will have the same size of the SVG container, and
    /// placed at its top-left corner (0,0).
    /// </summary>
    public Rectangle? Canvas { get; set; }

    /// <summary>
    /// Gets or sets the default background image opacity. If not specified
    /// this is equal to 0. So, to show the image you should specify an opacity
    /// value here, or set it later.
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Url);

        if (Canvas != null)
            sb.Append('@').Append(Canvas.ToString());

        if (Opacity > 0)
            sb.Append(" opacity=").Append(Opacity);

        return sb.ToString();
    }
}
