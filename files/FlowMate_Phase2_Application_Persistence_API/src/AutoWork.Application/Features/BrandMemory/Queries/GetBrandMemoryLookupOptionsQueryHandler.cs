using AutoWork.Application.DTOs.BrandMemory;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Queries;

public class GetBrandMemoryLookupOptionsQueryHandler : IRequestHandler<GetBrandMemoryLookupOptionsQuery, BrandMemoryLookupOptionsDto>
{
    private readonly IRepository<BrandStyle> _styles;
    private readonly IRepository<ToneKeyword> _keywords;
    private readonly IRepository<CtaTemplate> _ctas;
    private readonly IRepository<Hashtag> _hashtags;
    private readonly IRepository<VoiceSampleTemplate> _voiceSamples;

    public GetBrandMemoryLookupOptionsQueryHandler(
        IRepository<BrandStyle> styles,
        IRepository<ToneKeyword> keywords,
        IRepository<CtaTemplate> ctas,
        IRepository<Hashtag> hashtags,
        IRepository<VoiceSampleTemplate> voiceSamples)
    {
        _styles = styles;
        _keywords = keywords;
        _ctas = ctas;
        _hashtags = hashtags;
        _voiceSamples = voiceSamples;
    }

    public async Task<BrandMemoryLookupOptionsDto> Handle(GetBrandMemoryLookupOptionsQuery request, CancellationToken cancellationToken)
    {
        var styles = await _styles.GetAllAsync(cancellationToken);
        var keywords = await _keywords.GetAllAsync(cancellationToken);
        var ctas = await _ctas.GetAllAsync(cancellationToken);
        var hashtags = await _hashtags.GetAllAsync(cancellationToken);
        var voiceSamples = await _voiceSamples.GetAllAsync(cancellationToken);

        return new BrandMemoryLookupOptionsDto
        {
            Styles = styles.Select(s => new LookupItemDto { Id = s.Id, Name = s.StyleName }).ToList(),
            ToneKeywords = keywords.Select(k => new LookupItemDto { Id = k.Id, Name = k.Keyword }).ToList(),
            CtaTemplates = ctas.Select(c => new LookupItemDto { Id = c.Id, Name = c.CtaText }).ToList(),
            Hashtags = hashtags.Select(h => new LookupItemDto { Id = h.Id, Name = h.Tag }).ToList(),
            VoiceSamples = voiceSamples.Select(v => new VoiceSampleDto { Id = v.Id, SampleText = v.SampleText }).ToList()
        };
    }
}
