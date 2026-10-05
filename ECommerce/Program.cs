using BLL;
using DAL.Data;
using DAL.Models;
using DAL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Database connection string

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnectionString");

// Apply EF migrations before Serilog initializes, so Serilog's
// autoCreateSqlTable doesn't get undone by a pending migration
// (e.g. DropLogsTable) that runs afterwards.
using (var migrationContext = new AppDbContext(
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options))
{
    migrationContext.Database.Migrate();
}

// Setup for Serilog

Serilog.Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Add Identity
builder.Services
    .AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register your DAO and BLL
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<UserBLL>();
builder.Services.AddScoped<ProductDAO>();
builder.Services.AddScoped<ProductBLL>();
builder.Services.AddScoped<CategoryDAO>();
builder.Services.AddScoped<CategoryBLL>();
builder.Services.AddScoped<CartDAO>();
builder.Services.AddScoped<CartBLL>();
builder.Services.AddScoped<OrderDAO>();
builder.Services.AddScoped<OrderBLL>();
builder.Services.AddScoped<OrderItemDAO>();
builder.Services.AddScoped<OrderItemBLL>();
builder.Services.AddScoped<PaymentDAO>();
builder.Services.AddScoped<PaymentBLL>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

AppDbInitializer.Seed(app);

app.Run();