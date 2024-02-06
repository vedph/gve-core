using Gve.Text;

namespace Gve.Api.Models;

public class VarOperationsResponse
{
    public string? Error { get; }
    public IDictionary<string, IList<CharNodeSet>>? Results { get; }
    public IList<VarGenerationStep>? Steps { get; set; }
    public IList<string>? LogEntries { get; set; }

    public VarOperationsResponse(string error)
    {
        Error = error;
    }

    public VarOperationsResponse(IDictionary<string, IList<CharNodeSet>> results)
    {
        Results = results ?? throw new ArgumentNullException(nameof(results));
    }
}
