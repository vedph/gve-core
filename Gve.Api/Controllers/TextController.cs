using Gve.Api.Models;
using Gve.Api.Services;
using Gve.Text;
using Microsoft.AspNetCore.Mvc;

namespace Gve.Api.Controllers;

/// <summary>
/// Text controller.
/// </summary>
/// <seealso cref="Controller" />
[ApiController]
public sealed class TextController : Controller
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

    private static string GetNodeFeatureKey(string tag, ChainNode<char> node) =>
        $"{tag}:{node.Id}";

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
                result.TaggedNodes[tag] = [];

                foreach (ChainNode<char> node in nodes)
                {
                    CharChainNodeModel m = new()
                    {
                        Id = node.Id,
                        Label = node.Label,
                        Data = node.Data,
                        SourceTag = node.SourceTag,
                    };
                    string key = GetNodeFeatureKey(tag, node);
                    if (context.NodeFeatures.TryGetValue(key,
                        out List<VarFeature>? value))
                    {
                        m.Features = value.ToArray();
                    }
                    result.TaggedNodes[tag].Add(m);
                }
            }

            return new ResultWrapperModel<CharChainResultModel>(result);
        }
        catch (Exception e)
        {
            return new ResultWrapperModel<CharChainResultModel>(null, e.Message);
        }
    }
}
