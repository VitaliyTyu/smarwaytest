using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartWay.WebApi.DTO;
using SmartWay.WebApi.Interfaces;

namespace SmartWay.WebApi.Controllers;

[Authorize]
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


    [HttpPost("upload")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadFiles([FromForm] IList<IFormFile> files, CancellationToken cancellationToken)
    {
        if (!files.Any())
        {
            _logger.LogInformation("No files to upload");
            return BadRequest("No files to upload");
        }
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
        {
            _logger.LogInformation("File not found");
            return NotFound("File not found");
        }

        var fileStream = System.IO.File.OpenRead(file.Path);

        return File(fileStream, "application/octet-stream", file.Name);
    }

    [HttpGet("download-group")]
    [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadGroupOfFiles([FromQuery] Guid groupId, CancellationToken cancellationToken)
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
}