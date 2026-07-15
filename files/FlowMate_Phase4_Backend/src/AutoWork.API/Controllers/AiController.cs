using AutoWork.Application.DTOs.AI;
using AutoWork.Application.Features.AI.Commands;
using AutoWork.Application.Features.AI.Queries;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class AiController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AiController(IMediator mediator, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<GenerateContentResponse>>> Generate([FromBody] GenerateContentRequest request) =>
        OkResponse(await _mediator.Send(new GenerateContentCommand { Request = request }));

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AiHistoryDto>>>> History(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAiHistoryQuery { PageNumber = page, PageSize = pageSize });
        return OkResponse(PaginatedResult<AiHistoryDto>.Create(result.Items, result.TotalCount, result.PageNumber, result.PageSize));
    }

    [HttpGet("prompts")]
    public async Task<ActionResult<ApiResponse<List<AiPromptDto>>>> GetPrompts()
    {
        var prompts = await _unitOfWork.Ai.GetPromptsByUserIdAsync(_currentUser.UserId!.Value);
        var dtos = prompts.Select(p => new AiPromptDto
        {
            Id = p.Id,
            Name = p.Name,
            Template = p.Template,
            Category = p.Category,
            IsSystem = p.IsSystem,
            ProjectId = p.ProjectId,
            CreatedAt = p.CreatedAt
        }).ToList();

        return OkResponse(dtos);
    }

    [HttpPost("prompts")]
    public async Task<ActionResult<ApiResponse<AiPromptDto>>> CreatePrompt([FromBody] AiPromptDto request)
    {
        var prompt = await _unitOfWork.Ai.AddPromptAsync(new AiPrompt
        {
            UserId = _currentUser.UserId,
            Name = request.Name,
            Template = request.Template,
            Category = request.Category,
            IsSystem = false
        });

        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new AiPromptDto
        {
            Id = prompt.Id,
            Name = prompt.Name,
            Template = prompt.Template,
            Category = prompt.Category,
            IsSystem = prompt.IsSystem,
            CreatedAt = prompt.CreatedAt
        }, "Prompt created.");
    }

    [HttpDelete("prompts/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeletePrompt(Guid id)
    {
        var prompt = await _unitOfWork.Ai.GetPromptByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Prompt", id);

        if (prompt.IsSystem)
            return BadRequest(ApiResponse.Fail("Cannot delete system prompt."));

        if (prompt.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        await _unitOfWork.Ai.DeletePromptAsync(prompt);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Prompt deleted.");
    }
}
