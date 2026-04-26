using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;  // ← Agrega esta línea

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            // Crear rol Analista
            if (!await roleManager.RoleExistsAsync("Analista"))
            {
                await roleManager.CreateAsync(new IdentityRole("Analista"));
            }

            // Crear usuarios de ejemplo
            var users = new[]
            {
                new { Email = "cliente1@test.com", Password = "Pass123!", Rol = "" },
                new { Email = "cliente2@test.com", Password = "Pass123!", Rol = "" },
                new { Email = "analista@test.com", Password = "Pass123!", Rol = "Analista" }
            };

            foreach (var userInfo in users)
            {
                var user = await userManager.FindByEmailAsync(userInfo.Email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = userInfo.Email, Email = userInfo.Email };
                    await userManager.CreateAsync(user, userInfo.Password);
                    
                    if (!string.IsNullOrEmpty(userInfo.Rol))
                    {
                        await userManager.AddToRoleAsync(user, userInfo.Rol);
                    }
                }
            }

            // Obtener usuarios creados
            var cliente1 = await userManager.FindByEmailAsync("cliente1@test.com");
            var cliente2 = await userManager.FindByEmailAsync("cliente2@test.com");

            // Crear clientes si no existen
            if (!context.Clientes.Any())
            {
                context.Clientes.AddRange(
                    new Cliente { UsuarioId = cliente1.Id, IngresosMensuales = 3000, Activo = true },
                    new Cliente { UsuarioId = cliente2.Id, IngresosMensuales = 5000, Activo = true }
                );
                await context.SaveChangesAsync();
            }

            // Crear solicitudes de ejemplo
            if (!context.SolicitudesCredito.Any())
            {
                var clientes = await context.Clientes.ToListAsync();

                context.SolicitudesCredito.AddRange(
                    new SolicitudCredito 
                    { 
                        ClienteId = clientes[0].Id, 
                        MontoSolicitado = 5000, 
                        FechaSolicitud = DateTime.Now, 
                        Estado = "Pendiente",
                        MotivoRechazo = null
                    },
                    new SolicitudCredito 
                    { 
                        ClienteId = clientes[1].Id, 
                        MontoSolicitado = 10000, 
                        FechaSolicitud = DateTime.Now.AddDays(-5), 
                        Estado = "Aprobado",
                        MotivoRechazo = null
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}