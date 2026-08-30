using Microsoft.EntityFrameworkCore;
using TenantDesk.Application.Common.Interfaces;
using TenantDesk.Infrastructure.Data;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Services;
using TenantDesk.Infrastructure.Repositories;
using TenantDesk.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<PlanService>();

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<TenantService>();


builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();