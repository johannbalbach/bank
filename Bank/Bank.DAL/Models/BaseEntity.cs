namespace Bank.DAL.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreateDateTime { get; set; }
        public DateTime? ModifyDateTime { get; set; }
    }
}
