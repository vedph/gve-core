using Gve.Api.Models;
using Gve.Text;
using Microsoft.AspNetCore.Mvc;

namespace Gve.Api.Controllers;

[ApiController]
public class TextController : Controller
{
    /// <summary>
    /// Parses the operations in the specified text.
    /// </summary>
    /// <param name="model">The parsable text model.</param>
    /// <returns>Object with either <c>result</c> (a list of operations) or
    /// <c>error</c> (an error message).</returns>
    [HttpPost("text/operations/parse")]
    [Produces("application/json")]
    public ResultWrapperModel<IList<CharChainOperationModel>>
        ParseOperations([FromBody] ParsableTextBindingModel model)
    {
        try
        {
            List<CharChainOperationModel> operations = [];
            foreach (string line in model.Text.Split('\n',
                StringSplitOptions.RemoveEmptyEntries))
            {
                CharChainOperation op = CharChainOperation
                    .Parse<CharChainOperation>(line);
                operations.Add(new CharChainOperationModel(op));
            }
            return new ResultWrapperModel<IList<CharChainOperationModel>>(
                operations);
        }
        catch (Exception e)
        {
            return new ResultWrapperModel<IList<CharChainOperationModel>>(
                null, e.Message);
        }
    }
}
