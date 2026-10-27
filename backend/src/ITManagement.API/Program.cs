using ITManagement.API.Extensions;
using ITManagement.API.Middleware;
using ITManagement.Application;
using ITManagement.Infrastructure;
using Serilog;
using ITManagement.API.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API services
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(item => JsonNamingPolicy.CamelCase.ConvertName(item.Key),
                item => item.Value!.Errors.Select(error => string.IsNullOrEmpty(error.ErrorMessage)
                    ? "Giá trị không hợp lệ." : error.ErrorMessage).ToArray());
        return new BadRequestObjectResult(ApiResponse.Fail("Vui lòng kiểm tra dữ liệu nhập.", errors));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IT Management API", Version = "v1" });
    c.AddBearerSecurityDefinition();
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database
await app.SeedDatabaseAsync();

app.Run();

public partial class Program { } // Phục vụ integration testing
