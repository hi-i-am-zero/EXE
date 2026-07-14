using AutoWork.Application.DTOs.Channels;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Channels.Queries;

public class GetChannelsQueryHandler : IRequestHandler<GetChannelsQuery, List<ChannelDto>>
{
    private readonly IRepository<Channel> _channels;

    public GetChannelsQueryHandler(IRepository<Channel> channels)
    {
        _channels = channels;
    }

    public async Task<List<ChannelDto>> Handle(GetChannelsQuery request, CancellationToken cancellationToken)
    {
        var channels = await _channels.FindAsync(c => c.IsActive, cancellationToken);
        return channels
            .OrderBy(c => c.SortOrder)
            .Select(c => new ChannelDto { Id = c.Id, Code = c.Code, Name = c.Name, IconUrl = c.IconUrl })
            .ToList();
    }
}
