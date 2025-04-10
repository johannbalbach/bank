namespace UserService.Db.Entities
{
    public class Passport: Document
    {
        public string Series {  get; set; }
        public string Number { get; set; }
        public string Issuer { get; set; }
        public string IssueDate { get; set; }
    }
}
