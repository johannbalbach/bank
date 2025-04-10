using Bank.DAL.Enums;

namespace UserService.Db.Entities
{
    public class User: BaseDeletableEntity
    {
        public string UserName { get; set; }
        public string? Patronymic { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool IsManuallyBlocked { get; set; }
        public UserRole Role { get; set; }
        public List<Guid> CardIds { get; set; } = new List<Guid>();
        public List<Guid> BankAccountIds { get; set; } = new List<Guid>();
        public List<Guid> RequestIds { get; set; } = new List<Guid>();
    }
}
