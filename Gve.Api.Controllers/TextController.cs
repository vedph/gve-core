using Gve.Api.Models;
using Gve.Text;
using Gve.Text.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Api.Controllers;

/// <summary>
/// Text controller.
/// </summary>
/// <seealso cref="Controller" />
[ApiController]
[Route("api/text")]
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
    public ResultWrapperModel<IList<CharChainOperationSource>>
        ParseOperations([FromBody] ParsableTextBindingModel model)
    {
        try
        {
            List<CharChainOperationSource> operations = [];
            foreach (string line in model.Text.Split('\n',
                StringSplitOptions.RemoveEmptyEntries))
            {
                CharChainOperation op = CharChainOperation
                    .Parse<CharChainOperation>(line);
                operations.Add(new CharChainOperationSource(op));
            }
            return new ResultWrapperModel<IList<CharChainOperationSource>>(
                operations);
        }
        catch (Exception e)
        {
            return new ResultWrapperModel<IList<CharChainOperationSource>>(
                null, e.Message);
        }
    }

    private static List<ChainNode<char>> GetNodesFrom(CharChainNode[] models)
    {
        return models.Select(m => new ChainNode<char>(
                m.Data, m.Label, m.Id))
            .ToList();
    }

    /// <summary>
    /// Gets the input and output tags of all the given operations.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>Array of objects having input and output tags.</returns>
    [HttpPost("operations/tags")]
    [Produces("application/json")]
    public ResultWrapperModel<OperationTagsModel[]> GetOperationTags
        ([FromBody] OperationsBindingModel model)
    {
        try
        {
            List<CharChainOperation> operations = model.Operations.Select(
                s => s.ToOperation()).ToList();

            ChainOperationContext<char> context = new();
            context.Chain.Reset(
            [
                new ChainNode<char>()
                {
                    Id = 1,
                    Data = 'A',
                    Label = "A",
                }
            ]);
            context.LastTag = "v0";

            List<OperationTagsModel> tags = [];
            foreach (CharChainOperation op in operations)
            {
                (string inputTag, string outputTag) = op.GetTags(context);
                tags.Add(new OperationTagsModel(inputTag, outputTag));

                // add a mock node with output tag as this is required
                // by next tag calculation
                context.Chain.Append(inputTag, outputTag, new ChainNode<char>
                {
                    Id = context.Chain.GetNextId(),
                });
                context.LastTag = outputTag;
            }

            return new ResultWrapperModel<OperationTagsModel[]>(tags.ToArray());
        }
        catch (Exception e)
        {
            return new ResultWrapperModel<OperationTagsModel[]>(null, e.Message);
        }
    }

    /// <summary>
    /// Runs operations on the specified text.
    /// </summary>
    /// <param name="model">The model with text and operations.</param>
    /// <returns>Object with either <c>result</c> or <c>error</c>
    /// (an error message).</returns>
    [HttpPost("operations/run")]
    [Produces("application/json")]
    public ResultWrapperModel<CharChainResultModel>
        RunOperations([FromBody] TextOperationsBindingModel model)
    {
        try
        {
            // get operations
            List<CharChainOperation> operations = model.Operations.Select(
                s => s.ToOperation()).ToList();

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
                List<string> tagsUpToCurrent = tags[..(tags.IndexOf(tag) + 1)];
                int i = 0;
                foreach (ChainNode<char> node in nodes)
                {
                    CharChainNode m = new()
                    {
                        Id = node.Id,
                        Index = i++,
                        Label = node.Label,
                        Data = node.Data,
                        SourceTag = node.SourceTag,
                        Features = context.GetNodeFeatures(
                            node.Id, tagsUpToCurrent)
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
