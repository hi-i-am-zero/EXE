using AutoWork.Application.DTOs.Dashboard;
using MediatR;

namespace AutoWork.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
}
