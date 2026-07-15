using AutoWork.Application.DTOs.Workspace;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class UpdateProjectMemberRoleCommand : IRequest
{
    public UpdateProjectMemberRoleDto Request { get; set; } = null!;
}
