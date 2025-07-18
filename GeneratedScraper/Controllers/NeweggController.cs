using GeneratedScraper.Features.Product;
using GeneratedScraper.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneratedScraper.Controllers
{
    [ApiController]
    [Route("newegg")]
    public class NeweggController : ControllerBase
    {

        private DatabaseService _db;

        private ScraperService _scraper;

        public NeweggController(DatabaseService db, ScraperService scraper)
        {
            _db = db;
            _scraper = scraper;
        }

        [HttpGet(Name = "GetItem")]
        public async Task<ProductDto> GetItem(string url)
        {
            var result = await _scraper.ScrapeProductPageAsync(url);

            var product = result.Item1;
            var reviews = result.Item2;

            if (product == null)
            {
                //Throw internal service error
                return null;
            }

            var productId = _db.SaveProduct(product);

            reviews.ForEach(r => r.ProductId = productId);

            if (reviews != null)
            {
                _db.SaveReviews(reviews);
            }

            return product.ToDto(reviews);
        }
    }
}
