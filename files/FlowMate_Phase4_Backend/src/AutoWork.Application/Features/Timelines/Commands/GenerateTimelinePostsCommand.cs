using AutoWork.Application.DTOs.AI;
using MediatR;

namespace AutoWork.Application.Features.Timelines.Commands;

public class GenerateTimelinePostsCommand : IRequest<GenerateTimelinePostsResponse>
{
    public GenerateTimelinePostsRequest Request { get; set; } = null!;
}
