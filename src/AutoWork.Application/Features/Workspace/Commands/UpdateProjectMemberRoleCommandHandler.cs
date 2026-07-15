using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class UpdateProjectMemberRoleCommandHandler : IRequestHandler<UpdateProjectMemberRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public UpdateProjectMemberRoleCommandHandler(
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

    public async Task Handle(UpdateProjectMemberRoleCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        // Chỉ Owner thật sự mới được đổi role người khác (Admin không được tự nâng quyền cho nhau)
        var isOwner = await _accessGuard.IsOwnerAsync(command.Request.ProjectId, userId, cancellationToken);
        if (!isOwner)
        {
            throw new UnauthorizedException("Chỉ chủ sở hữu workspace mới có quyền đổi vai trò thành viên.");
        }

        var member = (await _members.FindAsync(
            m => m.ProjectId == command.Request.ProjectId && m.UserId == command.Request.UserId, cancellationToken))
            .FirstOrDefault()
            ?? throw new NotFoundException("Thành viên không tồn tại trong workspace.");

        member.Role = command.Request.Role;
        await _members.UpdateAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
