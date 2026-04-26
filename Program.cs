using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurar el contexto de la base de datos con SqlLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configuración de Identity para la autenticación de usuarios
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Agregar controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();  // Usa el punto de migración si estamos en desarrollo
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Forzar HSTS en producción
}

app.UseHttpsRedirection();  // Redirigir tráfico HTTP a HTTPS
app.UseStaticFiles();  // Usar archivos estáticos (CSS, JS, imágenes, etc.)

app.UseRouting();  // Configurar enrutamiento

app.UseAuthorization();  // Habilitar autorización

// Configurar la ruta predeterminada para las vistas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();  // Configurar Razor Pages

app.Run();  // Ejecutar la aplicación