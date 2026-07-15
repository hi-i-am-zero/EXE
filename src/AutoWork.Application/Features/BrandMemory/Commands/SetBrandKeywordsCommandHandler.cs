using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.Common.Helpers;
using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandKeywordsCommandHandler : IRequestHandler<SetBrandKeywordsCommand, List<LookupItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<BrandProfileKeyword> _joinRepo;
    private readonly IRepository<ToneKeyword> _keywords;
    private readonly IProjectAccessGuard _accessGuard;
    private readonly ICurrentUserService _currentUser;

    public SetBrandKeywordsCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<BrandProfileKeyword> joinRepo,
        IRepository<ToneKeyword> keywords,
        IProjectAccessGuard accessGuard,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _joinRepo = joinRepo;
        _keywords = keywords;
        _accessGuard = accessGuard;
        _currentUser = currentUser;
    }

    public async Task<List<LookupItemDto>> Handle(SetBrandKeywordsCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _accessGuard.EnsureEditorAsync(command.Request.ProjectId, userId, cancellationToken);

        var profile = await _unitOfWork.BrandProfiles.GetByProjectIdWithDetailsAsync(command.Request.ProjectId, cancellationToken)
            ?? throw new BadRequestException("Vui lòng lưu Thông tin doanh nghiệp trước khi chọn từ khóa.");

        await ManyToManySync.ReplaceSelectionAsync(
            _joinRepo,
            profile.BrandProfileKeywords.ToList(),
            command.Request.SelectedIds,
            keywordId => new BrandProfileKeyword { BrandProfileId = profile.Id, ToneKeywordId = keywordId },
            row => row.ToneKeywordId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var all = await _keywords.FindAsync(k => command.Request.SelectedIds.Contains(k.Id), cancellationToken);
        return all.Select(k => new LookupItemDto { Id = k.Id, Name = k.Keyword }).ToList();
    }
}
