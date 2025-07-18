namespace GeneratedScraper.Features.Product.Models
{
    public class Review
    {
        public long ProductId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public bool? IsVerifiedBuyer { get; set; }
        public int? HelpfulVotes { get; set; }
    }
}
