using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Workspace;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Queries;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;
    private readonly ICurrentUserService _currentUser;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork, IRepository<ProjectMember> members, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _members = members;
        _currentUser = currentUser;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var owned = await _unitOfWork.Projects.FindAsync(p => p.UserId == userId, cancellationToken);
        var memberships = await _members.FindAsync(m => m.UserId == userId, cancellationToken);
        var memberProjectIds = memberships.Select(m => m.ProjectId).Distinct().ToList();

        var joined = memberProjectIds.Count > 0
            ? await _unitOfWork.Projects.FindAsync(p => memberProjectIds.Contains(p.Id) && p.UserId != userId, cancellationToken)
            : [];

        // Lấy số lượng member của TẤT CẢ project liên quan trong 1 query duy nhất (group trong bộ nhớ),
        // thay vì gọi FindAsync riêng cho từng project (N+1).
        var allProjectIds = owned.Select(p => p.Id).Concat(joined.Select(p => p.Id)).Distinct().ToList();
        var allMemberships = await _members.FindAsync(m => allProjectIds.Contains(m.ProjectId), cancellationToken);
        var memberCountByProject = allMemberships
            .GroupBy(m => m.ProjectId)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<ProjectDto>();
        foreach (var project in owned)
        {
            result.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsOwner = true,
                MemberCount = memberCountByProject.GetValueOrDefault(project.Id, 0) + 1, // +1 chủ sở hữu
                CreatedAt = project.CreatedAt
            });
        }

        foreach (var project in joined)
        {
            result.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsOwner = false,
                MemberCount = memberCountByProject.GetValueOrDefault(project.Id, 0) + 1,
                CreatedAt = project.CreatedAt
            });
        }

        return result.OrderByDescending(p => p.CreatedAt).ToList();
    }
}
