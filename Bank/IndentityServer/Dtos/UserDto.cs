using Bank.DAL.Enums;

namespace IdentityServer.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string? Patronymic { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsManuallyBlocked { get; set; }
        public UserRole Role { get; set; }
    }
}
