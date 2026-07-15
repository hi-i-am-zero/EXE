using AutoWork.Application.DTOs.BrandMemory;
using MediatR;

namespace AutoWork.Application.Features.BrandMemory.Queries;

/// <summary>Danh mục lookup (Styles/ToneKeywords/CTA/Hashtag/VoiceSample) để render UI chọn lựa.</summary>
public class GetBrandMemoryLookupOptionsQuery : IRequest<BrandMemoryLookupOptionsDto>
{
}
