using System.Text.Json;
using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Api.Observability;
using Microsoft.AspNetCore.Mvc;

namespace Gauss.Identity.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private const string UnexpectedErrorCode = "Identity.UnexpectedError";
    private const string UnexpectedErrorDescription = "An unexpected error occurred.";

    private static readonly Action<ILogger, string, Exception> LogUnhandledException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(ExceptionHandlingMiddleware)),
            "Unhandled exception occurred. CorrelationId: {CorrelationId}");

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var correlationId = context.TraceIdentifier;

            LogUnhandledException(
                logger,
                correlationId,
                exception);

            var error = Error.Failure(
                UnexpectedErrorCode,
                UnexpectedErrorDescription);

            var problemDetails = ProblemDetailsMapper.ToProblemDetails(error);

            problemDetails.Extensions["correlationId"] = correlationId;

            await WriteProblemDetailsAsync(
                context,
                problemDetails);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        ProblemDetails problemDetails)
    {
        context.Response.Clear();

        context.Response.StatusCode = problemDetails.Status ??
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails,
            cancellationToken: context.RequestAborted);
    }
}
