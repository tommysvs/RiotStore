using RiotStore.Consumer.Workers;
using RiotStore.Consumer.Services.Interfaces;
using RiotStore.Consumer.Services.Implementations;
using RiotStore.Infrastructure.Data;
using RiotStore.Infrastructure.Repositories.Implementations;
using RiotStore.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Infrastructure
builder.Services.AddDbContext<RiotStoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IOrderProcessingService, OrderProcessingService>();
builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();

// Workers
builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();
host.Run();