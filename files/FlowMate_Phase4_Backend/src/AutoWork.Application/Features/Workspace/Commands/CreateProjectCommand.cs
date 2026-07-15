using AutoWork.Application.DTOs.Workspace;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public CreateProjectDto Request { get; set; } = null!;
}
