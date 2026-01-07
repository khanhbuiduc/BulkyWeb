using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Title = "Product 1", Description = "Description 1", ISBN = "ISBN001", Author = "Author 1", ListPrice = 90, Price = 90, Price50 = 85, Price100 = 80, CategoryId = 1, ImageUrl = "" },
                new Product { Id = 2, Title = "Product 2", Description = "Description 2", ISBN = "ISBN002", Author = "Author 2", ListPrice = 70, Price = 70, Price50 = 65, Price100 = 60, CategoryId = 2, ImageUrl = "" },
                new Product { Id = 3, Title = "Product 3", Description = "Description 3", ISBN = "ISBN003", Author = "Author 3", ListPrice = 80, Price = 80, Price50 = 75, Price100 = 70, CategoryId = 3, ImageUrl = "" }
            );
        }
    }
}