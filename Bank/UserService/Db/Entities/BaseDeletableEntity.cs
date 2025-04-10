namespace UserService.Db.Entities
{
    public abstract class BaseDeletableEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? ModifyDateTime { get; set; }
        public DateTime? DeleteDateTime { get; set; }
    }
}
