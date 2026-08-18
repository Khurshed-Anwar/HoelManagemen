using Asp.Versioning;
using HotelManagement.Authorization;
using HotelManagement.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using HotelManagement.Authorization.Handlers;
using HotelManagement.Data;
using HotelManagement.Data.AppSettings;
using HotelManagement.Identity;
using HotelManagement.Services.Admin;
using HotelManagement.Services.Auth;
using HotelManagement.Services.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Database ──────────────────────────────────────────────────────────────────
    builder.Services.AddDbContext<HotelListringDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("HotelListingConnectionString")));

    // ── Identity ──────────────────────────────────────────────────────────────────
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit           = true;
        options.Password.RequiredLength         = 8;
        options.Password.RequireUppercase       = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail         = true;
    })
    .AddEntityFrameworkStores<HotelListringDbContext>()
    .AddDefaultTokenProviders();

    // ── JWT Settings ──────────────────────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    // ── JWT Authentication ────────────────────────────────────────────────────────
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew                = TimeSpan.Zero  // No grace period — token expires exactly at 5 min
        };
    });

    // ── Authorization ─────────────────────────────────────────────────────────────
    builder.Services.AddAuthorization();

    // Dynamic policy provider — auto-creates policies for [RequirePermission("X.Y")]
    // No need to register each permission policy manually in Program.cs
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    builder.Services.AddScoped<IAuthorizationHandler,           PermissionAuthorizationHandler>();

    // ── API Versioning ────────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion                   = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions                   = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat           = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // ── Application Services ──────────────────────────────────────────────────────
    builder.Services.AddScoped<ITokenService,                TokenService>();
    builder.Services.AddScoped<IAuthService,                 AuthService>();
    builder.Services.AddScoped<IPermissionService,           PermissionService>();
    builder.Services.AddScoped<IResourcePermissionGenerator, ResourcePermissionGenerator>();
    builder.Services.AddScoped<IDepartmentService,           DepartmentService>();
    builder.Services.AddScoped<IRoleManagementService,       RoleManagementService>();
    builder.Services.AddScoped<IUserManagementService,       UserManagementService>();

    // ── Controllers ───────────────────────────────────────────────────────────────
    builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info = new()
            {
                Title   = "Hotel Management API",
                Version = "v1",
                Description = "JWT-authenticated, permission-based Hotel Management REST API"
            };

            // Add Bearer security scheme
            document.Components ??= new();
            document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Type        = SecuritySchemeType.Http,
                    Scheme      = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT access token"
                });

            // Require Bearer globally
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id   = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    }
                ] = []
            });

            return Task.CompletedTask;
        });
    });

    // ── Build ─────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Seed Database ─────────────────────────────────────────────────────────────
    try
    {
        using var scope = app.Services.CreateScope();
        await DbInitializer.SeedAsync(
            scope.ServiceProvider.GetRequiredService<HotelListringDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>());
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();  // ← Must be first

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();  // Serves raw OpenAPI JSON at /openapi/v1.json
        app.MapScalarApiReference(options =>
        {
            options.Title                = "Hotel Management API";
            options.DefaultHttpClient    = new(ScalarTarget.Http, ScalarClient.HttpClient);
            options.Authentication       = new ScalarAuthenticationOptions
            {
                PreferredSecurityScheme  = JwtBearerDefaults.AuthenticationScheme
            };
        });  // UI available at /scalar/v1
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();   // ← Must come before UseAuthorization
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // Startup failed — DI container not available, log to console
    Console.WriteLine($"[FATAL] Application failed to start: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;  // Re-throw so the process exits with a non-zero code
}


