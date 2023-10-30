using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWay.WebApi.Data.Entities;

namespace SmartWay.WebApi.Data.Configurations;

public class DownloadLinkConfiguration : IEntityTypeConfiguration<DownloadLink>
{
    public void Configure(EntityTypeBuilder<DownloadLink> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Url)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .IsRequired();
    }
}