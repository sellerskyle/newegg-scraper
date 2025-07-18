Newegg Product and Review Scraper
This C# console application scrapes product information and customer reviews from a Newegg product page. It uses Playwright and stores the data in a DuckDB database file.

How to Run the Solution
Prerequisites
.NET 8 SDK: You must have the .NET 8 SDK or a later version installed.

Google Chrome: The solution uses ChromeDriver, so you must have the Google Chrome browser installed. The Selenium package will automatically manage the driver executable.

Setup and Execution
cd GeneratedScraper

dotnet restore

dotnet run

In a browser, visit http://localhost:5046/swagger
