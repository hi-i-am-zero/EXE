using AutoWork.Application.DTOs.Channels;
using MediatR;

namespace AutoWork.Application.Features.Channels.Commands;

/// <summary>Liên kết "thủ công" 1 kênh (chưa có OAuth thật — AccessToken để trống,
/// sẽ điền khi làm API Facebook/Zalo thật ở giai đoạn sau).</summary>
public class CreateChannelAccountCommand : IRequest<ChannelAccountDto>
{
    public CreateChannelAccountDto Request { get; set; } = null!;
}
