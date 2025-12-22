using System.Text;

namespace Application.Common;

public static class CursorHelper
{
    public static string EncodeCursor(DateTime createdAtUtc, Guid id)
    {
        var cursorValue = $"{createdAtUtc:O}|{id}";
        var bytes = Encoding.UTF8.GetBytes(cursorValue);
        return Convert.ToBase64String(bytes);
    }

    public static (DateTime CreatedAtUtc, Guid Id)? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var cursorValue = Encoding.UTF8.GetString(bytes);
            var parts = cursorValue.Split('|');
            
            if (parts.Length != 2)
                return null;

            if (!DateTime.TryParse(parts[0], out var createdAtUtc))
                return null;

            if (!Guid.TryParse(parts[1], out var id))
                return null;

            return (createdAtUtc, id);
        }
        catch
        {
            return null;
        }
    }
}

