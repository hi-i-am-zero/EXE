using AutoWork.Application.DTOs.Workspace;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class AddProjectMemberCommand : IRequest<ProjectMemberDto>
{
    public AddProjectMemberDto Request { get; set; } = null!;
}
