using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Data;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.MaxDepth = 64;
});
// ========== CONFIGURACIÓN DE CACHÉ Y SESIONES ==========
// Usar SOLO UNO: Redis (producción) o Memoria (desarrollo)
// Para desarrollo local, usa MEMORIA:
builder.Services.AddDistributedMemoryCache();  // ← Usa esto para desarrollo sin Redis

// COMENTA Redis si no tienes Redis instalado
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
//     options.InstanceName = "CreditosApp_";
// });

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ========== FIN CONFIGURACIÓN ==========

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Inicializar datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    context.Database.EnsureCreated();
    await DbInitializer.InitializeAsync(context, userManager, roleManager);  // ← Agregar await
}

app.Run();