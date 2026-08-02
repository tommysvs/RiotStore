using Microsoft.EntityFrameworkCore;

namespace RiotStore.Infrastructure.Data
{
    public class RiotStoreDbContext : DbContext
    {
        public RiotStoreDbContext(DbContextOptions<RiotStoreDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<StockBalance> StockBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(p => p.product_id);
                entity.Property(p => p.product_id).HasColumnName("product_id");
                entity.Property(p => p.sku).HasColumnName("sku");
                entity.Property(p => p.name).HasColumnName("name");
                entity.Property(p => p.description).HasColumnName("description");
                entity.Property(p => p.category_id).HasColumnName("category_id");
                entity.Property(p => p.price).HasColumnName("price");
                entity.Property(p => p.initial_stock).HasColumnName("initial_stock");
                entity.Property(p => p.image_url).HasColumnName("image_url");
                entity.Property(p => p.is_active).HasColumnName("is_active");
                entity.Property(p => p.created_at).HasColumnName("created_at");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.category_id);
                entity.Property(c => c.category_id).HasColumnName("category_id");
                entity.Property(c => c.name).HasColumnName("name");
                entity.Property(c => c.description).HasColumnName("description");
                entity.Property(c => c.created_at).HasColumnName("created_at");
            });

            modelBuilder.Entity<StockBalance>(entity =>
            {
                entity.ToTable("stock_balances");
                entity.HasKey(s => s.product_id);
                entity.Property(s => s.product_id).HasColumnName("product_id");
                entity.Property(s => s.initial_stock).HasColumnName("initial_stock");
                entity.Property(s => s.total_attempts).HasColumnName("total_attempts");
                entity.Property(s => s.current_balance).HasColumnName("current_balance");
                entity.Property(s => s.status).HasColumnName("status");
                entity.Property(s => s.last_updated).HasColumnName("last_updated");
            });
        }
    }

    public class Product
    {
        public int product_id { get; set; }
        public string sku { get; set; } = null!;
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public int category_id { get; set; }
        public decimal price { get; set; }
        public int initial_stock { get; set; }
        public string image_url { get; set; } = null!;
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }

    public class Category
    {
        public int category_id { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = null!;
        public DateTime created_at { get; set; }
    }

    public class StockBalance
    {
        public int product_id { get; set; }
        public int initial_stock { get; set; }
        public int total_attempts { get; set; }
        public int current_balance { get; set; }
        public string status { get; set; } = null!;
        public DateTime last_updated { get; set; }
    }
}