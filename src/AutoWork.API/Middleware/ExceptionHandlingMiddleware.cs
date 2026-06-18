using System.Net;
using System.Text.Json;
using AutoWork.Shared.Models;
using FluentValidation;

namespace AutoWork.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteResponse(context, ApiResponse.Fail("Validation failed", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteResponse(context, ApiResponse.Fail(ex.Message));
        }
        catch (Application.Common.Exceptions.BadRequestException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await WriteResponse(context, ApiResponse.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await WriteResponse(context, ApiResponse.Fail(ex.Message));
        }
        catch (Application.Common.Exceptions.UnauthorizedException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await WriteResponse(context, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteResponse(context, ApiResponse.Fail("An error occurred processing your request."));
        }
    }

    private static async Task WriteResponse(HttpContext context, ApiResponse response)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
