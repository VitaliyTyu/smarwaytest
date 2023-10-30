using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.Data.Entities;

namespace SmartWay.WebApi.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FileModel> Files { get; set; }
    DbSet<DownloadLink> DownloadLinks { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}