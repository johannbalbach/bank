namespace UserService.Dtos
{
    public class PassportDto
    {
        public Guid Id { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string Issuer { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
