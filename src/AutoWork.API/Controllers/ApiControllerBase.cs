using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse> OkResponse(string? message = null) =>
        Ok(ApiResponse.Ok(message));

    protected ActionResult<ApiResponse<T>> FailResponse<T>(string message) =>
        BadRequest(ApiResponse<T>.Fail(message));
}
