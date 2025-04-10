using Bank.DAL.Enums;
using UserService.Validators;

namespace UserService.Dtos
{
    public class UserEditDto
    {
        public string? UserName { get; set; }
        [BirthDateValidation]
        public DateTime? BirthDate { get; set; }
        [PhoneNumberValidation]
        public string? PhoneNumber { get; set; }
        public UserRole? Role { get; set; }
    }
}
