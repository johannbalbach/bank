using Bank.DAL.Enums;
using System.ComponentModel.DataAnnotations;
using UserService.Validators;

namespace UserService.Dtos
{
    public class UserRegisterDto
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [PasswordValidation]
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }
}
