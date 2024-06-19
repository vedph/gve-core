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
[Route("text")]
public sealed class TextController : ControllerBase
{
    /// <summary>
    /// Parses the operations in the specified text.
    /// </summary>
    /// <param name="model">The parsable text model.</param>
    /// <returns>Object with either <c>result</c> (a list of operations) or
    /// <c>error</c> (an error message).</returns>
    [HttpPost("operations/parse")]
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

    private static List<ChainNode<char>> GetNodesFrom(CharChainNodeModel[] models)
    {
        return models.Select(m => new ChainNode<char>(
                m.Data, m.Label, m.Id))
            .ToList();
    }

    private static string GetNodeFeatureKey(string tag, ChainNode<char> node) =>
        $"{tag}_{node.Id}";

    /// <summary>
    /// Runs operations on the specified text.
    /// </summary>
    /// <param name="model">The model with text and operations.</param>
    /// <returns>Object with either <c>result</c> or <c>error</c>
    /// (an error message).</returns>
    [HttpPost("operations/run")]
    [Produces("application/json")]
    public ResultWrapperModel<CharChainResultModel>
        RunOperations([FromBody] TextOperationBindingModel model)
    {
        try
        {
            // get operations
            List<CharChainOperation> operations = model.Operations.Select(
                CharChainOperationMapper.Map).ToList();

            // init chain with base text (v0)
            ChainOperationContext<char> context = new();
            context.Chain.Reset(GetNodesFrom(model.Text));
            context.LastTag = "v0";

            // execute all the operations in their order
            List<string> tags = [];
            foreach (CharChainOperation op in operations)
            {
                op.Execute(context);
                tags.Add(context.LastTag);
            }

            // build result
            CharChainResultModel result = new()
            {
                Steps = context.Steps,
                ChainDump = context.Chain.Dump(),
                ChainTags = context.Chain.GetDistinctTags(),
            };

            // for each version in build order (distinct ensures that we
            // have no duplicates, even if this is not expected here)
            tags = tags.Distinct().ToList();
            foreach (string tag in tags)
            {
                // get its nodes
                IList<ChainNode<char>> nodes = context.Chain.GetTagNodes(tag);
                result.TaggedNodes[tag] = [];

                // create their models with injected features from context
                int i = 0;
                foreach (ChainNode<char> node in nodes)
                {
                    CharChainNodeModel m = new()
                    {
                        Id = node.Id,
                        Index = i++,
                        Label = node.Label,
                        Data = node.Data,
                        SourceTag = node.SourceTag,
                        Features = context.GetNodeFeatures(
                            node.Id, tags[..(i + 1)])
                    };
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
