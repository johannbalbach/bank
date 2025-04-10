using System.ComponentModel.DataAnnotations;
using UserService.Validators;

namespace UserService.Dtos
{
    public class LoginRequestDto
    {
        [EmailAddress]
        public string Email { get; set; }
        [PasswordValidation]
        public string Password { get; set; }
    }
}
