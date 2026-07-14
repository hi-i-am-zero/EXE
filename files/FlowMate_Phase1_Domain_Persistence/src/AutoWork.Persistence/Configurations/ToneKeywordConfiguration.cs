using AutoWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoWork.Persistence.Configurations;

public class ToneKeywordConfiguration : IEntityTypeConfiguration<ToneKeyword>
{
    public void Configure(EntityTypeBuilder<ToneKeyword> builder) =>
        builder.ConfigureSimpleLookup("ToneKeywords", k => k.Keyword, 50);
}
