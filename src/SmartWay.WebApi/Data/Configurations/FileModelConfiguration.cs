using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWay.WebApi.Data.Entities;

namespace SmartWay.WebApi.Data.Configurations;

public class FileModelConfiguration : IEntityTypeConfiguration<FileModel>
{
    public void Configure(EntityTypeBuilder<FileModel> builder)
    {
        builder
            .HasKey(f => f.Id);

        builder
            .Property(f => f.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(f => f.Path)
            .HasMaxLength(300)
            .IsRequired();

        builder
            .Property(f => f.UserId)
            .IsRequired();
        
        builder
            .Property(f => f.GroupId)
            .IsRequired();
    }
}