using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment;

public class UpdateCommentDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = default!;
}

