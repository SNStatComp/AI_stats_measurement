using AI_stats_measurement.Backend.Clients;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Clients;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AIMeasureDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));

//builder.Services.AddDbContext<AIMeasureDbContext>(options =>
//   options.UseSqlServer(
//       builder.Configuration.GetConnectionString("Default")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AIMeasureDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AIStatsMeasurement";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AIStatsMeasurementClient";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ILlmQuerier, ChatGPTQuerier>();
builder.Services.AddScoped<ILlmQuerier, ChatGPTWebSearchQuerier>();
builder.Services.AddScoped<ILlmQuerier, GeminiQuerier>();
builder.Services.AddScoped<ILlmQuerier, GeminiWebSearchQuerier>();
builder.Services.AddScoped<ILlmQuerier, GrokQuerier>();
builder.Services.AddScoped<ILlmQuerier, GrokWebSearchQuerier>();

builder.Services.AddScoped<LlmAggregator>();

builder.Services.AddScoped<FactChecker>(sp =>
    new FactChecker(0.05m, "CBS")
);

builder.Services.AddScoped<AnalyticsService>();

builder.Services.AddScoped<SourceNormalizer>();

builder.Services.AddScoped<EvaluationPipeline>();

builder.Services.AddScoped<IDataTransferService, DataTransferService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AIMeasureDbContext>();
    db.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AIMeasureDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    var adminEmail = builder.Configuration["Identity:AdminEmail"];
    var adminPassword = builder.Configuration["Identity:AdminPassword"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
