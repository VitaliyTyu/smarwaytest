using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.DTO;
using SmartWay.WebApi.Entities;
using SmartWay.WebApi.Interfaces;

namespace SmartWay.WebApi.Services;

public class FilesService : IFilesService
{
    private readonly IApplicationDbContext _applicationDbContext;
    private const string FolderPath = "files";

    public FilesService(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task UploadFiles(IList<IFormFile> files, string userId, CancellationToken cancellationToken)
    {
        var totalSize = files.Sum(f => f.Length);
        var bytesRead = 0;
        var groupId = Guid.NewGuid();

        foreach (var file in files)
        {
            var fileName = file.FileName;

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), FolderPath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName);

            var fileModel = new FileModel()
            {
                Id = Guid.NewGuid(),
                Name = fileName,
                Path = filePath,
                UserId = userId,
                GroupId = groupId,
            };

            await using var stream = new FileStream(filePath, FileMode.Create);
            await using var readStream = file.OpenReadStream();

            int read;
            var buffer = new byte[8192];

            while ((read = await readStream.ReadAsync(buffer)) > 0)
            {
                await stream.WriteAsync(buffer, 0, read, cancellationToken: cancellationToken);
                bytesRead += read;

                var percentage = (int)((double)bytesRead / totalSize * 100);
            }

            _applicationDbContext.Files.Add(fileModel);
        }

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }


    public async Task<List<FileModelDto>> GetAllFilesInfo(string userId, CancellationToken cancellationToken)
    {
        var files = await _applicationDbContext.Files
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Name)
            .Select(f => new FileModelDto()
            {
                GroupId = f.GroupId,
                Name = f.Name,
                Path = f.Path,
                Id = f.Id,
            })
            .ToListAsync(cancellationToken);

        return files;
    }

    public async Task<FileModelDto> GetFileById(Guid fileId, string currentUserId, CancellationToken cancellationToken)
    {
        var file = await _applicationDbContext.Files
            .AsNoTracking()
            .Where(f => f.Id == fileId && f.UserId == currentUserId)
            .OrderBy(f => f.Name)
            .Select(f => new FileModelDto()
            {
                GroupId = f.GroupId,
                Name = f.Name,
                Path = f.Path,
                Id = f.Id,
            })
            .SingleOrDefaultAsync(cancellationToken);

        return file;
    }

    public async Task<string> GetFilesByGroupId(string currentUserId, Guid groupId, CancellationToken cancellationToken)
    {
        var files = await _applicationDbContext.Files
            .AsNoTracking()
            .Where(f => f.GroupId == groupId && f.UserId == currentUserId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        var zipArchiveName = $"{Guid.NewGuid().ToString()}.zip";
        var zipArchivePath = Path.Combine(FolderPath, zipArchiveName);

        using var archive = ZipFile.Open(zipArchivePath, ZipArchiveMode.Create);

        foreach (var file in files)
        {
            if (File.Exists(file.Path))
                archive.CreateEntryFromFile(file.Path, file.Name);
        }

        return zipArchivePath;
    }
}