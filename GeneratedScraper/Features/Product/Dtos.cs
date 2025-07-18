using GeneratedScraper.Features.Product.Models;

namespace GeneratedScraper.Features.Product
{
    public class ProductDto
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int? PriceDollars { get; set; }
        public int? PriceCents { get; set; }
        public int? RatingBase { get; set; }
        public int? RatingSub { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ScrapedTimestampUtc { get; set; }

        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
