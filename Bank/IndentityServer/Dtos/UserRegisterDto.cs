using Bank.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Dtos
{
    public class UserRegisterDto
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }
}
