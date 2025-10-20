using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Common.Results;

public static class ResultExtensions
{

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {

            if (result.Value is null)
            {

                return new ObjectResult(new { error = "Success result with null value" })
                {
                    StatusCode = 500
                };
            }

            return new OkObjectResult(result.Value);
        }

        var errorMessage = result.Error ?? "Unknown error";

        return errorMessage switch
        {
            var error when error.Contains("NotFound") || error.Contains("not found")
                => new NotFoundObjectResult(new { error }),

            var error when error.Contains("already exists") || error.Contains("Conflict")
                => new ConflictObjectResult(new { error }),

            var error when error.Contains("Insufficient") || error.Contains("insuficiente")
                => new ConflictObjectResult(new { error }),

            var error when error.Contains("Concurrency")
                => new ConflictObjectResult(new { error }),

            _ => new BadRequestObjectResult(new { error = errorMessage })
        };
    }

    public static ActionResult<T> ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object? routeValues)
    {
        if (result.IsSuccess)
        {
           
            if (result.Value is null)
            {
                return new ObjectResult(new { error = "Success result with null value" })
                {
                    StatusCode = 500
                };
            }

            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);
        }

     
        return result.ToActionResult();
    }

   
    public static IActionResult ToNoContentResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        var errorMessage = result.Error ?? "Unknown error";

        return errorMessage switch
        {
            var error when error.Contains("NotFound") || error.Contains("not found")
                => new NotFoundObjectResult(new { error }),

            var error when error.Contains("already exists")
                => new ConflictObjectResult(new { error }),

            _ => new BadRequestObjectResult(new { error = errorMessage })
        };
    }
}