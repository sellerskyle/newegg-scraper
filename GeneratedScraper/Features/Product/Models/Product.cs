namespace GeneratedScraper.Features.Product.Models
{
    public class Product
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

        public ProductDto ToDto(List<Review>? reviews)
        {
            return new ProductDto
            {
                Url = Url,
                Title = Title,
                Brand = Brand,
                PriceDollars = PriceDollars,
                PriceCents = PriceCents,
                RatingBase = RatingBase,
                RatingSub = RatingSub,
                Description = Description,
                ScrapedTimestampUtc = ScrapedTimestampUtc,
                Reviews = reviews ?? []
            };
        }
    }
}
