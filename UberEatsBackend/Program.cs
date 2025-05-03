using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UberEatsBackend.Data;
using UberEatsBackend.Middleware;
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
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configurar CORS
builder.Services.AddCustomCors();

// Configurar autenticación JWT
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
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSecret))
  };
});

// Agregar autorización
builder.Services.AddAuthorization();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Controladores
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "UberEatsBackend API", Version = "v1" });

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

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
