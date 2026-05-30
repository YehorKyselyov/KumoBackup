using KumoBackup.Server.Application.Backups;
using KumoBackup.Server.Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KumoBackup.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/backups")]
public sealed class BackupsController(
    BackupService backupService) : ControllerBase
{
    [HttpPost("upload")]
    [RequestSizeLimit(104_857_600)]
    public async Task<ActionResult<BackupResponse>> Upload(
        IFormFile? file,
        [FromForm] string? deviceAlias,
        [FromForm] string? appVersion,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Backup file is required." });
        }

        await using var content = file.OpenReadStream();
        var result = await backupService.UploadAsync(
            new BackupUpload
            {
                Content = content,
                FileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                DeviceAlias = deviceAlias,
                AppVersion = appVersion,
            },
            cancellationToken);

        return result switch
        {
            BackupUploadResult.Created created => Ok(created.Backup),
            BackupUploadResult.Invalid invalid => BadRequest(new { error = invalid.Error }),
            BackupUploadResult.TooLarge tooLarge => StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new { error = tooLarge.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BackupResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await backupService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await backupService.GetDownloadAsync(id, cancellationToken);
        return result switch
        {
            BackupDownloadResult.Found found => PhysicalFile(found.Path, found.ContentType, found.FileName),
            BackupDownloadResult.NotFound { Error: not null } missing => NotFound(new { error = missing.Error }),
            BackupDownloadResult.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await backupService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
