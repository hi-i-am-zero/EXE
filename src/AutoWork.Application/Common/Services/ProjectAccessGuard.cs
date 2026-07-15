using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;

namespace AutoWork.Application.Common.Services;

public class ProjectAccessGuard : IProjectAccessGuard
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ProjectMember> _members;

    public ProjectAccessGuard(IUnitOfWork unitOfWork, IRepository<ProjectMember> members)
    {
        _unitOfWork = unitOfWork;
        _members = members;
    }

    public async Task<Project> EnsureViewerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (project.UserId == userId)
        {
            return project;
        }

        var isMember = (await _members.FindAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken)).Count > 0;
        if (!isMember)
        {
            throw new UnauthorizedException("Bạn không có quyền truy cập workspace này.");
        }

        return project;
    }

    public async Task<Project> EnsureEditorAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await EnsureViewerAsync(projectId, userId, cancellationToken);
        if (project.UserId == userId)
        {
            return project;
        }

        var membership = (await _members.FindAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken))
            .FirstOrDefault();

        if (membership is null || membership.Role == "Member")
        {
            throw new UnauthorizedException("Chỉ Owner hoặc Admin của workspace mới có quyền chỉnh sửa.");
        }

        return project;
    }

    public async Task<bool> IsOwnerAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken);
        return project is not null && project.UserId == userId;
    }
}
