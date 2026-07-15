using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Channels;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Channels.Commands;

public class CreateChannelAccountCommandHandler : IRequestHandler<CreateChannelAccountCommand, ChannelAccountDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<ChannelAccount> _channelAccounts;
    private readonly IRepository<Channel> _channels;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public CreateChannelAccountCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<ChannelAccount> channelAccounts,
        IRepository<Channel> channels,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _channelAccounts = channelAccounts;
        _channels = channels;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<ChannelAccountDto> Handle(CreateChannelAccountCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var channel = await _channels.GetByIdAsync(command.Request.ChannelId, cancellationToken)
            ?? throw new BadRequestException("Nền tảng không hợp lệ.");

        var account = new ChannelAccount
        {
            ProjectId = command.Request.ProjectId,
            ChannelId = command.Request.ChannelId,
            UserId = userId,
            Name = command.Request.Name.Trim(),
            ProfileUrl = command.Request.ProfileUrl?.Trim(),
            IsActive = true
            // AccessToken/RefreshToken để trống — sẽ điền khi làm OAuth thật (giai đoạn sau)
        };

        await _channelAccounts.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChannelAccountDto
        {
            Id = account.Id,
            ChannelId = channel.Id,
            ChannelCode = channel.Code,
            ChannelName = channel.Name,
            Name = account.Name,
            ProfileUrl = account.ProfileUrl,
            IsActive = account.IsActive,
            HasApiConnection = false
        };
    }
}
