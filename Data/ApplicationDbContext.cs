using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定資料表名稱
            modelBuilder.Entity<Product>().ToTable("Products");

            // 設定索引
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // 種子資料
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "筆記型電腦", Description = "高效能筆記型電腦", Price = 25000, Stock = 10 },
                new Product { Id = 2, Name = "滑鼠", Description = "無線光學滑鼠", Price = 500, Stock = 50 },
                new Product { Id = 3, Name = "鍵盤", Description = "機械式鍵盤", Price = 1200, Stock = 30 }
            );
        }
    }

}
