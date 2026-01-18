using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public IEnumerable<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Product>();
            }

            searchTerm = searchTerm.Trim();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // SQL Server Full-Text Search với CONTAINS
                // ❌ BỎ ORDER BY trong raw SQL - EF Core sẽ tự động xử lý
                var products = _db.Products
                    .FromSqlRaw(@"
                        SELECT * FROM Products
                        WHERE CONTAINS((Title, Author, ISBN, Description), {0})",
                        $"\"{searchTerm}*\"")
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .OrderBy(p => p.Title) // ✅ Sắp xếp bằng LINQ sau khi query
                    .ToList();

                stopwatch.Stop();

                Debug.WriteLine($"✅ FTS SUCCESS - Found {products.Count} results in {stopwatch.ElapsedMilliseconds}ms for: '{searchTerm}'");
                Console.WriteLine($"✅ FTS: {products.Count} results in {stopwatch.ElapsedMilliseconds}ms");

                return products;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Debug.WriteLine($"❌ FTS FAILED: {ex.GetType().Name}");
                Debug.WriteLine($"   Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
                Debug.WriteLine($"⚠️ Switching to LINQ fallback...");

                Console.WriteLine($"❌ FTS Error: {ex.Message}");

                var stopwatch = Stopwatch.StartNew();

                // Fallback: Nếu FTS chưa setup hoặc lỗi, dùng LINQ
                var lowerSearchTerm = searchTerm.ToLower();
                var products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Author.ToLower().Contains(lowerSearchTerm) ||
                        p.ISBN.ToLower().Contains(lowerSearchTerm) ||
                        (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(lowerSearchTerm))
                    )
                    .OrderByDescending(p =>
                        (p.Title.ToLower().StartsWith(lowerSearchTerm) ? 5 : 0) +
                        (p.Title.ToLower().Contains(lowerSearchTerm) ? 3 : 0) +
                        (p.Author.ToLower().Contains(lowerSearchTerm) ? 2 : 0) +
                        (p.ISBN.ToLower().Contains(lowerSearchTerm) ? 1 : 0)
                    )
                    .ThenBy(p => p.Title)
                    .ToList();

                stopwatch.Stop();

                Debug.WriteLine($"⚠️ LINQ FALLBACK - Found {products.Count} results in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"⚠️ LINQ: {products.Count} results in {stopwatch.ElapsedMilliseconds}ms");

                return products;
            }
        }
    }
}