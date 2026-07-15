using AutoWork.Application.DTOs.Media;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
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
        IFormFile file, [FromQuery] string? folder = null, [FromQuery] Guid? projectId = null)
    {
        if (file.Length == 0)
            return FailResponse<UploadMediaResponse>("File is empty.");

        await using var stream = file.OpenReadStream();
        var storagePath = await _storageService.UploadAsync(stream, file.FileName, file.ContentType, folder);
        var publicUrl = _storageService.GetPublicUrl(storagePath);

        // Bug cũ: trước đây không lưu MediaFile vào DB, khiến mọi tham chiếu MediaFileId ở nơi khác
        // (VD: gắn ảnh sản phẩm ở Brand Memory, gắn ảnh vào bài viết) không tìm thấy dữ liệu.
        var mediaFile = new MediaFile
        {
            UserId = _currentUser.UserId!.Value,
            ProjectId = projectId,
            FileName = file.FileName,
            FileUrl = publicUrl,
            StoragePath = storagePath,
            MimeType = file.ContentType,
            FileSize = file.Length
        };
        await _unitOfWork.Media.AddAsync(mediaFile);
        await _unitOfWork.SaveChangesAsync();

        return OkResponse(new UploadMediaResponse
        {
            Id = mediaFile.Id,
            FileName = mediaFile.FileName,
            PublicUrl = mediaFile.FileUrl,
            FileSize = mediaFile.FileSize
        }, "File uploaded.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var media = await _unitOfWork.Media.GetByIdAsync(id)
            ?? throw new Application.Common.Exceptions.NotFoundException("Media", id);

        if (media.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        await _storageService.DeleteAsync(media.StoragePath);
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
