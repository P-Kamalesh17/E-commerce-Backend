using System.Text;
using BLL;
using DAL.Data;
using DAL.Models;
using DAL.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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