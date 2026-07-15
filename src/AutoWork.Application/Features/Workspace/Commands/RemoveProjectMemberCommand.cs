using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class RemoveProjectMemberCommand : IRequest
{
    public Guid ProjectId { get; set; }

    public Guid MemberUserId { get; set; }
}
