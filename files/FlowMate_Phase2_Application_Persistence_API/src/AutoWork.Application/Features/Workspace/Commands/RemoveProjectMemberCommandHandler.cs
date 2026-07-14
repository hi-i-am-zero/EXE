using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public RemoveProjectMemberCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<ProjectMember> members,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _members = members;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveProjectMemberCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.ProjectId, userId, cancellationToken);

        var member = (await _members.FindAsync(
            m => m.ProjectId == command.ProjectId && m.UserId == command.MemberUserId, cancellationToken))
            .FirstOrDefault()
            ?? throw new NotFoundException("Thành viên không tồn tại trong workspace.");

        await _members.DeleteAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
