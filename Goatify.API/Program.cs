using Goatify.Infrastructure;
using Goatify.Infrastructure.Data;
using Goatify.API.Services;
using Goatify.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

var jwtKey = builder.Configuration["Jwt:Key"] ??
    throw new InvalidOperationException("Jwt:Key is missing from configuration.");

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
}

// DB
builder.Services.AddDbContext<GoatifyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<GoatifyDbContext>()
    .AddDefaultTokenProviders();

// JWT Service Registration ✅ ADD THIS
builder.Services.AddScoped<JwtService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Remove duplicate: builder.Services.AddAuthentication(); ❌ DELETE THIS LINE

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain the deployed frontend origin.");
        }

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .SelectMany(item => item.Value!.Errors.Select(error => error.ErrorMessage))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray();

        return new BadRequestObjectResult(new ProblemDetails
        {
            Title = "Validation failed",
            Detail = errors.Length == 0
                ? "Please check the entered information and try again."
                : string.Join(" ", errors),
            Status = StatusCodes.Status400BadRequest
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedRoles(services);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
            logger.LogError(exception, "Unhandled API error.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Server error",
            Detail = "Something went wrong while processing your request. Please try again.",
            Status = StatusCodes.Status500InternalServerError
        });
    });
});

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();
app.MapControllers();

app.Run();
