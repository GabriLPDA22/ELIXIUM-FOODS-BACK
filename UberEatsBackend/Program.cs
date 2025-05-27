using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Amazon.S3;
using UberEatsBackend.Data;
using UberEatsBackend.Repositories;
using UberEatsBackend.Services;
using UberEatsBackend.Utils;

// Configurar comportamiento de timestamp para PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Configuración de AppSettings
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>() ?? new AppSettings();

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(appSettings.ConnectionString));

// Registro de repositorios y servicios básicos
builder.Services.AddSingleton(appSettings);
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

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
// CONFIGURACIÓN DE ALMACENAMIENTO E IMÁGENES
// =====================================

// Configurar AWS S3
var awsSettings = builder.Configuration.GetSection("AWS").Get<AWSSettings>();
if (awsSettings != null && !string.IsNullOrEmpty(awsSettings.AccessKey))
{
    builder.Services.AddSingleton<IAmazonS3>(provider =>
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region)
        };

        return new AmazonS3Client(awsSettings.AccessKey, awsSettings.SecretKey, config);
    });
    Console.WriteLine($"✅ AWS S3 configurado para región: {awsSettings.Region}");
}

// Configurar el servicio de almacenamiento según la configuración
var storageSettings = builder.Configuration.GetSection("StorageSettings").Get<StorageSettings>();
if (storageSettings?.UseS3Storage == true && awsSettings != null)
{
    builder.Services.AddScoped<IStorageService, S3StorageService>();
    Console.WriteLine($"✅ Almacenamiento S3 habilitado - Bucket: {awsSettings.S3?.BucketName}");
}
else
{
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
    Console.WriteLine("✅ Almacenamiento local habilitado");
}

// Registrar el servicio genérico de imágenes
builder.Services.AddScoped<IImageService, ImageService>();
Console.WriteLine("✅ Servicio genérico de imágenes registrado");

// =====================================
// CONFIGURACIÓN DE CORS
// =====================================

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowVueApp", builder =>
  {
    builder.WithOrigins("http://localhost:5173") // URL de desarrollo de Vue
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
  });
});

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
      Console.WriteLine($"📝 Encabezado de autorización recibido: {authHeader}");

      if (!string.IsNullOrEmpty(authHeader))
      {
        // Extraer el token JWT del formato "Bearer {token}"
        string token = authHeader;
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
          token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // Verificar si el token parece ser un JWT válido (debe tener dos puntos)
        if (token.Count(c => c == '.') != 2)
        {
          Console.WriteLine("⚠️ Advertencia: El token no tiene el formato JWT válido (header.payload.signature)");
          // En desarrollo, permitimos probar incluso con tokens mal formados
          if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
          {
            Console.WriteLine("⚠️ Estamos en desarrollo, intentando procesar el token de todos modos");
            context.Token = token;
          }
        }
        else
        {
          // Token parece válido, asignarlo
          context.Token = token;
          Console.WriteLine($"✅ Token JWT válido extraído: {token.Substring(0, Math.Min(30, token.Length))}...");
        }
      }
      else
      {
        Console.WriteLine("⚠️ No se encontró encabezado de autorización");
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
        Console.WriteLine("🔴 El token está malformado. Debe tener el formato de JWT válido con tres secciones separadas por puntos.");
      }

      // Log detallado para depuración
      Console.WriteLine($"StackTrace: {context.Exception.StackTrace}");

      return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
      Console.WriteLine($"✅ Token validado correctamente para usuario: {context.Principal?.Identity?.Name}");
      return Task.CompletedTask;
    },
    OnChallenge = context =>
    {
      Console.WriteLine($"⚠️ Desafío de autenticación activado: {context.AuthenticateFailure?.Message ?? "Sin detalles de error"}");
      return Task.CompletedTask;
    }
  };
});

// Agregar autorización
builder.Services.AddAuthorization();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Controladores
builder.Services.AddControllers();

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
    Description = "API para aplicación tipo UberEats con servicio de imágenes genérico",
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

// Inicializar la base de datos (opcional)
if (app.Environment.IsDevelopment())
{
  using (var scope = app.Services.CreateScope())
  {
    var services = scope.ServiceProvider;
    try
    {
      var context = services.GetRequiredService<ApplicationDbContext>();

      // Asegurarse de que la base de datos exista
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
// CONFIGURACIÓN DE MIDDLEWARE
// =====================================

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UberEatsBackend API v1");
    c.RoutePrefix = "swagger";
  });

  Console.WriteLine("✅ Swagger habilitado en /swagger");
}

// Middleware CORS - importante colocarlo antes de los middleware de autenticación
app.UseCors("AllowVueApp");
Console.WriteLine("✅ CORS configurado para permitir peticiones desde http://localhost:5173");

// En desarrollo, podemos desactivar la redirección HTTPS para simplificar
if (!app.Environment.IsDevelopment())
{
  app.UseHttpsRedirection();
}

// Servir archivos estáticos (para almacenamiento local)
app.UseStaticFiles();

// Middleware de diagnóstico para verificar todos los encabezados de autorización
app.Use(async (context, next) =>
{
  var authHeader = context.Request.Headers["Authorization"].ToString();
  
  // Solo mostrar log si hay un header de autorización para evitar spam
  if (!string.IsNullOrEmpty(authHeader))
  {
    Console.WriteLine($"[DEBUG] Authorization Header: '{authHeader.Substring(0, Math.Min(50, authHeader.Length))}...'");
  }

  await next();
});

// Middleware de autenticación antes de autorización - orden importante
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// =====================================
// MENSAJES DE INICIO Y EJECUCIÓN
// =====================================

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("🚀 UBEREATS BACKEND INICIADO");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine($"🏗️  Entorno: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔐 JWT Issuer: {appSettings.JwtIssuer}");
Console.WriteLine($"🔐 JWT Audience: {appSettings.JwtAudience}");
Console.WriteLine($"📚 Swagger: http://localhost:5290/swagger");
Console.WriteLine($"🌐 CORS: http://localhost:5173");

// Información del almacenamiento
if (storageSettings?.UseS3Storage == true)
{
    Console.WriteLine($"☁️  Almacenamiento: AWS S3 ({awsSettings?.S3?.BucketName})");
    Console.WriteLine($"🌍 Región S3: {awsSettings?.Region}");
}
else
{
    Console.WriteLine("💾 Almacenamiento: Local (wwwroot/uploads)");
}

Console.WriteLine("📁 Servicios registrados:");
Console.WriteLine("   ├── 🏢 Business Service");
Console.WriteLine("   ├── 🍽️  Restaurant Service");
Console.WriteLine("   ├── 🥘 Product Service");
Console.WriteLine("   ├── 🔗 RestaurantProduct Service");
Console.WriteLine("   ├── 📦 Order Service");
Console.WriteLine("   ├── 👤 User Service");
Console.WriteLine("   ├── 🖼️  Image Service (Genérico)");
Console.WriteLine("   ├── 💾 Storage Service");
Console.WriteLine("   └── 🎟️  Promotion Service");

Console.WriteLine("=".PadRight(60, '='));

try
{
  app.Run();
  Console.WriteLine("✅ Aplicación finalizada correctamente");
}
catch (Exception ex)
{
  Console.WriteLine($"🔴 Error fatal al ejecutar la aplicación: {ex.Message}");
  Console.WriteLine(ex.StackTrace);
}