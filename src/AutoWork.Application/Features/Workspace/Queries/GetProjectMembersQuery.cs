using AutoWork.Application.DTOs.Workspace;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Queries;

public class GetProjectMembersQuery : IRequest<List<ProjectMemberDto>>
{
    public Guid ProjectId { get; set; }
}
