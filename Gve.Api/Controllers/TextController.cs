using Gve.Api.Models;
using Gve.Api.Services;
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

    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))
            .ToList();
    }

    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new(nodes.Select(n => n.Data).ToArray());

    /// <summary>
    /// Runs operations on the specified text.
    /// </summary>
    /// <param name="model">The model with text and operations.</param>
    /// <returns>Object with either <c>result</c> or <c>error</c>
    /// (an error message).</returns>
    [HttpPost("text/operations/run")]
    [Produces("application/json")]
    public ResultWrapperModel<CharChainResultModel>
        RunOperations([FromBody] TextOperationBindingModel model)
    {
        try
        {
            List<CharChainOperation> operations = model.Operations.Select(
                CharChainOperationMapper.Map).ToList();

            ChainOperationContext<char> context = new();
            context.Chain.AppendRange("", "v0", GetNodesFrom(model.Text));
            context.LastTag = "v0";

            foreach (CharChainOperation op in operations) op.Execute(context);

            CharChainResultModel result = new()
            {
                Steps = context.Steps,
                ChainDump = context.Chain.Dump(),
                ChainTags = context.Chain.GetDistinctTags(),
            };

            foreach (string tag in result.ChainTags)
            {
                IList<ChainNode<char>> nodes = context.Chain.GetTagNodes(tag);
                result.TaggedNodes[tag] = nodes.ToArray();

                result.TaggedTexts.Add(new Tuple<string, string>(tag,
                    GetVersionString(nodes)));
            }

            return new ResultWrapperModel<CharChainResultModel>(result);
        }
        catch (Exception e)
        {
            return new ResultWrapperModel<CharChainResultModel>(null, e.Message);
        }
    }
}
