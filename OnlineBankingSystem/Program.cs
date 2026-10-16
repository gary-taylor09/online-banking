using OnlineBankingSystem.Database;
using OnlineBankingSystem.Extensions;
using OnlineBankingSystem.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;

// Ensure logging directory exists
var logDirectory = "C:/ApplicationLogging/OnlineBankingSystem";
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Setup NLog
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure NLog as the logging provider
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiServices();

var key = builder.Configuration.GetValue<string>("Jwt:SecretKey");

builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JWTOptions>();

builder.Services.AddAuthentication(options => 
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters.ValidIssuer = jwtOptions.Issuer;
        options.TokenValidationParameters.ValidAudience = jwtOptions.Audience;
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
    });

builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<User>(options => 
    { 
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddApiEndpoints();
 
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.SeedRolesAsync();

app.Run();
