using Gve.Api.Models;
using Gve.Text;
using Microsoft.AspNetCore.Mvc;

namespace Gve.Api.Controllers;

public class TextController : Controller
{
    //[HttpPost("text/operations")]
    //[Produces("application/json")]
    //public VarOperationsResponse RunOperationsText([FromBody]
    //    VarOperationsBindingModel model)
    //{
    //    try
    //    {
    //        RamLogger logger = new();
    //        VarGenerator generator = new(logger) { IsTrackingEnabled = true };
    //        generator.SetEntries(model.OperationsText);
    //        VarOperationsResponse response = new(generator.Generate(model.Text))
    //        {
    //            //Steps = generator.GetSteps(null),
    //            LogEntries = logger.GetEntries(),
    //        };
    //        return response;
    //    }
    //    catch (Exception e)
    //    {
    //        return new VarOperationsResponse(e.Message);
    //    }
    //}
}
