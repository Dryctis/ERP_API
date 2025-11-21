using System.Text;
using ERP_API.Common.Configuration;
using ERP_API.Common.Filters;
using ERP_API.Common.Seed;
using ERP_API.Data;
using ERP_API.Repositories.Implementations;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Implementations;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings no configurados");

var adminSettings = builder.Configuration
    .GetSection(AdminSettings.SectionName)
    .Get<AdminSettings>() ?? throw new InvalidOperationException("Admin settings no configurados");

var databaseSettings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>() ?? new DatabaseSettings();


var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? Environment.GetEnvironmentVariable("ERP_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string no configurado");

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? Environment.GetEnvironmentVariable("ERP_JWT_SECRET")
    ?? throw new InvalidOperationException("JWT Secret no configurado");

var adminPassword = builder.Configuration["Admin:DefaultPassword"]
    ?? Environment.GetEnvironmentVariable("ERP_ADMIN_PASSWORD")
    ?? throw new InvalidOperationException("Admin password no configurado");


jwtSettings.Secret = jwtSecret;
jwtSettings.Validate();

adminSettings.DefaultPassword = adminPassword;
adminSettings.Validate();


builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(adminSettings);
builder.Services.AddSingleton(databaseSettings);

builder.Services.AddControllers(opts =>
{
    opts.Filters.Add<GlobalExceptionFilter>();
});


builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(databaseSettings.CommandTimeout);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        if (databaseSettings.EnableSensitiveDataLogging)
            opt.EnableSensitiveDataLogging();

        if (databaseSettings.EnableDetailedErrors)
            opt.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ERP_API.Validators.CustomerCreateValidator>();


var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

      
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                logger.LogWarning(
                    "Authentication failed: {Error}",
                    context.Exception.Message);

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP API",
        Version = "v1",
        Description = "API para sistema ERP"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AppCors", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Aplicando migraciones...");
        await db.Database.MigrateAsync();

        logger.LogInformation("Iniciando seed de datos...");
        await DatabaseSeeder.SeedAsync(db, adminSettings);

        logger.LogInformation("Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Error fatal al inicializar base de datos");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
 
    app.UseHsts();
    app.UseHttpsRedirection();
}

if (allowedOrigins.Length > 0)
{
    app.UseCors("AppCors");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");


var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation(
    "Aplicación iniciada. Ambiente: {Environment}, Issuer: {Issuer}",
    app.Environment.EnvironmentName,
    jwtSettings.Issuer);

app.Run();