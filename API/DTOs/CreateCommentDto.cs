using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = default!;

    public Guid? ParentCommentId { get; set; }
}

