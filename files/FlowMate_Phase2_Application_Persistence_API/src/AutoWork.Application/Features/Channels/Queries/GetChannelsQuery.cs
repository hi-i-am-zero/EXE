using AutoWork.Application.DTOs.Channels;
using MediatR;

namespace AutoWork.Application.Features.Channels.Queries;

/// <summary>Danh mục nền tảng hỗ trợ (Facebook, Instagram, TikTok, Zalo, Website...).</summary>
public class GetChannelsQuery : IRequest<List<ChannelDto>>
{
}
