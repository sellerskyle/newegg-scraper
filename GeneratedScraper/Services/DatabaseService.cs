using DuckDB.NET.Data;
using GeneratedScraper.Features.Product.Models;
using System.Data;

namespace GeneratedScraper.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly string _dbPath;

        public DatabaseService(string dbPath = "newegg_data.duckdb")
        {
            _dbPath = dbPath;
            _connection = new DuckDBConnection($"Data Source={_dbPath}");
        }

        public void InitializeDatabase()
        {
            _connection.Open();

            using var command = _connection.CreateCommand();

            command.CommandText = @"
            CREATE SEQUENCE IF NOT EXISTS product_id_seq;
            CREATE TABLE IF NOT EXISTS Products (
                Id BIGINT PRIMARY KEY DEFAULT nextval('product_id_seq'),
                Url VARCHAR NOT NULL UNIQUE,
                Title VARCHAR,
                Brand VARCHAR,
                PriceDollars INT,
                PriceCents INT,
                RatingBase INT,                
                RatingSub INT,
                Description VARCHAR,
                ScrapedTimestampUtc TIMESTAMP
            );";
            command.ExecuteNonQuery();

            command.CommandText = @"
            CREATE SEQUENCE IF NOT EXISTS review_id_seq;
            CREATE TABLE IF NOT EXISTS Reviews (
                Id BIGINT PRIMARY KEY DEFAULT nextval('review_id_seq'),
                ProductId BIGINT,
                ReviewerName VARCHAR,
                Rating INT,
                Title VARCHAR,
                Body VARCHAR,
                Date TIMESTAMP,
                IsVerifiedBuyer BOOLEAN,
                HelpfulVotes INT,
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );";
            command.ExecuteNonQuery();
            Console.WriteLine("Database initialized successfully.");
            _connection.Close();
        }

        public long SaveProduct(Product product)
        {
            _connection.Open();
            using var command = _connection.CreateCommand();

            command.CommandText = @"
            INSERT INTO Products (Id, Url, Title, Brand, PriceDollars, PriceCents, RatingBase, RatingSub, Description, ScrapedTimestampUtc)
            VALUES (nextval('review_id_seq'), ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ON CONFLICT (Url) DO UPDATE SET
                Title = excluded.Title,
                Brand = excluded.Brand,
                PriceDollars = excluded.PriceDollars,
                PriceCents = excluded.PriceCents,
                RatingBase = excluded.RatingBase,                
                RatingSub = excluded.RatingSub,
                Description = excluded.Description,
                ScrapedTimestampUtc = excluded.ScrapedTimestampUtc;
        ";

            command.Parameters.Add(new DuckDBParameter(product.Url));
            command.Parameters.Add(new DuckDBParameter(product.Title));
            command.Parameters.Add(new DuckDBParameter(product.Brand));
            command.Parameters.Add(new DuckDBParameter(product.PriceDollars));
            command.Parameters.Add(new DuckDBParameter(product.PriceCents));
            command.Parameters.Add(new DuckDBParameter(product.RatingBase));
            command.Parameters.Add(new DuckDBParameter(product.RatingSub));
            command.Parameters.Add(new DuckDBParameter(product.Description));
            command.Parameters.Add(new DuckDBParameter(product.ScrapedTimestampUtc));

            command.ExecuteNonQuery();

            command.CommandText = "SELECT Id FROM Products WHERE Url = ?;";
            command.Parameters.Clear();
            command.Parameters.Add(new DuckDBParameter(product.Url));

            var productId = command.ExecuteScalar();
            if (productId == null)
            {
                _connection.Close();
                throw new InvalidOperationException("Could not retrieve product ID after saving.");
            }
            _connection.Close();

            return (long)productId;

        }

        public void SaveReviews(IEnumerable<Review> reviews)
        {
            _connection.Open();

            using var transaction = _connection.BeginTransaction();
            using var command = _connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = @"
            INSERT INTO Reviews (Id, ProductId, ReviewerName, Rating, Title, Body, Date, IsVerifiedBuyer, HelpfulVotes)
            VALUES (nextval('review_id_seq'), ?, ?, ?, ?, ?, ?, ?, ?);";

            // Prepare parameters once
            command.Parameters.Add(new DuckDBParameter()); // ProductId
            command.Parameters.Add(new DuckDBParameter()); // ReviewerName
            command.Parameters.Add(new DuckDBParameter()); // Rating
            command.Parameters.Add(new DuckDBParameter()); // Title
            command.Parameters.Add(new DuckDBParameter()); // Body
            command.Parameters.Add(new DuckDBParameter()); // Date
            command.Parameters.Add(new DuckDBParameter()); // IsVerifiedBuyer
            command.Parameters.Add(new DuckDBParameter()); // HelpfulVotes

            foreach (var review in reviews)
            {
                ((DuckDBParameter)command.Parameters[0]).Value = review.ProductId;
                ((DuckDBParameter)command.Parameters[1]).Value = review.ReviewerName;
                ((DuckDBParameter)command.Parameters[2]).Value = review.Rating;
                ((DuckDBParameter)command.Parameters[3]).Value = review.Title;
                ((DuckDBParameter)command.Parameters[4]).Value = review.Body;
                ((DuckDBParameter)command.Parameters[5]).Value = review.Date;
                ((DuckDBParameter)command.Parameters[6]).Value = review.IsVerifiedBuyer;
                ((DuckDBParameter)command.Parameters[7]).Value = review.HelpfulVotes;
                command.ExecuteNonQuery();
            }

            transaction.Commit();

            _connection.Close();
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}