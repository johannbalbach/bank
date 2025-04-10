using Bank.DAL.Interfaces;

namespace Bank.DAL.Models
{
    public class BaseDeletableEntity : BaseEntity, ISoftDeletable
    {
        public DateTime? DeleteDateTime { get; set; }
    }
}
