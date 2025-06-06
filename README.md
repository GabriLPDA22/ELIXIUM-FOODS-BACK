# 🚀 Setup del Entorno Local - UberEatsBackend

## 📋 Requisitos Previos

Antes de empezar, asegúrate de tener instalado:

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) o superior
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)
- IDE recomendado: Visual Studio 2022, VS Code o JetBrains Rider

## 🛠️ Configuración Paso a Paso

### 1. 📥 Clonar el Repositorio

```bash
git clone https://github.com/tuusuario/UberEatsBackend.git
cd UberEatsBackend
```

### 2. 🐳 Levantar la Base de Datos con Docker

Primero, asegúrate de que Docker Desktop esté ejecutándose, luego ejecuta:

```bash
# Crear y levantar el contenedor de PostgreSQL
docker run --name ubereats-postgres \
  -e POSTGRES_DB=ubereats_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=password123 \
  -p 5432:5432 \
  -d postgres:15
```

**Alternativamente, puedes usar docker-compose** (recomendado):

Crea un archivo `docker-compose.yml` en la raíz del proyecto:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    container_name: ubereats-postgres
    environment:
      POSTGRES_DB: ubereats_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: Admin123!
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

Luego ejecuta:

```bash
docker-compose up -d
```

### 3. ⚙️ Configurar Variables de Entorno

Crea un archivo `.env` en la raíz del proyecto con la siguiente configuración:

```env
# Database Configuration
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=ubereats_db;Username=postgres;Password=Admin123!

# JWT Configuration
JWT_SECRET_KEY=tu-clave-secreta-muy-larga-y-segura-aqui-minimo-32-caracteres
JWT_ISSUER=UberEatsBackend
JWT_AUDIENCE=UberEatsApp
JWT_EXPIRATION_MINUTES=60

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# CORS Configuration (para desarrollo local)
CORS_ORIGINS=http://localhost:3000,http://localhost:5173
```

### 4. 📦 Restaurar Dependencias

```bash
cd UberEatsBackend
dotnet restore
```

### 5. 🗄️ Configurar Entity Framework y Migraciones

#### a) Verificar que EF Tools esté instalado:

```bash
dotnet tool install --global dotnet-ef
# O si ya está instalado, actualizarlo:
dotnet tool update --global dotnet-ef
```

#### b) Crear la primera migración (si no existe):

```bash
dotnet ef migrations add InitialCreate
```

#### c) Aplicar las migraciones a la base de datos:

```bash
dotnet ef database update
```

### 6. 🚀 Ejecutar la Aplicación

```bash
dotnet run
```

O en modo desarrollo con hot reload:

```bash
dotnet watch run
```

### 7. ✅ Verificar que Todo Funciona

La aplicación debería estar ejecutándose en:
- **API**: http://localhost:8080/api
- **Swagger UI**: http://localhost:8080/swagger

## 🔧 Comandos Útiles para el Desarrollo

### Base de Datos y Migraciones

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreMigracionAnterior

# Eliminar la última migración (solo si no se ha aplicado)
dotnet ef migrations remove

# Ver el estado de las migraciones
dotnet ef migrations list
```

### Docker

```bash
# Ver contenedores en ejecución
docker ps

# Parar el contenedor de PostgreSQL
docker stop ubereats-postgres

# Iniciar el contenedor de PostgreSQL
docker start ubereats-postgres

# Ver logs del contenedor
docker logs ubereats-postgres

# Conectarse a la base de datos desde la línea de comandos
docker exec -it ubereats-postgres psql -U postgres -d ubereats_db
```

### Desarrollo

```bash
# Ejecutar con hot reload
dotnet watch run

# Limpiar y rebuilder
dotnet clean && dotnet build

# Ejecutar tests (si existen)
dotnet test
```

## 🐛 Solución de Problemas Comunes

### Error de Conexión a la Base de Datos

1. Verifica que Docker Desktop esté ejecutándose
2. Confirma que el contenedor de PostgreSQL esté running: `docker ps`
3. Verifica la cadena de conexión en tu archivo `.env`

### Error en las Migraciones

```bash
# Si las migraciones fallan, intenta:
dotnet ef database drop --force
dotnet ef database update
```

### Puerto 5432 ya en uso

```bash
# Cambiar el puerto en docker-compose.yml o en el comando docker run
# Por ejemplo, usar el puerto 5433:
-p 5433:5432
# Y actualizar la cadena de conexión: Port=5433
```

### Problemas de CORS

Asegúrate de que las URLs en `CORS_ORIGINS` coincidan con la URL de tu frontend.

## 📱 Probar la API

Una vez que la aplicación esté ejecutándose, puedes:

1. **Usar Swagger UI**: Ve a http://localhost:8080/swagger
2. **Usar Postman/Insomnia**: Importa la colección de endpoints
3. **cURL ejemplo**:

```bash
# Registrar un usuario
curl -X POST "http://localhost:8080/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "phoneNumber": "612345678"
  }'
```

## 🔄 Workflow de Desarrollo Diario

1. **Iniciar Docker Desktop**
2. **Levantar la base de datos**: `docker start ubereats-postgres` (o `docker-compose up -d`)
3. **Aplicar migraciones nuevas** (si las hay): `dotnet ef database update`
4. **Ejecutar la aplicación**: `dotnet watch run`
5. **Desarrollar** 🎉

## 📊 Datos de Prueba (Seed Data)

Para poblar la base de datos con datos de prueba, puedes ejecutar:

```bash
# Si tienes configurado un seeder
dotnet run --seed
```

O crear manualmente algunos registros usando Swagger UI o Postman.

---

¡Con estos pasos deberías tener tu entorno de desarrollo local completamente funcional! 🚀
