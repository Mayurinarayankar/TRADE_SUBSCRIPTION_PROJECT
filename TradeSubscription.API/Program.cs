using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TradeSubscriptionAPI.Data;
using TradeSubscriptionAPI.Helpers;
using TradeSubscriptionAPI.Middleware;
using TradeSubscriptionAPI.Repositories;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services;
using TradeSubscriptionAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════
//  1. DATABASE
// ════════════════════════════════════════════════════════════
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
);

// ════════════════════════════════════════════════════════════
//  2. REPOSITORIES  (Data Access Layer)
// ════════════════════════════════════════════════════════════
builder.Services.AddScoped<IUserRepository,                 UserRepository>();
builder.Services.AddScoped<ICompanyRepository,              CompanyRepository>();
builder.Services.AddScoped<IIncotermRepository,             IncotermRepository>();
builder.Services.AddScoped<ITradeRepository,                TradeRepository>();
builder.Services.AddScoped<ISubscriptionPlanRepository,     SubscriptionPlanRepository>();
builder.Services.AddScoped<ICompanySubscriptionRepository,  CompanySubscriptionRepository>();
builder.Services.AddScoped<IInvoiceRepository,              InvoiceRepository>();

// ════════════════════════════════════════════════════════════
//  3. SERVICES  (Business Logic Layer)
// ════════════════════════════════════════════════════════════
builder.Services.AddScoped<IJwtHelper,                   JwtHelper>();
builder.Services.AddScoped<IAuthService,                 AuthService>();
builder.Services.AddScoped<ICompanyService,              CompanyService>();
builder.Services.AddScoped<IIncotermService,             IncotermService>();
builder.Services.AddScoped<ITradeService,                TradeService>();
builder.Services.AddScoped<ISubscriptionPlanService,     SubscriptionPlanService>();
builder.Services.AddScoped<ICompanySubscriptionService,  CompanySubscriptionService>();
builder.Services.AddScoped<IInvoiceService,              InvoiceService>();

// ════════════════════════════════════════════════════════════
//  4. JWT AUTHENTICATION
// ════════════════════════════════════════════════════════════
var jwtSecret  = builder.Configuration["Jwt:Secret"]
                 ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set true in production
        options.SaveToken            = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew                = TimeSpan.Zero   // no extra expiry buffer
        };

        // Return 401 JSON instead of redirect for API clients
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode  = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"success":false,"message":"Unauthorized. Please provide a valid token."}""");
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode  = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"success":false,"message":"Forbidden. You do not have permission to access this resource."}""");
            }
        };
    });

builder.Services.AddAuthorization();

// ════════════════════════════════════════════════════════════
//  5. SWAGGER / OPENAPI
// ════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Trade & Subscription Management API",
        Version     = "v1",
        Description = "Full CRUD REST API for Trades, Incoterms, Subscription Plans, " +
                      "Invoices, and Companies — secured with JWT (Bearer).",
        Contact = new OpenApiContact
        {
            Name  = "Dev Team",
            Email = "dev@tradesub.com"
        }
    });

    // JWT Bearer button inside Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Paste your JWT here.  Example:  Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML doc comments (enable <GenerateDocumentationFile> in .csproj)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ════════════════════════════════════════════════════════════
//  6. CONTROLLERS  +  JSON OPTIONS
// ════════════════════════════════════════════════════════════
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        // camelCase property names in JSON responses
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Serialize enum values as strings (e.g. "Active" not 1)
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Ignore null values in response to keep payloads clean
        opt.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ════════════════════════════════════════════════════════════
//  7. CORS
// ════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ════════════════════════════════════════════════════════════
//  8. BUILD
// ════════════════════════════════════════════════════════════
var app = builder.Build();

// ════════════════════════════════════════════════════════════
//  9. AUTO-MIGRATE ON STARTUP
// ════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying EF Core migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database is up to date.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed. Check your connection string.");
        throw;
    }
}

// ════════════════════════════════════════════════════════════
//  10. MIDDLEWARE PIPELINE  (order matters!)
// ════════════════════════════════════════════════════════════

// Global exception handler — must be first
app.UseMiddleware<ExceptionMiddleware>();

// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trade & Subscription API v1");
        c.RoutePrefix          = string.Empty;   // serve Swagger at root "/"
        c.DisplayRequestDuration();
        c.EnableFilter();
        c.EnableDeepLinking();
        c.DefaultModelsExpandDepth(-1);          // hide schema section by default
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();   // ← must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();