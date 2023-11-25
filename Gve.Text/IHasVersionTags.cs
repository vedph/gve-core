using System.Collections.Generic;

namespace Gve.Text;

public interface IHasVersionTags
{
    IList<string> VersionTags { get; set; }
}
