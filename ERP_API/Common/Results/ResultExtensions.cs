using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Common.Results;


public static class ResultExtensions
{
    
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error switch
        {
            var error when error.Contains("NotFound") => new NotFoundObjectResult(new { error }),
            var error when error.Contains("already exists") => new ConflictObjectResult(new { error }),
            var error when error.Contains("Insufficient") => new ConflictObjectResult(new { error }),
            var error when error.Contains("Concurrency") => new ConflictObjectResult(new { error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }

    
    public static ActionResult<T> ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);

        return result.ToActionResult();
    }

    
    public static IActionResult ToNoContentResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.Error switch
        {
            var error when error.Contains("NotFound") => new NotFoundObjectResult(new { error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }
}