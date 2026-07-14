using AutoWork.Application.DTOs.Media;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Shared.Enums;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoWork.API.Controllers;

[Authorize]
public class MediaController : ApiControllerBase
{
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MediaController(IStorageService storageService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MediaFileDto>>>> GetMedia([FromQuery] string? folder = null)
    {
        var files = await _unitOfWork.Media.GetByUserIdAsync(_currentUser.UserId!.Value, folder);
        return OkResponse(files.Select(f => new MediaFileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            ContentType = f.MimeType,
            FileSize = f.FileSize,
            FileType = MapFileType(f.MimeType),
            PublicUrl = f.FileUrl,
            CreatedAt = f.CreatedAt
        }).ToList());
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadMediaResponse>>> Upload(
        IFormFile file, [FromQuery] string? folder = null)
    {
        if (file.Length == 0)
            return FailResponse<UploadMediaResponse>("File is empty.");

        await using var stream = file.OpenReadStream();
        var url = await _storageService.UploadAsync(stream, file.FileName, file.ContentType, folder);

        return OkResponse(new UploadMediaResponse
        {
            FileName = file.FileName,
            PublicUrl = url,
            FileSize = file.Length
        }, "File uploaded.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var media = await _unitOfWork.Media.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Media", id);

        if (media.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        await _storageService.DeleteAsync(media.FileUrl);
        await _unitOfWork.Media.DeleteAsync(media);
        await _unitOfWork.SaveChangesAsync();
        return OkResponse("Media deleted.");
    }

    private static MediaFileType MapFileType(string mimeType) => mimeType switch
    {
        var m when m.StartsWith("image/") => MediaFileType.Image,
        var m when m.StartsWith("video/") => MediaFileType.Video,
        var m when m.StartsWith("audio/") => MediaFileType.Audio,
        "application/pdf" => MediaFileType.Document,
        _ => MediaFileType.Other
    };
}
