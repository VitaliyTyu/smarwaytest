using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using SmartWay.WebApi.DTO;
using SmartWay.WebApi.Entities;

namespace SmartWay.WebApi.Interfaces;

public interface IFilesService
{
    Task UploadFiles(IList<IFormFile> files, string userId, CancellationToken cancellationToken);
    Task<List<FileModelDto>> GetAllFilesInfo(string userId, CancellationToken cancellationToken);
    Task<FileModelDto> GetFileById(Guid fileId, string currentUserId, CancellationToken cancellationToken);
    Task<string> GetFilesByGroupId(string currentUserId, Guid groupId, CancellationToken cancellationToken);
}