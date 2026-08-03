using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

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
        public DbSet<Client> Clients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<GeneratorBenchmark> GeneratorBenchmarks { get; set; }
        public DbSet<PurchaseAttempt> PurchaseAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.category_id);
                entity.Property(c => c.category_id).HasColumnName("category_id");
                entity.Property(c => c.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(c => c.description).HasColumnName("description");
                entity.Property(c => c.created_at).HasColumnName("created_at");
                
                entity.HasIndex(c => c.name).IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(p => p.product_id);
                entity.Property(p => p.product_id).HasColumnName("product_id");
                entity.Property(p => p.sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
                entity.Property(p => p.name).HasColumnName("name").IsRequired().HasMaxLength(150);
                entity.Property(p => p.description).HasColumnName("description");
                entity.Property(p => p.category_id).HasColumnName("category_id");
                entity.Property(p => p.price).HasColumnName("price").HasPrecision(10, 2);
                entity.Property(p => p.initial_stock).HasColumnName("initial_stock");
                entity.Property(p => p.image_url).HasColumnName("image_url").HasMaxLength(255);
                entity.Property(p => p.is_active).HasColumnName("is_active");
                entity.Property(p => p.created_at).HasColumnName("created_at");
                
                entity.HasOne(p => p.category)
                    .WithMany()
                    .HasForeignKey(p => p.category_id)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(p => p.sku).IsUnique();
                entity.HasIndex(p => p.category_id).HasDatabaseName("idx_products_category");
                entity.HasIndex(p => p.is_active).HasDatabaseName("idx_products_is_active");
            });

            modelBuilder.Entity<StockBalance>(entity =>
            {
                entity.ToTable("stock_balances");
                entity.HasKey(s => s.product_id);
                entity.Property(s => s.product_id).HasColumnName("product_id");
                entity.Property(s => s.initial_stock).HasColumnName("initial_stock");
                entity.Property(s => s.total_attempts).HasColumnName("total_attempts");
                entity.Property(s => s.current_balance).HasColumnName("current_balance");
                entity.Property(s => s.status).HasColumnName("status").HasMaxLength(30);
                entity.Property(s => s.last_updated).HasColumnName("last_updated");
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("clients");
                entity.HasKey(c => c.client_id);
                entity.Property(c => c.client_id).HasColumnName("client_id").ValueGeneratedNever();
                entity.Property(c => c.full_name).HasColumnName("full_name").IsRequired().HasMaxLength(100);
                entity.Property(c => c.email).HasColumnName("email").IsRequired().HasMaxLength(100);
                entity.Property(c => c.address).HasColumnName("address");
                entity.Property(c => c.segment).HasColumnName("segment").HasMaxLength(20).HasDefaultValue("mid-demand");
                entity.Property(c => c.created_at).HasColumnName("created_at");
                
                entity.HasIndex(c => c.email).IsUnique().HasDatabaseName("idx_clients_email");
                entity.HasIndex(c => c.segment).HasDatabaseName("idx_clients_segment");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(o => o.order_id);
                entity.Property(o => o.order_id).HasColumnName("order_id");
                entity.Property(o => o.transaction_id).HasColumnName("transaction_id").IsRequired().HasMaxLength(100);
                entity.Property(o => o.client_id).HasColumnName("client_id");
                entity.Property(o => o.total_amount).HasColumnName("total_amount").HasPrecision(10, 2);
                entity.Property(o => o.origin).HasColumnName("origin").HasMaxLength(30).HasDefaultValue("WEB_UI");
                entity.Property(o => o.status).HasColumnName("status").HasMaxLength(30).HasDefaultValue("PROCESSED");
                
                entity.Property(o => o.customer_segment).HasColumnName("customer_segment").HasMaxLength(20).HasDefaultValue("mid-demand");
                entity.Property(o => o.product_category).HasColumnName("product_category").HasMaxLength(100).HasDefaultValue("Sin categoría");
                entity.Property(o => o.is_retry).HasColumnName("is_retry").HasDefaultValue(false);
                entity.Property(o => o.original_order_id).HasColumnName("original_order_id");
                entity.Property(o => o.total_quantity_requested).HasColumnName("total_quantity_requested").HasDefaultValue(0);
                
                entity.Property(o => o.created_at).HasColumnName("created_at");
                
                entity.HasIndex(o => o.transaction_id).IsUnique();
                entity.HasIndex(o => o.client_id).HasDatabaseName("idx_orders_client");
                entity.HasIndex(o => o.created_at).HasDatabaseName("idx_orders_created_at");
                entity.HasIndex(o => o.customer_segment).HasDatabaseName("idx_orders_customer_segment");
                entity.HasIndex(o => o.product_category).HasDatabaseName("idx_orders_product_category");
                entity.HasIndex(o => o.is_retry).HasDatabaseName("idx_orders_is_retry");
                entity.HasIndex(o => o.original_order_id).HasDatabaseName("idx_orders_original_order_id");
                
                entity.HasOne<Client>().WithMany().HasForeignKey(o => o.client_id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("order_details");
                entity.HasKey(od => od.order_detail_id);
                entity.Property(od => od.order_detail_id).HasColumnName("order_detail_id");
                entity.Property(od => od.order_id).HasColumnName("order_id");
                entity.Property(od => od.product_id).HasColumnName("product_id");
                entity.Property(od => od.quantity).HasColumnName("quantity");
                entity.Property(od => od.unit_price).HasColumnName("unit_price").HasPrecision(10, 2);
                entity.Property(od => od.subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
                
                entity.HasIndex(od => od.order_id).HasDatabaseName("idx_order_details_order");
                entity.HasIndex(od => od.product_id).HasDatabaseName("idx_order_details_product");
                
                entity.HasOne<Order>().WithMany().HasForeignKey(od => od.order_id).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Product>().WithMany().HasForeignKey(od => od.product_id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GeneratorBenchmark>(entity =>
            {
                entity.ToTable("generator_benchmarks");
                entity.HasKey(g => g.benchmark_id);
                entity.Property(g => g.benchmark_id).HasColumnName("benchmark_id").ValueGeneratedOnAdd();
                entity.Property(g => g.total_events_generated).HasColumnName("total_events_generated");
                entity.Property(g => g.elapsed_seconds).HasColumnName("elapsed_seconds");
                entity.Property(g => g.events_per_second).HasColumnName("events_per_second");
                entity.Property(g => g.measured_at).HasColumnName("measured_at");
                entity.Property(g => g.notes).HasColumnName("notes").HasMaxLength(500);
                
                entity.HasIndex(g => g.measured_at).HasDatabaseName("idx_generator_benchmarks_measured_at");
            });

            modelBuilder.Entity<PurchaseAttempt>(entity =>
            {
                entity.ToTable("purchase_attempts");
                entity.HasKey(p => p.attempt_id);
                entity.Property(p => p.attempt_id).HasColumnName("attempt_id");
                entity.Property(p => p.order_id).HasColumnName("order_id");
                entity.Property(p => p.product_id).HasColumnName("product_id");
                entity.Property(p => p.product_category).HasColumnName("product_category").IsRequired().HasMaxLength(100);
                entity.Property(p => p.customer_segment).HasColumnName("customer_segment").IsRequired().HasMaxLength(20);
                entity.Property(p => p.quantity_requested).HasColumnName("quantity_requested");
                entity.Property(p => p.is_retry).HasColumnName("is_retry").HasDefaultValue(false);
                entity.Property(p => p.original_order_id).HasColumnName("original_order_id");
                entity.Property(p => p.status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(p => p.attempted_at).HasColumnName("attempted_at");
                entity.Property(p => p.processed_at).HasColumnName("processed_at");
                
                entity.HasIndex(p => p.order_id).HasDatabaseName("idx_purchase_attempts_order_id");
                entity.HasIndex(p => p.product_id).HasDatabaseName("idx_purchase_attempts_product_id");
                entity.HasIndex(p => p.product_category).HasDatabaseName("idx_purchase_attempts_product_category");
                entity.HasIndex(p => p.customer_segment).HasDatabaseName("idx_purchase_attempts_customer_segment");
                entity.HasIndex(p => p.is_retry).HasDatabaseName("idx_purchase_attempts_is_retry");
                entity.HasIndex(p => p.attempted_at).HasDatabaseName("idx_purchase_attempts_attempted_at");
                entity.HasIndex(p => p.status).HasDatabaseName("idx_purchase_attempts_status");
            });
        }
    }

    public class Product
    {
        public int product_id { get; set; }
        public string sku { get; set; } = null!;
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public int category_id { get; set; }
        public decimal price { get; set; }
        public int initial_stock { get; set; }
        public string? image_url { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public virtual Category? category { get; set; }
    }

    public class Category
    {
        public int category_id { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public DateTime created_at { get; set; }
    }

    public class StockBalance
    {
        public int product_id { get; set; }
        public int initial_stock { get; set; }
        public int total_attempts { get; set; }
        public int current_balance { get; set; }
        public string status { get; set; } = "ACTIVE";
        public DateTime last_updated { get; set; } = DateTime.UtcNow;
    }

    public class Client
    {
        public int client_id { get; set; }
        public string full_name { get; set; } = null!;
        public string email { get; set; } = null!;
        public string? address { get; set; }
        public string segment { get; set; } = "mid-demand";
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }

    public class Order
    {
        public long order_id { get; set; }
        public string transaction_id { get; set; } = null!;
        public int client_id { get; set; }
        public decimal total_amount { get; set; }
        public string origin { get; set; } = "WEB_UI";
        public string status { get; set; } = "PROCESSED";
        public string customer_segment { get; set; } = "mid-demand";
        public string product_category { get; set; } = "Sin categoría";
        public bool is_retry { get; set; } = false;
        public long? original_order_id { get; set; }
        public int total_quantity_requested { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }

    public class OrderDetail
    {
        public long order_detail_id { get; set; }
        public long order_id { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal subtotal { get; set; }
    }

    public class GeneratorBenchmark
    {
        public int benchmark_id { get; set; }
        public int total_events_generated { get; set; }
        public double elapsed_seconds { get; set; }
        public double events_per_second { get; set; }
        public DateTime measured_at { get; set; } = DateTime.UtcNow;
        public string? notes { get; set; }
    }

    public class PurchaseAttempt
    {
        public long attempt_id { get; set; }
        public long order_id { get; set; }
        public int product_id { get; set; }
        public string product_category { get; set; } = null!;
        public string customer_segment { get; set; } = null!;
        public int quantity_requested { get; set; }
        public bool is_retry { get; set; } = false;
        public long? original_order_id { get; set; }
        public string status { get; set; } = "PENDING";
        public DateTime attempted_at { get; set; } = DateTime.UtcNow;
        public DateTime? processed_at { get; set; }
    }
}