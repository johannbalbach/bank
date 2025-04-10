namespace Bank.DAL.Interfaces
{
    public interface ISoftDeletable
    {
        public DateTime? DeleteDateTime { get; set; }
    }
}
