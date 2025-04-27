# ELIXIUM-FOODS-BACK
# 🍽️ UberEatsBackend 🚚

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-brightgreen)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14.0-blue)
![JWT](https://img.shields.io/badge/JWT-Authentication-orange)
![License](https://img.shields.io/badge/license-MIT-green)

A robust, scalable backend service for a food delivery platform built with .NET 8 and modern architecture patterns. This project serves as the backend counterpart to a Vue.js + TypeScript + Tailwind CSS frontend, forming a complete food delivery application.

## 🚀 Features

- **🔐 JWT Authentication & Authorization**: Secure user management with role-based access control
- **🏪 Complete Restaurant Management**: Manage restaurants, menus, and products
- **🛒 Order Processing System**: Full order lifecycle from creation to delivery
- **💳 Payment Processing Integration**: Ready for payment gateway implementation
- **📱 Mobile-First API Design**: Optimized for mobile applications
- **🗄️ PostgreSQL Database**: Robust data persistence with AWS hosting
- **🔄 Repository Pattern**: Clean separation of concerns
- **🔌 RESTful API**: Well-designed API endpoints following REST principles

## 📋 Architecture

The application follows a clean architecture pattern with:

```
UberEatsBackend/
├── Controllers/       # API endpoints
├── Models/            # Domain entities
├── DTOs/              # Data Transfer Objects
├── Services/          # Business logic
├── Repositories/      # Data access layer
├── Data/              # Database context and configurations
├── Middleware/        # Custom middleware components
└── Utils/             # Helper utilities
```

## 🔧 Technologies

- **ASP.NET Core 8**: Latest framework for building high-performance APIs
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Robust, open-source database
- **JWT Authentication**: Secure token-based authentication
- **Swagger/OpenAPI**: API documentation and testing
- **CORS Support**: Configured for frontend integration
- **Error Handling Middleware**: Consistent error responses
- **Logging System**: Comprehensive activity tracking

## 🛠️ Installation & Setup

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- [PostgreSQL](https://www.postgresql.org/download/) (or access to an AWS PostgreSQL instance)
- IDE (recommended: Visual Studio 2022 or JetBrains Rider)

### Getting Started

1. **Clone the repository**

```bash
git clone https://github.com/yourusername/UberEatsBackend.git
cd UberEatsBackend
```

2. **Update database connection string**

Edit `appsettings.json` with your PostgreSQL connection details:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=your-aws-postgres-instance.amazonaws.com;Database=ubereats;Username=your_username;Password=your_password"
}
```

3. **Apply database migrations**

```bash
dotnet ef database update
```

4. **Run the application**

```bash
dotnet run
```

The API will be available at `https://localhost:7264` and `http://localhost:5264` by default.

## 📝 API Documentation

Once the application is running, access the Swagger UI documentation at:
```
https://localhost:7264/swagger
```

## 🔄 Front-End Integration

This backend is designed to pair with a Vue.js front-end application using:
- Vue 3 with Composition API
- TypeScript for type safety
- Tailwind CSS for styling
- BEM naming convention for CSS components

CORS is configured to allow requests from the Vue development server.

## 🛡️ Authentication

The API uses JWT tokens for authentication. To access protected endpoints:

1. Register a user via `/api/Auth/register`
2. Login via `/api/Auth/login` to receive a token
3. Include the token in the Authorization header as a Bearer token

## 🧪 Testing

Run the test suite with:

```bash
dotnet test
```

## 📦 Deployment

The application is designed to be deployed to:
- AWS EC2 instances
- Azure App Service
- Docker containers
- Any platform supporting .NET 8 applications

## 📚 Entity Relationship Diagram

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│    Users    │     │  Restaurants │     │   Menus     │
├─────────────┤     ├──────────────┤     ├─────────────┤
│ Id          │     │ Id           │     │ Id          │
│ Email       │     │ Name         │     │ Name        │
│ PasswordHash│     │ Description  │     │ Description │
│ FirstName   │     │ LogoUrl      │     │ RestaurantId│
│ LastName    │◄────┤ UserId       │◄────┤             │
│ PhoneNumber │     │ AddressId    │     │             │
│ Role        │     │ IsOpen       │     │             │
└─────────────┘     └──────────────┘     └─────┬───────┘
       ▲                    ▲                   │
       │                    │                   │
       │                    │                   ▼
┌─────────────┐     ┌──────┴───────┐     ┌─────────────┐
│  Addresses  │     │    Orders    │     │  Categories │
├─────────────┤     ├──────────────┤     ├─────────────┤
│ Id          │     │ Id           │     │ Id          │
│ Street      │     │ UserId       │     │ Name        │
│ City        │     │ RestaurantId │     │ Description │
│ State       │◄────┤ AddressId    │     │ MenuId      │
│ ZipCode     │     │ Status       │     │             │
│ UserId      │     │ Total        │     │             │
└─────────────┘     └──────┬───────┘     └─────┬───────┘
                           │                   │
                           ▼                   ▼
                    ┌──────────────┐     ┌─────────────┐
                    │  OrderItems  │     │  Products   │
                    ├──────────────┤     ├─────────────┤
                    │ Id           │     │ Id          │
                    │ OrderId      │◄────┤ Name        │
                    │ ProductId    │     │ Description │
                    │ Quantity     │     │ Price       │
                    │ UnitPrice    │     │ ImageUrl    │
                    │              │     │ CategoryId  │
                    └──────────────┘     └─────────────┘
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📧 Contact

Your Name - [your.email@example.com](mailto:your.email@example.com)

Project Link: [https://github.com/yourusername/UberEatsBackend](https://github.com/yourusername/UberEatsBackend)

---

⭐ Star this repository if you find it helpful! ⭐