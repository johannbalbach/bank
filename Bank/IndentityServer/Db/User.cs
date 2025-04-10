using Bank.DAL.Enums;

namespace IndentityServer.Db
{
    public class User
    {
        public Guid Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? ModifyDateTime { get; set; }
        public DateTime? DeleteDateTime { get; set; }
        public string UserName { get; set; }
        public string? Patronymic { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool IsManuallyBlocked { get; set; }
        public UserRole Role { get; set; }
    }
}
