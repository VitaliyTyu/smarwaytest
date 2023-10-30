using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartWay.WebApi.Data.Entities;
using SmartWay.WebApi.Interfaces;
using SmartWay.WebApi.Models;
using SmartWay.WebApi.Services;

namespace SmartWay.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly IFilesService _filesService;
    private readonly UserManager<IdentityUser> _userManager;

    public FilesController(ILogger<FilesController> logger, IFilesService filesService,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _filesService = filesService;
        _userManager = userManager;
    }


    [Authorize]
    [HttpPost("upload")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadFiles([FromForm] IList<IFormFile> files, CancellationToken cancellationToken)
    {
        if (!files.Any())
            return BadRequest("No files to upload");
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        await _filesService.UploadFiles(files, currentUserId, cancellationToken);

        _logger.LogInformation("Files uploaded");

        return Ok();
    }

    [Authorize]
    [HttpGet("upload-progress")]
    public async Task<ActionResult<UploadProgress>> UploadProgress(Guid? fileId, Guid? groupId,
        CancellationToken cancellationToken)
    {
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        if (fileId == null && groupId == null)
            return BadRequest();

        return await _filesService.GetUploadProgress(currentUserId, cancellationToken);
    }

    [Authorize]
    [HttpGet("all")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FileModelDto>>> GetAllFilesInfo(CancellationToken cancellationToken)
    {
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        return await _filesService.GetAllFilesInfo(currentUserId, cancellationToken);
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadFile([FromQuery] Guid fileId, CancellationToken cancellationToken)
    {
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        var file = await _filesService.GetFileById(fileId, currentUserId, cancellationToken);

        if (file == null)
            return NotFound("File not found");

        var fileStream = System.IO.File.OpenRead(file.Path);

        return File(fileStream, "application/octet-stream", file.Name);
    }

    [Authorize]
    [HttpGet("download-group")]
    [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult>
        DownloadGroupOfFiles([FromQuery] Guid groupId,
            CancellationToken cancellationToken) // TODO сделать общий ендпоинт для загрузки архивов
    {
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        var zipArchivePath = await _filesService.GetFilesByGroupId(currentUserId, groupId, cancellationToken);

        var fileStream = System.IO.File.OpenRead(zipArchivePath);

        return File(fileStream, "application/octet-stream", Path.GetFileName(zipArchivePath));
    }


    [Authorize]
    [HttpPost("generate-link")]
    public async Task<ActionResult<string>> GenerateDownloadLink(Guid? fileId, Guid? groupId,
        CancellationToken cancellationToken)
    {
        var currentUserId = (await _userManager.GetUserAsync(User))?.Id;

        if (currentUserId == null)
        {
            _logger.LogInformation("User not found");
            return BadRequest("User not found");
        }

        if (fileId == null && groupId == null)
            return BadRequest();

        var linkId = Guid.NewGuid();

        var linkUrl = Url.Action("DownloadFilesByLink", "Files", new { linkId = linkId }, Request.Scheme,
            Request.Host.Value);
        
        var downloadLink = new DownloadLink()
        {
            Id = linkId,
            UserId = currentUserId,
            FileId = fileId,
            GroupId = groupId,
            Url = linkUrl,
        };

        await _filesService.SaveDownloadLink(downloadLink, cancellationToken);

        return linkUrl;
    }


    [HttpGet("download-by-link")]
    [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadFilesByLink(Guid linkId, CancellationToken cancellationToken)
    {
        string archivePath;

        try
        {
            archivePath = await _filesService.GetFilesByLink(linkId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while download files by link");
            return BadRequest("Invalid link");
        }


        var fileStream = System.IO.File.OpenRead(archivePath);

        return File(fileStream, "application/octet-stream", Path.GetFileName(archivePath));
    }
}