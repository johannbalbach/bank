namespace CreditService.Dtos
{
    public class CreditRatingDto
    {
        public Guid UserId { get; set; }
        public int CreditScore { get; set; }
        public int TotalCredits { get; set; }
        public int OverduePayments { get; set; } 
        public string RatingDescription { get; set; }
    }
}
