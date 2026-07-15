using AutoWork.Application.DTOs.Channels;
using MediatR;

namespace AutoWork.Application.Features.Channels.Queries;

public class GetChannelAccountsQuery : IRequest<List<ChannelAccountDto>>
{
    public Guid ProjectId { get; set; }
}
