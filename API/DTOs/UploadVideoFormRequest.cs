using System.ComponentModel.DataAnnotations;
using Domain.Videos;

namespace API.DTOs;

public class UploadVideoFormRequest
{
    [Required(ErrorMessage = "视频文件不能为空")]
    public IFormFile VideoFile { get; set; } = default!;

    [Required(ErrorMessage = "视频标题不能为空")]
    [StringLength(120, ErrorMessage = "标题长度不能超过120个字符")]
    public string Title { get; set; } = default!;

    [StringLength(2000, ErrorMessage = "描述长度不能超过2000个字符")]
    public string? Description { get; set; }

    public IFormFile? ThumbnailFile { get; set; }

    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
}
