using GeneratedScraper.Features.Product.Models;
using Microsoft.Playwright;
using PlaywrightExtraSharp;
using System.Globalization;
using PlaywrightExtraSharp.Plugins.ExtraStealth;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;

namespace GeneratedScraper.Services
{
    public class ScraperService
    {
        private static IBrowser? _browser;
        private static readonly List<string> _userAgents = new()
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/114.0"
        };

        private static int _agentIndex = 0;

        private static readonly Random _random = new();

        public ScraperService()
        {
        }

        public async Task<(Product, List<Review>)> ScrapeProductPageAsync(string url)
        {

            var playwrightExtra = new PlaywrightExtra(PlaywrightExtraSharp.Models.BrowserTypeEnum.Chromium);
            playwrightExtra.Install();
            playwrightExtra.Use(new StealthExtraPlugin());
            await playwrightExtra.LaunchAsync(new()
            {
                Headless = false
            }, persistContext: false);

            if (playwrightExtra == null) return (null, null);

            var context = await playwrightExtra.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = _userAgents[_agentIndex],
            });

            _agentIndex = _agentIndex + 1 % _userAgents.Count;

            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60000 });

                await Task.Delay(_random.Next(2000, 5000));

                var product = await ScrapeProductDataAsync(page, url);
                var reviews = await ScrapeReviewsAsync(page);

                return (product, reviews);
            }
            catch (Exception e)
            {
                var x = e;
                return (null, null);
            }
            finally
            {
                //await page.CloseAsync();
                //await context.CloseAsync();
            }
        }

        private async Task<Product> ScrapeProductDataAsync(IPage page, string url)
        {
            var product = new Product { Url = url, ScrapedTimestampUtc = DateTime.UtcNow };

            product.Title = await GetTitle(page);

            product.Brand = await GetBrand(page);

            product.PriceDollars = await GetPriceDollars(page);

            product.PriceCents = await GetPriceCents(page);

            var rating = await GetRating(page);

            product.RatingBase = rating.Item1;
            product.RatingSub = rating.Item2;

            product.Description = await GetDescription(page);

            return product;
        }

        private async Task<List<Review>> ScrapeReviewsAsync(IPage page)
        {
            var reviews = new List<Review>();

            var reviewElements = await GetReviewElements(page);

            if (reviewElements != null)
            {
                foreach (var element in reviewElements)
                {
                    try
                    {
                        var review = new Review();

                        review.ReviewerName = await GetReviewerName(element);

                        review.Rating = await GetReviewRating(element);

                        review.Title = await GetReviewTitle(element);

                        review.Body = await GetReviewBody(element);

                        review.Date = await GetReviewDate(element);

                        review.IsVerifiedBuyer = await GetReviewerVerified(element);

                        review.HelpfulVotes = await GetReviewHelpfulVotes(element);

                        reviews.Add(review);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Skipping a review due to parsing error: {ex.Message}");
                    }
                }
            }

            return reviews;
        }

        private async Task<string> GetTitle(IPage page)
        {
            return await page.Locator("h1.product-title").InnerTextAsync() ?? "N/A"; 
        }

        private async Task<string> GetBrand(IPage page)
        {
            //return await page.Locator("div.product-view-brand > a").First.GetAttributeAsync("title") ?? "N/A";
            var shopAllText = await page.Locator("div.seller-store-link").InnerTextAsync();

            string pattern = @"Shop All\s+(.*?)\s+Products";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = regex.Match(shopAllText);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;

        }

        private async Task<int?> GetPriceDollars(IPage page)
        {
            var priceDollarsText = await page.Locator("div.price-current > strong").First.InnerTextAsync();
            if (int.TryParse(priceDollarsText.Replace(",", ""), NumberStyles.Currency, CultureInfo.InvariantCulture, out var priceDollars))
            {
                return priceDollars;
            }

            return null;
        }

        private async Task<int?> GetPriceCents(IPage page)
        {
            var priceCentsText = await page.Locator("div.price-current > sup").First.InnerTextAsync();
            if (int.TryParse(priceCentsText.Replace(".", ""), NumberStyles.Currency, CultureInfo.InvariantCulture, out var priceCents))
            {
                return priceCents;
            }

            return null;
        }

        private async Task<(int?, int?)> GetRating(IPage page)
        {
            var ratingValue = await page.Locator("div.product-rating > I").First.GetAttributeAsync("title");
            var str = ratingValue == null ? null : ratingValue.Split(' ')[0];
            if (str != null && double.TryParse(str, out var rating))
            {
                var parts = str.Split(".");
                if (parts != null)
                {
                    if (parts.Length == 1)
                    {
                        return (int.Parse(parts[0]), 0);

                    }
                    else
                    {
                        return (int.Parse(parts[0]), int.Parse(parts[1]));

                    }
                }
            }

            return (null, null);
        }

        private async Task<string> GetDescription(IPage page)
        {
            return await page.Locator("div.product-bullets").InnerTextAsync();
        }

        private async Task<IReadOnlyList<ILocator>?> GetReviewElements(IPage page)
        {
            await page.Locator("div:has-text('Reviews').tab-nav").First.ClickAsync();
            await page.WaitForSelectorAsync("div.comments");
            await page.WaitForSelectorAsync("div.comments-cell");

            return await page.Locator("div.comments-cell").AllAsync();
        }

        private async Task<string> GetReviewerName(ILocator element)
        {
            return await element.Locator("div.comments-name").InnerTextAsync();
        }

        private async Task<int?> GetReviewRating(ILocator element)
        {
            var ratingClass = await element.Locator("div.comments-title > i.rating").GetAttributeAsync("class");
            if (!string.IsNullOrEmpty(ratingClass) && int.TryParse(ratingClass[ratingClass.Length - 1].ToString(), out int rating))
            {
                return rating;
            }

            return null;
        }

        private async Task<string> GetReviewTitle(ILocator element)
        {
            return await element.Locator("span.comments-title-content").InnerTextAsync();
        }

        private async Task<string> GetReviewBody(ILocator element)
        {
            return await element.Locator("div.comments-content").InnerTextAsync();
        }

        private async Task<DateTime?> GetReviewDate(ILocator element)
        {
            var dateText = await element.Locator("div.comments-title > span.comments-text").InnerTextAsync();
            if (DateTime.TryParse(dateText, out var date))
            {
                return date;
            }

            return null;
        }

        private async Task<bool?> GetReviewerVerified(ILocator element)
        {
            var verifiedBadge = await element.Locator("div.comments-verified-owner").CountAsync();
            return verifiedBadge > 0;
        }

        private async Task<int?> GetReviewHelpfulVotes(ILocator element)
        {
            var votesText = await element.Locator("div.review-helpful").InnerTextAsync();
            if (!string.IsNullOrEmpty(votesText) && int.TryParse(votesText, out int votes))
            {
                return votes;
            }

            return null;
        }
    }
}
