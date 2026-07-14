using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Workspace;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Queries;

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public GetProjectMembersQueryHandler(
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

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var project = await _accessGuard.EnsureViewerAsync(request.ProjectId, userId, cancellationToken);
        var members = await _members.FindAsync(m => m.ProjectId == request.ProjectId, cancellationToken);

        // Lấy toàn bộ User liên quan (owner + members) trong 1 query duy nhất thay vì N query riêng lẻ
        var userIds = members.Select(m => m.UserId).Append(project.UserId).Distinct().ToList();
        var users = (await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), cancellationToken))
            .ToDictionary(u => u.Id);

        var result = new List<ProjectMemberDto>();

        if (users.TryGetValue(project.UserId, out var owner))
        {
            result.Add(new ProjectMemberDto
            {
                ProjectId = project.Id,
                UserId = owner.Id,
                Email = owner.Email,
                FullName = $"{owner.FirstName} {owner.LastName}".Trim(),
                AvatarUrl = owner.AvatarUrl,
                Role = "Owner",
                JoinedAt = project.CreatedAt
            });
        }

        foreach (var member in members)
        {
            if (!users.TryGetValue(member.UserId, out var user)) continue;

            result.Add(new ProjectMemberDto
            {
                Id = member.Id,
                ProjectId = member.ProjectId,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                AvatarUrl = user.AvatarUrl,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            });
        }

        return result;
    }
}
