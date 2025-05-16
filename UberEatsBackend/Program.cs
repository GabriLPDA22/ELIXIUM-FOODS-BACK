using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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
var appSettings = appSettingsSection.Get<AppSettings>();

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(appSettings.ConnectionString));

// Registro de repositorios y servicios
builder.Services.AddSingleton(appSettings);
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configurar CORS para permitir peticiones del cliente Vue.js
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

// Configurar autenticación JWT con registro de eventos para depuración
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

// Configurar Swagger/OpenAPI
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

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UberEatsBackend API v1");

    // Usar la ruta por defecto para Swagger UI - http://localhost:5290/swagger
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

// Middleware de diagnóstico para verificar todos los encabezados de autorización
app.Use(async (context, next) =>
{
  var authHeader = context.Request.Headers["Authorization"].ToString();
  Console.WriteLine($"[DEBUG MIDDLEWARE] Cabecera Authorization: '{authHeader}'");

  await next();
});

// Middleware de autenticación antes de autorización - orden importante
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Mensaje de inicio para confirmación
Console.WriteLine($"🚀 Servidor iniciado en modo {app.Environment.EnvironmentName}");
Console.WriteLine($"🔐 JWT configurado con Issuer: {appSettings.JwtIssuer}, Audience: {appSettings.JwtAudience}");
Console.WriteLine($"📚 Documentación Swagger disponible en: http://localhost:5290/swagger");

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