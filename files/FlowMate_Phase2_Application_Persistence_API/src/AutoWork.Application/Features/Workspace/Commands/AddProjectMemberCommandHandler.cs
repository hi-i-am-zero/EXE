using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Workspace;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Commands;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, ProjectMemberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public AddProjectMemberCommandHandler(
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

    public async Task<ProjectMemberDto> Handle(AddProjectMemberCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        // Chỉ Owner/Admin của workspace mới được thêm thành viên
        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var invitedUser = await _unitOfWork.Users.GetByEmailAsync(command.Request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy tài khoản với email này.");

        var existing = (await _members.FindAsync(
            m => m.ProjectId == command.Request.ProjectId && m.UserId == invitedUser.Id, cancellationToken))
            .FirstOrDefault();

        if (existing is not null)
        {
            throw new BadRequestException("Người dùng này đã là thành viên của workspace.");
        }

        var member = new ProjectMember
        {
            ProjectId = command.Request.ProjectId,
            UserId = invitedUser.Id,
            Role = command.Request.Role,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow // FlowMate v1: thêm trực tiếp, chưa có luồng "chấp nhận lời mời" — sẽ bổ sung nếu cần ở phase sau
        };

        await _members.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserId = invitedUser.Id,
            Email = invitedUser.Email,
            FullName = $"{invitedUser.FirstName} {invitedUser.LastName}".Trim(),
            AvatarUrl = invitedUser.AvatarUrl,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }
}
