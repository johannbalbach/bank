using Bank.DAL.Enums;

namespace UserService.Dtos
{
    public class UserBrieflyDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsManuallyBlocked { get; set; }
        public UserRole Role { get; set; }
    }
}
