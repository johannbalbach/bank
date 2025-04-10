namespace UserService.Db.Entities
{
    public class INNDocument : Document
    {
        public string INN { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string Issuer { get; set; }
        public string IssueDate { get; set; }
    }
}
