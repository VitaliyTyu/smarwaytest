using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.Data.Configurations;
using SmartWay.WebApi.Data.Entities;
using SmartWay.WebApi.Interfaces;

namespace SmartWay.WebApi.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<FileModel> Files { get; set; }

    public DbSet<DownloadLink> DownloadLinks { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.ApplyConfiguration(new FileModelConfiguration());
       modelBuilder.ApplyConfiguration(new DownloadLinkConfiguration());

       base.OnModelCreating(modelBuilder);
   }
}