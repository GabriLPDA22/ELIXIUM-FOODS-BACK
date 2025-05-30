using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Amazon.S3;
using Amazon.SimpleEmail;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using UberEatsBackend.Data;
using UberEatsBackend.Repositories;
using UberEatsBackend.Services;
using UberEatsBackend.Utils;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Configuración de AppSettings
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>() ?? new AppSettings();

var awsSection = builder.Configuration.GetSection("AWS");
var awsSettings = awsSection.Get<AWSSettings>();

var storageSection = builder.Configuration.GetSection("StorageSettings");
var storageSettings = storageSection.Get<StorageSettings>();

var sendGridSection = builder.Configuration.GetSection("SendGrid");
var sendGridSettings = sendGridSection.Get<SendGridSettings>();

if (awsSettings != null)
{
    appSettings.AWS = awsSettings;
}

if (sendGridSettings != null)
{
    appSettings.SendGrid = sendGridSettings;
}

// Configuración de Base de Datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(appSettings.ConnectionString));

// Registro de Servicios
builder.Services.AddSingleton(appSettings);
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GoogleAuthService>();

// User services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Restaurant services
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();

// Order services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Product services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// RestaurantProduct services
builder.Services.AddScoped<IRestaurantProductRepository, RestaurantProductRepository>();
builder.Services.AddScoped<IRestaurantProductService, RestaurantProductService>();

// offer services
builder.Services.AddScoped<IProductOfferRepository, ProductOfferRepository>();
builder.Services.AddScoped<IProductOfferService, ProductOfferService>();

// Business services
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService, BusinessService>();

// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configuración de AWS S3
if (awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
{
    builder.Services.AddSingleton<IAmazonS3>(provider =>
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region),
            Timeout = TimeSpan.FromMinutes(5),
            MaxErrorRetry = 3,
            UseHttp = false
        };

        return new AmazonS3Client(awsSettings.AccessKey, awsSettings.SecretKey, config);
    });

    // Configurar SES si está configurado
    if (!string.IsNullOrEmpty(awsSettings.SES?.FromEmail))
    {
        builder.Services.AddSingleton<IAmazonSimpleEmailService>(provider =>
        {
            var config = new Amazon.SimpleEmail.AmazonSimpleEmailServiceConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region),
                Timeout = TimeSpan.FromMinutes(2),
                MaxErrorRetry = 3
            };

            return new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(
                awsSettings.AccessKey,
                awsSettings.SecretKey,
                config);
        });
    }
}

// Configuración de Storage
if (storageSettings?.UseS3Storage == true && awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
{
    builder.Services.AddScoped<IStorageService, S3StorageService>();
}
else
{
    throw new InvalidOperationException("S3 Storage requerido pero no configurado correctamente.");
}

// Configuración de Email Service
bool emailServiceConfigured = false;

// Prioridad: AWS SES -> SendGrid -> DummyEmailService
if (awsSettings != null &&
    !string.IsNullOrEmpty(awsSettings.AccessKey) &&
    !string.IsNullOrEmpty(awsSettings.SES?.FromEmail))
{
    builder.Services.AddScoped<IEmailService, SESEmailService>();
    emailServiceConfigured = true;
}
else if (sendGridSettings != null && !string.IsNullOrEmpty(sendGridSettings.ApiKey))
{
    builder.Services.AddSendGrid(options =>
    {
        options.ApiKey = sendGridSettings.ApiKey;
    });

    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
    emailServiceConfigured = true;
}

if (!emailServiceConfigured)
{
    builder.Services.AddScoped<IEmailService, DummyEmailService>();
}

builder.Services.AddScoped<IImageService, ImageService>();

// Configuración de Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();

    if (builder.Environment.IsDevelopment())
    {
        // En desarrollo, mostrar solo logs importantes
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore.StaticFiles", LogLevel.Error);
        logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Error);
        logging.AddFilter("Microsoft.AspNetCore.Mvc.ModelBinding", LogLevel.Error);
        logging.AddFilter("Microsoft.AspNetCore.HostFiltering", LogLevel.Error);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
    }
    else
    {
        // En producción, mostrar solo logs críticos
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddFilter("Microsoft", LogLevel.Warning);
    }
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configuración de Autenticación JWT
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
        ValidIssuer = appSettings.JwtIssuer,
        ValidAudience = appSettings.JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSecret)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                string token = authHeader;
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }

                if (token.Count(c => c == '.') == 2)
                {
                    context.Token = token;
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UberEatsBackend API",
        Version = "v1",
        Description = "API para aplicación tipo UberEats",
        Contact = new OpenApiContact
        {
            Name = "Soporte",
            Email = "soporte@ubereatsclone.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// Crear directorio wwwroot si no existe para evitar advertencias
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

// Inicialización de Base de Datos
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error al inicializar la base de datos");
        }
    }
}

// Verificación de servicios críticos
using (var scope = app.Services.CreateScope())
{
    try
    {
        var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await s3Client.ListBucketsAsync();

        var sesClient = scope.ServiceProvider.GetService<IAmazonSimpleEmailService>();
        if (sesClient != null)
        {
            await sesClient.GetSendQuotaAsync();
        }

        scope.ServiceProvider.GetRequiredService<IImageService>();
        scope.ServiceProvider.GetRequiredService<IEmailService>();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error verificando servicios críticos");

        if (ex.Message.Contains("S3") || ex.Message.Contains("Bucket"))
        {
            throw;
        }
    }
}

// Configuración de Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UberEatsBackend API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

app.UseCors("AllowVueApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
