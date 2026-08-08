using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Infrastructure;

/// <summary>
/// Translates gRPC failures from downstream services into RFC 7807 responses, so callers
/// receive an accurate status code instead of a blanket 500. Detail from the downstream
/// service is echoed only for client errors; server-side failures return a generic message
/// so internal diagnostics are not exposed.
/// </summary>
public sealed class RpcExceptionHandler : IExceptionHandler
{
    private readonly ILogger<RpcExceptionHandler> _logger;

    public RpcExceptionHandler(ILogger<RpcExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RpcException rpcException)
        {
            return false;
        }

        var (status, title) = Map(rpcException.StatusCode);

        _logger.LogError(
            rpcException,
            "Downstream gRPC call failed with {GrpcStatus} for {Method} {Path}",
            rpcException.StatusCode,
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Instance = httpContext.Request.Path,
            Detail = status < StatusCodes.Status500InternalServerError
                ? rpcException.Status.Detail
                : null
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int Status, string Title) Map(StatusCode statusCode) => statusCode switch
    {
        StatusCode.InvalidArgument or StatusCode.FailedPrecondition or StatusCode.OutOfRange =>
            (StatusCodes.Status400BadRequest, "The request was rejected by the upstream service."),
        StatusCode.Unauthenticated =>
            (StatusCodes.Status401Unauthorized, "Authentication is required."),
        StatusCode.PermissionDenied =>
            (StatusCodes.Status403Forbidden, "Access to the requested resource is denied."),
        StatusCode.NotFound =>
            (StatusCodes.Status404NotFound, "The requested resource was not found."),
        StatusCode.AlreadyExists or StatusCode.Aborted =>
            (StatusCodes.Status409Conflict, "The request conflicts with the current state of the resource."),
        StatusCode.ResourceExhausted =>
            (StatusCodes.Status429TooManyRequests, "The upstream service is rate limiting requests."),
        StatusCode.Unimplemented =>
            (StatusCodes.Status501NotImplemented, "The requested operation is not supported."),
        StatusCode.Unavailable =>
            (StatusCodes.Status503ServiceUnavailable, "The upstream service is unavailable."),
        StatusCode.DeadlineExceeded =>
            (StatusCodes.Status504GatewayTimeout, "The upstream service did not respond in time."),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}
