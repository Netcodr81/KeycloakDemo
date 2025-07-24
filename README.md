# KeycloakDemo

A comprehensive demonstration of Keycloak integration across multiple web technologies including Angular, ASP.NET Core MVC, Blazor, and ASP.NET Framework MVC.

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Environment Setup](#environment-setup)
- [Running the Applications](#running-the-applications)
- [Keycloak Configuration](#keycloak-configuration)
- [Authentication Features](#authentication-features)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

## 🎯 Overview

This repository demonstrates secure authentication and authorization using Keycloak across different web technologies:

- **Angular 19** - Modern SPA with keycloak-angular integration
- **ASP.NET Core 9.0 MVC** - Server-side web application with Keycloak.AuthServices
- **Blazor Server** - Interactive web UI with Keycloak integration
- **ASP.NET Framework MVC** - Legacy framework with OWIN Keycloak integration

All applications connect to a shared Keycloak instance running in Docker, showcasing SSO (Single Sign-On) capabilities.

## 🔧 Prerequisites

Before getting started, ensure you have the following installed:

### Required Software
- **Git** - For cloning the repository
- **Docker Desktop** - For running Keycloak
- **Node.js** (v18 or later) - For Angular application
- **.NET 9.0 SDK** - For ASP.NET Core applications
- **.NET Framework 4.8** - For ASP.NET Framework application
- **Visual Studio Code** or **Visual Studio** - IDE for development

### Optional Tools
- **Angular CLI** - `npm install -g @angular/cli`
- **PowerShell** - For running scripts (Windows)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Netcodr81/KeycloakDemo.git
cd KeycloakDemo
```

### 2. Start Keycloak Server

Start the Keycloak server using Docker Compose:

```bash
docker-compose up -d
```

Wait for Keycloak to fully start (usually takes 1-2 minutes). You can verify it's running by visiting:
- **Keycloak Admin Console**: http://localhost:8080
- **Credentials**: admin / admin

### 3. Install Dependencies

Navigate to each project directory and install dependencies:

#### Angular Application
```bash
cd src/Angular.Web
npm install
```

#### ASP.NET Core MVC
```bash
cd src/MVC.Web
dotnet restore
```

#### Blazor Application
```bash
cd src/Blazor.Web
dotnet restore
```

#### ASP.NET Framework MVC
Open `KeycloakDemo.sln` in Visual Studio and restore NuGet packages.

## 📁 Project Structure

```
KeycloakDemo/
├── .containers/           # Docker container data
├── .files/               # Keycloak realm import files
├── docker-compose.yml    # Keycloak container configuration
├── KeycloakDemo.sln     # Visual Studio solution file
└── src/
    ├── Angular.Web/          # Angular 19 SPA
    │   ├── src/app/
    │   │   ├── auth.guard.ts        # Route protection
    │   │   ├── app.config.ts        # Keycloak configuration
    │   │   ├── user-info/           # User information components
    │   │   └── user-roles/          # Role management
    │   └── package.json
    ├── MVC.Web/              # ASP.NET Core 9.0 MVC
    │   ├── Controllers/
    │   ├── Authentication/
    │   └── MVC.Web.csproj
    ├── Blazor.Web/           # Blazor Server Application
    │   ├── Components/
    │   ├── Authentication/
    │   └── Blazor.Web.csproj
    └── MVC.Framework.Web/    # ASP.NET Framework MVC
        ├── Controllers/
        ├── Authentication/
        └── MVC.Framework.Web.csproj
```

## 🛠️ Environment Setup

### Keycloak Configuration

The repository includes pre-configured Keycloak realm settings. The following are automatically configured:

- **Realm**: `keycloak_demo`
- **Clients**:
  - `angular_client` - For Angular SPA
  - `mvc_client` - For ASP.NET Core MVC
  - `blazor_client` - For Blazor application
  - `mvc_framework_client` - For ASP.NET Framework
- **Roles**:
  - `Angular_Client_Admin`
  - `Angular_Client_User`
- **Test Users**: Configured with appropriate roles

### Application Configuration

Each application is pre-configured to connect to the local Keycloak instance:

#### Angular (src/Angular.Web/src/app/app.config.ts)
```typescript
config: {
  url: 'http://localhost:8080',
  realm: 'keycloak_demo',
  clientId: 'angular_client'
}
```

#### ASP.NET Core (appsettings.json)
```json
{
  "Keycloak": {
    "AuthServerUrl": "http://localhost:8080",
    "Realm": "keycloak_demo",
    "ClientId": "mvc_client"
  }
}
```

## 🏃‍♂️ Running the Applications

### 1. Start Keycloak (if not already running)
```bash
docker-compose up -d
```

### 2. Run Applications

#### Angular Application
```bash
cd src/Angular.Web
npm start
# or
ng serve
```
**URL**: http://localhost:4200

#### ASP.NET Core MVC
```bash
cd src/MVC.Web
dotnet run
```
**URL**: https://localhost:5001

#### Blazor Application
```bash
cd src/Blazor.Web
dotnet run
```
**URL**: https://localhost:5003

#### ASP.NET Framework MVC
Open `KeycloakDemo.sln` in Visual Studio and run the `MVC.Framework.Web` project.

## 🔐 Keycloak Configuration

### Admin Access
- **URL**: http://localhost:8080
- **Username**: admin
- **Password**: admin

### Realm Configuration
The `keycloak_demo` realm includes:

#### Clients Configuration
| Client ID | Type | Access Type | Valid Redirect URIs |
|-----------|------|-------------|-------------------|
| angular_client | Public | SPA | http://localhost:4200/* |
| mvc_client | Confidential | Web App | https://localhost:5001/* |
| blazor_client | Confidential | Web App | https://localhost:5003/* |
| mvc_framework_client | Confidential | Web App | http://localhost:* |

#### Roles
- **Client Roles**: Specific to each application
- **Realm Roles**: Shared across applications

#### Users
Test users are pre-configured with different role assignments.

## ✨ Authentication Features

### Angular Application Features
- **Route Protection**: Role-based access control
- **JWT Token Display**: View parsed token claims
- **User Information**: Display user profile data
- **Role Management**: Show user roles and permissions
- **Silent SSO**: Seamless authentication experience

### ASP.NET Core Features
- **Authorization Policies**: Role and claim-based authorization
- **User Context**: Access user information in controllers
- **Secure APIs**: Protected API endpoints
- **Logout Handling**: Proper session termination

### Blazor Features
- **Component Protection**: Secure Blazor components
- **User State**: Real-time user authentication state
- **Interactive UI**: Dynamic content based on user roles

### Framework MVC Features
- **OWIN Integration**: Keycloak authentication middleware
- **Action Filters**: Method-level security
- **Claims Principal**: Access user claims

## 🔧 Development Configuration

### VS Code Launch Configuration
The repository includes VS Code configuration for debugging:

```json
{
  "type": "msedge",
  "request": "launch",
  "name": "Launch Edge against localhost",
  "url": "http://localhost:4200",
  "webRoot": "${workspaceFolder}/src/Angular.Web/src"
}
```

### Angular Development
- **Hot Reload**: Enabled for development
- **Source Maps**: Configured for debugging
- **ESLint**: Code quality enforcement
- **TailwindCSS**: Styling framework with DaisyUI components

## 🐛 Troubleshooting

### Common Issues

#### 1. Keycloak Not Starting
```bash
# Check container status
docker ps

# View logs
docker-compose logs keycloak

# Restart container
docker-compose restart keycloak
```

#### 2. CORS Issues
Ensure Keycloak clients have proper CORS configuration:
- Valid Redirect URIs must include your application URLs
- Web Origins should include your application domains

#### 3. Angular Debugging Issues
- Ensure source maps are enabled in `tsconfig.json`
- Verify launch.json webRoot path is correct
- Check that the Angular dev server is running on the expected port

#### 4. Authentication Redirect Loops
- Verify client configuration in Keycloak
- Check that redirect URIs match exactly
- Ensure proper client type (Public vs Confidential)

#### 5. Role-based Access Issues
- Verify role mappings in Keycloak
- Check client role vs realm role configuration
- Ensure user has appropriate role assignments

### Useful Commands

```bash
# Restart all containers
docker-compose restart

# View container logs
docker-compose logs -f

# Stop all containers
docker-compose down

# Rebuild and start
docker-compose up --build -d

# Clean Angular node_modules
cd src/Angular.Web && rm -rf node_modules && npm install

# Clean .NET build artifacts
dotnet clean && dotnet restore
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Additional Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [keycloak-angular Library](https://github.com/mauriciovigolo/keycloak-angular)
- [Keycloak.AuthServices](https://github.com/NikiforovAll/keycloak-authorization-services-dotnet)
- [Angular Security Guide](https://angular.io/guide/security)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

---

**Note**: This is a demonstration project for educational purposes. For production use, ensure proper security configurations, use HTTPS, and follow security best practices.