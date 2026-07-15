using AutoWork.Application.DTOs.Workspace;
using MediatR;

namespace AutoWork.Application.Features.Workspace.Queries;

/// <summary>Danh sách workspace mà user hiện tại sở hữu HOẶC là thành viên.</summary>
public class GetProjectsQuery : IRequest<List<ProjectDto>>
{
}
