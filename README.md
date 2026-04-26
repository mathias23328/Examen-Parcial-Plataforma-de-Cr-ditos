# 🏦 Plataforma de Gestión de Solicitudes de Crédito

## 🌐 Ver la aplicación en línea
🔗 **URL:** https://examen-parcial-plataforma-de-cr-ditos-1.onrender.com

---

## 🛠️ Tecnologías utilizadas
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core + SQLite
- ASP.NET Core Identity (Autenticación y roles)
- Redis (Sesiones y caché)
- Bootstrap 5
- Render.com (Despliegue)

---

## 📂 Estructura del proyecto

Examen-Parcial-Plataforma-de-Cr-ditos/
├── Controllers/
│ ├── SolicitudController.cs # Mis Solicitudes, Crear, Cancelar
│ ├── AnalistaController.cs # Panel, Aprobar, Rechazar
│ └── HomeController.cs # Inicio, Ayuda
├── Models/
│ ├── Cliente.cs
│ └── SolicitudCredito.cs
├── Views/
│ ├── Solicitud/
│ │ ├── MisSolicitudes.cshtml
│ │ ├── Crear.cshtml
│ │ └── Detalle.cshtml
│ ├── Analista/
│ │ └── Panel.cshtml
│ └── Home/
│ ├── Index.cshtml
│ └── Ayuda.cshtml
├── Data/
│ ├── ApplicationDbContext.cs
│ └── DbInitializer.cs
├── Program.cs
├── appsettings.json
├── Dockerfile
└── README.md
---

## 🔐 Credenciales de prueba

| Rol | Correo electrónico | Contraseña |
|-----|-------------------|------------|
| 👤 **Cliente** | `cliente1@test.com` | `Pass123!` |
| 👤 **Cliente** | `cliente2@test.com` | `Pass123!` |
| 📊 **Analista** | `analista@test.com` | `Pass123!` |

---

## 💻 Comandos para ejecutar localmente

### 1. Clonar el repositorio
```bash
git clone https://github.com/mathias23328/Examen-Parcial-Plataforma-de-Cr-ditos.git
cd Examen-Parcial-Plataforma-de-Cr-ditos
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
http://localhost:5299
