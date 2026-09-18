using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common.Interfaces;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Services;
using TenantDesk.Infrastructure.Data;
using TenantDesk.Application.Services;
using TenantDesk.Infrastructure.Repositories;
using TenantDesk.Infrastructure.Identity;
using TenantDesk.Infrastructure.Services;
using TenantDesk.Infrastructure.BackgroundServices;
using TenantDesk.Infrastructure.HealthChecks;
using TenantDesk.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TenantDesk.Domain.Entities;
using System.Text;
using TenantDesk.Application.Mapping;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using TenantDesk.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RequestLoggingFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<RequestLoggingFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database")
    .AddCheck<EmailHealthCheck>("email");

builder.Services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- from development: Plan + Tenant (Phase 2) ---
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<TenantService>();


builder.Services.AddScoped<ISlaRepository, SlaRepository>();
builder.Services.AddScoped<SlaService>();

builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddScoped<IBusinessHourRepository, BusinessHourRepository>();
builder.Services.AddScoped<BusinessHourService>();

builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IBusinessTimeCalculator, BusinessTimeCalculator>();
builder.Services.AddScoped<ISlaCalculatorService, SlaCalculatorService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddHostedService<NotificationCleanupJob>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();   
builder.Services.AddScoped<CategoryService>();                            

// --- JWT Authentication setup ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
// --- end JWT Authentication setup ---

// --- Application services (Phase 3: Auth + User) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// --- end Application services ---

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(check => new
            {
                name = check.Key,
                status = check.Value.Status.ToString(),
                description = check.Value.Description
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Run();