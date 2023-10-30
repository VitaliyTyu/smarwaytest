using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using SmartWay.WebApi.Data.Entities;
using SmartWay.WebApi.Models;
using SmartWay.WebApi.Services;

namespace SmartWay.WebApi.Interfaces;

public interface IFilesService
{
    Task UploadFiles(IList<IFormFile> files, string userId, CancellationToken cancellationToken);
    Task<List<FileModelDto>> GetAllFilesInfo(string userId, CancellationToken cancellationToken);
    Task<FileModelDto> GetFileById(Guid fileId, string currentUserId, CancellationToken cancellationToken);
    Task<string> GetFilesByGroupId(string currentUserId, Guid groupId, CancellationToken cancellationToken);
    Task SaveDownloadLink(DownloadLink downloadLink, CancellationToken cancellationToken);
    Task<string> GetFilesByLink(Guid linkId, CancellationToken cancellationToken);
    Task<UploadProgress> GetUploadProgress(string userId, CancellationToken cancellationToken);
}