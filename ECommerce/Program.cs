using System.Text;
using BLL;
using DAL.Data;
using DAL.Models;
using DAL.Services;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;


// Must exist before WebApplication.CreateBuilder() runs — its static web assets
// loader throws DirectoryNotFoundException if wwwroot is missing, and even once
// past that, WebRootFileProvider is resolved from whatever's on disk at startup,
// so a wwwroot created later (e.g. lazily by FileUploadService on first upload)
// is never picked up by UseStaticFiles().
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products"));

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

// Register File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

//Authentication AND Authorization

//Token Validation Parameters

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),

    ValidateIssuer = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],

    ValidateAudience = true,
    ValidAudience = builder.Configuration["Jwt:Audience"],

    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddSingleton(tokenValidationParameters);

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

//Add JWT Bearer Authentication
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable static files for image serving
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//Truncate database first
//using (var scope = app.Services.CreateScope())
//{
//    var appDbRemover = new AppDbRemover(scope.ServiceProvider.GetRequiredService<AppDbContext>());
//    appDbRemover.TruncateAllDataAsync().Wait();
//}

//AppDbInitializer.Seed(app).Wait();
//AppDbInitializer.SeedRoles(app).Wait();

app.Run();