# 🚀 UberEatsBackend API

<div align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9" />
  <img src="https://img.shields.io/badge/EF_Core-9.0-00C58E?style=for-the-badge" alt="EF Core" />
  <img src="https://img.shields.io/badge/PostgreSQL-AWS-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" alt="JWT" />
  <img src="https://img.shields.io/badge/REST-API-FF6C37?style=for-the-badge&logo=swagger&logoColor=white" alt="REST API" />
</div>

<p align="center">
  <b>Potente backend para una plataforma de delivery de comida inspirada en UberEats | TFG Proyecto</b>
</p>

Este proyecto implementa una API robusta y escalable para una plataforma de entrega de comida, construida con tecnología .NET 9 y siguiendo los patrones de arquitectura modernos. Se integra con un frontend creado con Vue 3, TypeScript y Tailwind CSS para ofrecer una experiencia de usuario fluida y responsive.

## ✨ Características Principales

- **Autenticación Segura**: Sistema JWT con roles y permisos granulares
- **Gestión de Restaurantes**: Catálogo completo con menús, categorías y productos
- **Procesamiento de Pedidos**: Flujo completo desde creación hasta entrega
- **Integración de Pagos**: Preparado para conectar con pasarelas de pago
- **Diseño Mobile-First**: API optimizada para aplicaciones móviles
- **Base de Datos en AWS**: PostgreSQL alojado en la nube para alta disponibilidad
- **Arquitectura Avanzada**: Patrón repositorio y separación de responsabilidades
- **Seguridad Robusta**: Protección contra vulnerabilidades comunes (XSS, CSRF, inyección SQL)

## 🏗️ Arquitectura

```
UberEatsBackend/
├── 🎮 Controllers/       # Endpoints de la API REST
├── 📦 Models/            # Entidades del dominio
├── 📋 DTOs/              # Objetos de transferencia de datos
├── ⚙️ Services/          # Lógica de negocio
├── 🗃️ Repositories/      # Capa de acceso a datos
├── 💾 Data/              # Contexto y configuraciones de BD
├── 🔌 Middleware/        # Componentes de middleware personalizados
├── 🛠️ Utils/             # Utilidades y helpers
└── 📝 .env               # Variables de entorno (desarrollo local)
```

## 🔧 Stack Tecnológico

**Backend:**
- **ASP.NET Core 8**: Marco de trabajo moderno para APIs de alto rendimiento
- **Entity Framework Core**: ORM para operaciones de base de datos
- **PostgreSQL**: Base de datos robusta alojada en AWS
- **JWT Authentication**: Mecanismo seguro de autenticación basado en tokens
- **Swagger/OpenAPI**: Documentación interactiva de la API
- **CORS configurado**: Integración segura con el frontend
- **Manejo de errores centralizado**: Respuestas consistentes en toda la API
- **Variables de entorno**: Configuración segura entre entornos

**Frontend asociado:**
- Vue 3 (Composition API)
- TypeScript
- Tailwind CSS
- Metodología BEM para CSS

## 🚦 Cómo Empezar

### Requisitos Previos

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) o superior
- Acceso a PostgreSQL (local o en AWS)
- IDE recomendado: Visual Studio 2022/VS Code/JetBrains Rider

### Configuración Inicial

1. **Clonar el repositorio**

```bash
git clone https://github.com/tuusuario/UberEatsBackend.git
cd UberEatsBackend
```

2. **Configurar variables de entorno**

Copia `.env.example` a `.env` y ajusta los valores:

```bash
cp .env.example .env
# Edita el archivo .env con tus credenciales y configuración
```

3. **Aplicar migraciones de base de datos**

```bash
dotnet ef database update
```

4. **Ejecutar la aplicación**

```bash
dotnet run
```

La API estará disponible en `https://api.elixiumfoods/api` y Swagger en `https://localhost:5290/swagger`

## 📊 Modelo de Datos

![image](https://github.com/user-attachments/assets/b4434355-c2d8-4655-a93c-4e8eb05ffe3d)


## 🔒 Autenticación y Autorización

El sistema implementa autenticación JWT completa:

1. **Registro**: `POST /api/Auth/register`
   ```json
   {
     "email": "usuario@example.com",
     "password": "Contraseña123!",
     "firstName": "Nombre",
     "lastName": "Apellido",
     "phoneNumber": "612345678"
   }
   ```

2. **Login**: `POST /api/Auth/login`
   ```json
   {
     "email": "usuario@example.com",
     "password": "Contraseña123!"
   }
   ```

3. El servidor devuelve un token JWT que debe incluirse en el encabezado `Authorization: Bearer {token}` para acceder a endpoints protegidos.

## 📱 Ejemplos de Uso de la API

### Crear un nuevo restaurante
```http
POST /api/Restaurants
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Burger Deluxe",
  "description": "Las mejores hamburguesas de la ciudad",
  "logoUrl": "https://example.com/logo.png",
  "isOpen": true,
  "deliveryFee": 2.50,
  "estimatedDeliveryTime": 30,
  "address": {
    "street": "Calle Principal 123",
    "city": "Madrid",
    "state": "Madrid",
    "zipCode": "28001"
  }
}
```

### Realizar un pedido
```http
POST /api/Orders
Content-Type: application/json
Authorization: Bearer {token}

{
  "restaurantId": 1,
  "deliveryAddressId": 3,
  "items": [
    {
      "productId": 5,
      "quantity": 2
    },
    {
      "productId": 8,
      "quantity": 1
    }
  ],
  "paymentMethod": "card"
}
```

## 🚢 Despliegue

El backend está diseñado para ser desplegado en:
- Contenedores Docker
- AWS Elastic Beanstalk
- Azure App Service
- Kubernetes

## 📋 Mejoras Futuras

- Implementación de notificaciones en tiempo real con SignalR
- Integración con servicios de mapas para seguimiento de entregas
- Sistema de calificación y reseñas
- Optimización de consultas para mayor rendimiento
- Implementación de caché distribuida

## 📄 Licencia

Este proyecto forma parte de un Trabajo de Fin de Grado (TFG) y está sujeto a las directrices académicas correspondientes.

---

<div align="center">
  <b>Desarrollado con ❤️ por:</b><br>
  <b>Francisco Villa</b>:
  <a href="https://github.com/Franvilla03">GitHub</a> •
  <a href="https://www.linkedin.com/in/francisco-villa-cabero-640734239/">LinkedIn</a>
  <br>
  <b>Gabriel Saiz</b>:
  <a href="mailto:gsaiz.bajo@gmail.com">Email</a> •
  <a href="https://github.com/GabriLPDA22">GitHub</a> •
  <a href="https://www.linkedin.com/in/gabriel-saiz-de-la-maza-bajo-140370184/">LinkedIn</a>
</div>
