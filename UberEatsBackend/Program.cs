using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Amazon.S3;
using Amazon.SimpleEmail; // ← LÍNEA AGREGADA
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using UberEatsBackend.Data;
using UberEatsBackend.Repositories;
using UberEatsBackend.Services;
using UberEatsBackend.Utils;

// Configurar comportamiento de timestamp para PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// =====================================
// CONFIGURACIÓN DE APPSETTINGS
// =====================================

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>() ?? new AppSettings();

// Configurar AWS Settings por separado
var awsSection = builder.Configuration.GetSection("AWS");
var awsSettings = awsSection.Get<AWSSettings>();

// Storage Settings
var storageSection = builder.Configuration.GetSection("StorageSettings");
var storageSettings = storageSection.Get<StorageSettings>();

// SendGrid Settings
var sendGridSection = builder.Configuration.GetSection("SendGrid");
var sendGridSettings = sendGridSection.Get<SendGridSettings>();

// Asegurar que AWS esté en AppSettings
if (awsSettings != null)
{
  appSettings.AWS = awsSettings;
}

// Asegurar que SendGrid esté en AppSettings
if (sendGridSettings != null)
{
  appSettings.SendGrid = sendGridSettings;
}

Console.WriteLine("🔧 Configuración cargada:");
Console.WriteLine($"   JWT Issuer: {appSettings.JwtIssuer}");
Console.WriteLine($"   JWT Audience: {appSettings.JwtAudience}");
Console.WriteLine($"   AWS Region: {awsSettings?.Region}");
Console.WriteLine($"   S3 Bucket: {awsSettings?.S3?.BucketName}");
Console.WriteLine($"   Use S3 Storage: {storageSettings?.UseS3Storage}");
Console.WriteLine($"   SendGrid From Email: {sendGridSettings?.FromEmail}");
Console.WriteLine($"   SES From Email: {awsSettings?.SES?.FromEmail}");

// =====================================
// CONFIGURACIÓN DE BASE DE DATOS
// =====================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(appSettings.ConnectionString));

// =====================================
// REGISTRO DE SERVICIOS BÁSICOS
// =====================================

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

// Business services
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBusinessService, BusinessService>();

// Promotion services
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();

// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// =====================================
// CONFIGURACIÓN DE AWS S3 (SOLO S3, SIN SES AUTOMÁTICO)
// =====================================

if (awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
{
  Console.WriteLine($"🔧 Configurando AWS S3...");
  Console.WriteLine($"   Region: {awsSettings.Region}");
  Console.WriteLine($"   Bucket: {awsSettings.S3?.BucketName}");
  Console.WriteLine($"   AccessKey: {awsSettings.AccessKey?.Substring(0, 8)}...");

  // Configurar S3
  builder.Services.AddSingleton<IAmazonS3>(provider =>
  {
    try
    {
      var config = new AmazonS3Config
      {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region),
        Timeout = TimeSpan.FromMinutes(5),
        MaxErrorRetry = 3,
        UseHttp = false // Forzar HTTPS
      };

      var s3Client = new AmazonS3Client(awsSettings.AccessKey, awsSettings.SecretKey, config);
      Console.WriteLine($"✅ Cliente S3 creado exitosamente");
      return s3Client;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"🔴 Error creando cliente S3: {ex.Message}");
      Console.WriteLine($"🔴 StackTrace: {ex.StackTrace}");
      throw;
    }
  });

  // ✅ CONFIGURAR SES **SOLO** SI ESTÁ EXPLÍCITAMENTE CONFIGURADO EN APPSETTINGS
  if (!string.IsNullOrEmpty(awsSettings.SES?.FromEmail))
  {
    Console.WriteLine($"🔧 Configurando AWS SES...");
    Console.WriteLine($"   SES From Email: {awsSettings.SES.FromEmail}");

    builder.Services.AddSingleton<IAmazonSimpleEmailService>(provider =>
    {
      try
      {
        var config = new Amazon.SimpleEmail.AmazonSimpleEmailServiceConfig
        {
          RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region),
          Timeout = TimeSpan.FromMinutes(2),
          MaxErrorRetry = 3
        };

        var sesClient = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(
                awsSettings.AccessKey,
                awsSettings.SecretKey,
                config);

        Console.WriteLine($"✅ Cliente SES creado exitosamente");
        return sesClient;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"🔴 Error creando cliente SES: {ex.Message}");
        throw;
      }
    });

    Console.WriteLine($"✅ AWS S3 y SES configurados para región: {awsSettings.Region}");
  }
  else
  {
    Console.WriteLine($"✅ AWS S3 configurado para región: {awsSettings.Region}");
    Console.WriteLine($"⚠️ SES no configurado - se usará SendGrid o DummyEmailService");
  }
}
else
{
  Console.WriteLine($"⚠️ AWS Settings no encontrados o incompletos");
  if (awsSettings == null) Console.WriteLine("   - awsSettings es null");
  if (string.IsNullOrEmpty(awsSettings?.AccessKey)) Console.WriteLine("   - AccessKey está vacío");
  if (string.IsNullOrEmpty(awsSettings?.SecretKey)) Console.WriteLine("   - SecretKey está vacío");
}

// =====================================
// CONFIGURACIÓN DE SERVICIOS DE ALMACENAMIENTO
// =====================================

if (storageSettings?.UseS3Storage == true && awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
{
  builder.Services.AddScoped<IStorageService, S3StorageService>();
  Console.WriteLine($"✅ S3 Storage Service registrado - Bucket: {awsSettings.S3?.BucketName}");
}
else
{
  Console.WriteLine($"🔴 ERROR: S3 Storage requerido pero no configurado correctamente");
  Console.WriteLine($"   UseS3Storage: {storageSettings?.UseS3Storage}");
  Console.WriteLine($"   AWS AccessKey válido: {!string.IsNullOrEmpty(awsSettings?.AccessKey)}");
  Console.WriteLine($"   AWS SecretKey válido: {!string.IsNullOrEmpty(awsSettings?.SecretKey)}");
  Console.WriteLine($"   S3 Bucket configurado: {!string.IsNullOrEmpty(awsSettings?.S3?.BucketName)}");

  throw new InvalidOperationException("S3 Storage requerido pero no configurado correctamente. Verifica tu appsettings.json");
}

// =====================================
// CONFIGURACIÓN DE SERVICIOS DE EMAIL (PRIORIDAD)
// =====================================

Console.WriteLine("🔧 Configurando servicio de email...");

// Orden de prioridad para servicios de email:
// 1. AWS SES (si está configurado y tenemos credenciales)
// 2. SendGrid (si está configurado)
// 3. DummyEmailService (para desarrollo)

bool emailServiceConfigured = false;

// 1. Intentar configurar AWS SES primero
if (awsSettings != null &&
    !string.IsNullOrEmpty(awsSettings.AccessKey) &&
    !string.IsNullOrEmpty(awsSettings.SES?.FromEmail))
{
  try
  {
    // SES ya está configurado arriba, solo registrar el servicio
    builder.Services.AddScoped<IEmailService, SESEmailService>();
    Console.WriteLine($"✅ Email Service (AWS SES) registrado");
    Console.WriteLine($"   From Email: {awsSettings.SES.FromEmail}");
    Console.WriteLine($"   Region: {awsSettings.Region}");

    if (builder.Environment.IsDevelopment())
    {
      Console.WriteLine("⚠️  IMPORTANTE: Para desarrollo con SES:");
      Console.WriteLine("   1. Verifica tu email en la consola de AWS SES");
      Console.WriteLine("   2. O usa el modo sandbox verificando sender y receiver");
      Console.WriteLine("   3. Los emails no verificados se simularán en logs");
    }

    emailServiceConfigured = true;
  }
  catch (Exception ex)
  {
    Console.WriteLine($"🔴 Error configurando SES: {ex.Message}");
  }
}

// 2. Si SES no está disponible, intentar SendGrid
if (!emailServiceConfigured &&
    sendGridSettings != null &&
    !string.IsNullOrEmpty(sendGridSettings.ApiKey))
{
  try
  {
    // Configurar SendGrid
    builder.Services.AddSendGrid(options =>
    {
      options.ApiKey = sendGridSettings.ApiKey;
    });

    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
    Console.WriteLine($"✅ Email Service (SendGrid) registrado como fallback");
    Console.WriteLine($"   From Email: {sendGridSettings.FromEmail}");
    Console.WriteLine($"   API Key: {sendGridSettings.ApiKey?.Substring(0, 8)}...");

    emailServiceConfigured = true;
  }
  catch (Exception ex)
  {
    Console.WriteLine($"🔴 Error configurando SendGrid: {ex.Message}");
  }
}

// 3. Si ninguno está disponible, usar DummyEmailService
if (!emailServiceConfigured)
{
  builder.Services.AddScoped<IEmailService, DummyEmailService>();
  Console.WriteLine("⚠️ Email Service DUMMY registrado (NO enviará emails reales)");
  Console.WriteLine("   - Todos los emails se mostrarán en logs y consola");
  Console.WriteLine("   - Perfecto para desarrollo y testing");
  emailServiceConfigured = true;
}

// Registrar el servicio genérico de imágenes
builder.Services.AddScoped<IImageService, ImageService>();
Console.WriteLine("✅ Image Service registrado");

// =====================================
// CONFIGURACIÓN DE LOGGING
// =====================================

builder.Services.AddLogging(logging =>
{
  logging.ClearProviders();
  logging.AddConsole();
  logging.AddDebug();
  if (builder.Environment.IsDevelopment())
  {
    logging.SetMinimumLevel(LogLevel.Debug);
  }
  else
  {
    logging.SetMinimumLevel(LogLevel.Information);
  }
});

// =====================================
// CONFIGURACIÓN DE CORS
// =====================================

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowVueApp", policy =>
  {
    policy.WithOrigins("http://localhost:5173") // URL de desarrollo de Vue
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
  });
});

Console.WriteLine("✅ CORS configurado para permitir peticiones desde http://localhost:5173");

// =====================================
// CONFIGURACIÓN DE AUTENTICACIÓN JWT
// =====================================

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
    ClockSkew = TimeSpan.FromMinutes(5) // 5 minutos de tolerancia para problemas de sincronización de reloj
  };

  // Configuración para depuración de problemas de JWT
  options.Events = new JwtBearerEvents
  {
    OnMessageReceived = context =>
    {
      var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

      if (!string.IsNullOrEmpty(authHeader))
      {
        Console.WriteLine($"📝 Authorization Header recibido: {authHeader.Substring(0, Math.Min(30, authHeader.Length))}...");

        // Extraer el token JWT del formato "Bearer {token}"
        string token = authHeader;
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
          token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // Verificar si el token parece ser un JWT válido (debe tener dos puntos)
        if (token.Count(c => c == '.') == 2)
        {
          context.Token = token;
          Console.WriteLine($"✅ Token JWT válido extraído");
        }
        else
        {
          Console.WriteLine("⚠️ Token no tiene formato JWT válido");
        }
      }

      return Task.CompletedTask;
    },
    OnAuthenticationFailed = context =>
    {
      Console.WriteLine($"🔴 Error de autenticación: {context.Exception.Message}");

      if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
      {
        context.Response.Headers.Append("Token-Expired", "true");
        Console.WriteLine("🔴 El token ha expirado");
      }
      else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
      {
        Console.WriteLine("🔴 La firma del token es inválida");
      }
      else if (context.Exception is SecurityTokenMalformedException)
      {
        Console.WriteLine("🔴 El token está malformado");
      }

      return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
      Console.WriteLine($"✅ Token validado correctamente para usuario: {context.Principal?.Identity?.Name}");
      return Task.CompletedTask;
    },
    OnChallenge = context =>
    {
      Console.WriteLine($"⚠️ Desafío de autenticación: {context.AuthenticateFailure?.Message ?? "Sin detalles"}");
      return Task.CompletedTask;
    }
  };
});

// Agregar autorización
builder.Services.AddAuthorization();
Console.WriteLine("✅ Autenticación y autorización JWT configuradas");

// =====================================
// AUTOMAPPER
// =====================================

builder.Services.AddAutoMapper(typeof(Program));
Console.WriteLine("✅ AutoMapper configurado");

// =====================================
// CONTROLADORES
// =====================================

builder.Services.AddControllers();
Console.WriteLine("✅ Controladores registrados");

// =====================================
// CONFIGURACIÓN DE SWAGGER/OPENAPI
// =====================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "UberEatsBackend API",
    Version = "v1",
    Description = "API para aplicación tipo UberEats con servicio de imágenes S3 y email SES/SendGrid",
    Contact = new OpenApiContact
    {
      Name = "Soporte",
      Email = "soporte@ubereatsclone.com"
    }
  });

  // Configurar Swagger para usar JWT
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

// =====================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// =====================================

var app = builder.Build();

// =====================================
// INICIALIZACIÓN DE BASE DE DATOS
// =====================================

if (app.Environment.IsDevelopment())
{
  using (var scope = app.Services.CreateScope())
  {
    var services = scope.ServiceProvider;
    try
    {
      var context = services.GetRequiredService<ApplicationDbContext>();
      context.Database.EnsureCreated();
      Console.WriteLine("✅ Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "🔴 Error al inicializar la base de datos");
      Console.WriteLine($"🔴 Error al inicializar la base de datos: {ex.Message}");
    }
  }
}

// =====================================
// VERIFICACIÓN DE SERVICIOS CRÍTICOS
// =====================================

using (var scope = app.Services.CreateScope())
{
  try
  {
    // Verificar que S3 esté funcionando
    var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
    await s3Client.ListBucketsAsync();
    Console.WriteLine("✅ Conexión a S3 verificada exitosamente");

    // Verificar SES (solo si está configurado)
    if (awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
    {
      try
      {
        var sesClient = scope.ServiceProvider.GetService<IAmazonSimpleEmailService>();
        if (sesClient != null)
        {
          // Verificar quota de SES (esto confirma que la conexión funciona)
          await sesClient.GetSendQuotaAsync();
          Console.WriteLine("✅ Conexión a SES verificada exitosamente");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"⚠️ SES disponible pero con advertencia: {ex.Message}");
        Console.WriteLine("   - Verifica que el email esté verificado en SES");
        Console.WriteLine("   - En desarrollo se usarán logs simulados");
      }
    }

    // Verificar que el servicio de imágenes esté registrado
    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
    Console.WriteLine("✅ Image Service verificado exitosamente");

    // Verificar que el servicio de email esté registrado
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    Console.WriteLine($"✅ Email Service verificado: {emailService.GetType().Name}");

    // Test rápido del email service en desarrollo
    if (app.Environment.IsDevelopment())
    {
      Console.WriteLine("🧪 Realizando test de email service...");
      var testResult = await emailService.SendEmailAsync(
          "test@example.com",
          "Test Email",
          "<h1>Test HTML</h1>",
          "Test Text");

      Console.WriteLine($"   Resultado del test: {(testResult ? "✅ Éxito" : "❌ Fallo")}");
    }

  }
  catch (Exception ex)
  {
    Console.WriteLine($"🔴 Error verificando servicios críticos: {ex.Message}");
    Console.WriteLine($"🔴 StackTrace: {ex.StackTrace}");

    // Solo fallar si es S3, los demás pueden funcionar en modo degradado
    if (ex.Message.Contains("S3") || ex.Message.Contains("Bucket"))
    {
      throw;
    }
    else
    {
      Console.WriteLine("⚠️ Continuando con servicios en modo degradado...");
    }
  }
}

// =====================================
// CONFIGURACIÓN DE MIDDLEWARE
// =====================================

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UberEatsBackend API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
  });
  Console.WriteLine("✅ Swagger habilitado en /swagger");
}

// Middleware CORS - importante colocarlo antes de los middleware de autenticación
app.UseCors("AllowVueApp");

// En desarrollo, podemos desactivar la redirección HTTPS para simplificar
if (!app.Environment.IsDevelopment())
{
  app.UseHttpsRedirection();
}

// Servir archivos estáticos (para cualquier contenido estático que necesitemos)
app.UseStaticFiles();

// Middleware de diagnóstico para debugging de requests (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
  app.Use(async (context, next) =>
  {
    var authHeader = context.Request.Headers["Authorization"].ToString();

    // Solo mostrar log si hay un header de autorización para evitar spam
    if (!string.IsNullOrEmpty(authHeader))
    {
      Console.WriteLine($"[DEBUG] {context.Request.Method} {context.Request.Path} - Auth: {authHeader.Substring(0, Math.Min(30, authHeader.Length))}...");
    }

    await next();
  });
}

// Middleware de autenticación antes de autorización - orden importante
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// =====================================
// MENSAJES DE INICIO Y EJECUCIÓN
// =====================================

Console.WriteLine("=".PadLeft(80, '='));
Console.WriteLine("🚀 UBEREATS BACKEND INICIADO EXITOSAMENTE");
Console.WriteLine("=".PadLeft(80, '='));
Console.WriteLine($"🏗️  Entorno: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔐 JWT Issuer: {appSettings.JwtIssuer}");
Console.WriteLine($"🔐 JWT Audience: {appSettings.JwtAudience}");
Console.WriteLine($"📚 Swagger: http://localhost:5290/swagger");
Console.WriteLine($"🌐 CORS: http://localhost:5173");
Console.WriteLine($"☁️  Almacenamiento: AWS S3 ({awsSettings?.S3?.BucketName})");

// Mostrar servicio de email configurado
var emailServiceType = "No configurado";
if (awsSettings?.SES?.FromEmail != null)
{
  emailServiceType = $"AWS SES ({awsSettings.SES.FromEmail})";
}
else if (sendGridSettings?.FromEmail != null)
{
  emailServiceType = $"SendGrid ({sendGridSettings.FromEmail})";
}
else
{
  emailServiceType = "Dummy Service (Solo logs)";
}

Console.WriteLine($"📧 Email: {emailServiceType}");
Console.WriteLine($"🌍 Región AWS: {awsSettings?.Region}");
Console.WriteLine($"🔗 Base URL S3: {awsSettings?.S3?.BaseUrl}");

Console.WriteLine("\n📁 Servicios registrados:");
Console.WriteLine("   ├── 🏢 Business Service");
Console.WriteLine("   ├── 🍽️  Restaurant Service");
Console.WriteLine("   ├── 🥘 Product Service");
Console.WriteLine("   ├── 🔗 RestaurantProduct Service");
Console.WriteLine("   ├── 📦 Order Service");
Console.WriteLine("   ├── 👤 User Service");
Console.WriteLine("   ├── 🖼️  Image Service (S3)");
Console.WriteLine("   ├── ☁️  S3 Storage Service");
Console.WriteLine($"   ├── 📧 Email Service ({emailServiceType})");
Console.WriteLine("   └── 🎟️  Promotion Service");

if (app.Environment.IsDevelopment())
{
  Console.WriteLine("\n🔧 Notas para desarrollo:");
  if (awsSettings?.SES?.FromEmail != null)
  {
    Console.WriteLine("   📧 SES: Verifica emails en AWS Console para testing");
    Console.WriteLine("   📧 Los emails no verificados se simularán en logs");
  }
  Console.WriteLine("   🧪 Todos los servicios se pueden testear desde Swagger");
  Console.WriteLine("   📝 Los logs de email aparecerán en consola para debugging");
}

Console.WriteLine("\n🔧 Endpoints principales:");
Console.WriteLine("   ├── POST /api/images/upload/base64");
Console.WriteLine("   ├── POST /api/images/upload");
Console.WriteLine("   ├── DELETE /api/images");
Console.WriteLine("   ├── POST /api/images/upload/multiple");
Console.WriteLine("   ├── POST /api/Auth/forgot-password");
Console.WriteLine("   └── POST /api/Auth/reset-password");

Console.WriteLine("=".PadLeft(80, '='));
Console.WriteLine("🎯 Backend listo para recibir peticiones...");
Console.WriteLine("=".PadLeft(80, '='));

try
{
  app.Run();
  Console.WriteLine("✅ Aplicación finalizada correctamente");
}
catch (Exception ex)
{
  Console.WriteLine($"🔴 Error fatal al ejecutar la aplicación: {ex.Message}");
  Console.WriteLine($"🔴 StackTrace: {ex.StackTrace}");
  throw;
}
