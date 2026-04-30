using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.BuildingBlocks.Api.Responses;

public static class ProblemDetailsMapper
{
    public static ProblemDetails ToProblemDetails(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = GetStatusCode(error.Type);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Description,
            Type = GetTypeUri(statusCode)
        };

        problemDetails.Extensions["code"] = error.Code;

        return problemDetails;
    }

    public static int GetStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => "Validation Error",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Failure => "Server Error",
            _ => "Server Error"
        };
    }

    private static string GetTypeUri(int statusCode)
    {
        return $"https://httpstatuses.com/{statusCode}";
    }
}
