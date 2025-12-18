using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool Succeeded { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Token { get; set; }
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
    }

}
