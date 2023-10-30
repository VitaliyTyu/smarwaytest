using System.IO.Compression;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using SmartWay.WebApi.Data.Entities;
using SmartWay.WebApi.Interfaces;
using SmartWay.WebApi.Models;

namespace SmartWay.WebApi.Services;

public class FilesService : IFilesService
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IAppCache _appCache;
    private const string FolderPath = "files";
    private const string CachePrefix = "uploadProgress_";

    public FilesService(IApplicationDbContext applicationDbContext, IAppCache appCache)
    {
        _applicationDbContext = applicationDbContext;
        _appCache = appCache;
    }

    public async Task UploadFiles(IList<IFormFile> files, string userId, CancellationToken cancellationToken)
    {
        var groupSizeInBytes = files.Sum(f => f.Length);
        var groupBytesRead = 0;
        var groupId = Guid.NewGuid();

        foreach (var file in files)
        {
            var fileSizeInBytes = file.Length;
            var fileBytesRead = 0;
            var fileName = file.FileName;

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), FolderPath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName);
            var fileId = Guid.NewGuid();

            var fileModel = new FileModel()
            {
                Id = fileId,
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
                groupBytesRead += read;
                fileBytesRead += read;

                var fileUploadPercentage = (int)((double)fileBytesRead / fileSizeInBytes * 100);
                var groupUploadPercentage = (int)((double)groupBytesRead / groupSizeInBytes * 100);

                var cacheKey = $"{CachePrefix}{userId}";

                _appCache.Add(cacheKey, new UploadProgress()
                {
                    LastFileId = fileId,
                    GroupId = groupId,
                    LastFileUploadProgress = fileUploadPercentage,
                    GroupUploadProgress = groupUploadPercentage,
                }, TimeSpan.FromMinutes(1));
            }

            await _applicationDbContext.Files.AddAsync(fileModel, cancellationToken);
        }

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UploadProgress> GetUploadProgress(string userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CachePrefix}{userId}";

        var uploadProgress = await _appCache.GetAsync<UploadProgress>(cacheKey);

        return uploadProgress;
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

        var zipArchivePath = GenerateZipArchive(files);

        return zipArchivePath;
    }


    public async Task SaveDownloadLink(DownloadLink downloadLink, CancellationToken cancellationToken)
    {
        await _applicationDbContext.DownloadLinks.AddAsync(downloadLink);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetFilesByLink(Guid linkId, CancellationToken cancellationToken)
    {
        var link = await _applicationDbContext.DownloadLinks
            .AsNoTracking()
            .Where(x => x.Id == linkId)
            .SingleOrDefaultAsync(cancellationToken);

        if (link == null)
            throw new ArgumentException("Link not found");

        var files = await _applicationDbContext.Files
            .AsNoTracking()
            .Where(f => f.Id == link.FileId || f.GroupId == link.GroupId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        var zipArchivePath = GenerateZipArchive(files);

        return zipArchivePath;
    }


    private string GenerateZipArchive(List<FileModel> files)
    {
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