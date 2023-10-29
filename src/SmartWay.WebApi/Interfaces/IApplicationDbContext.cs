using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.Entities;

namespace SmartWay.WebApi.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FileModel> Files { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}