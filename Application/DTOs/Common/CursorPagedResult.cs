namespace Application.DTOs.Common;

public class CursorPagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public int Count { get; set; }
}

