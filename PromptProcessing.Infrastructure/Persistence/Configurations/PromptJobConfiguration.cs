using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Infrastructure.Persistence.Configurations;

public class PromptJobConfiguration : IEntityTypeConfiguration<PromptJob>
{
    public void Configure(EntityTypeBuilder<PromptJob> builder)
    {
        builder.ToTable("PromptJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .HasMaxLength(10000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnType("text");

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}