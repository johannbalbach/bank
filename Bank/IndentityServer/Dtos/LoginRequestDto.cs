using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Dtos
{
    public class LoginRequestDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
