using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<SolicitudCredito> SolicitudesCredito { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar las tablas manualmente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UsuarioId).IsRequired();
                entity.Property(e => e.IngresosMensuales).IsRequired();
                entity.Property(e => e.Activo).IsRequired();
            });
            
            modelBuilder.Entity<SolicitudCredito>(entity =>
            {
                entity.ToTable("SolicitudesCredito");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontoSolicitado).IsRequired();
                entity.Property(e => e.FechaSolicitud).IsRequired();
                entity.Property(e => e.Estado).IsRequired();
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.SolicitudesCredito)
                    .HasForeignKey(e => e.ClienteId);
            });
              // Agregar restricciones después de crear las tablas
    modelBuilder.Entity<Cliente>()
        .HasCheckConstraint("CK_Cliente_IngresosMensuales", "IngresosMensuales > 0");
        
    modelBuilder.Entity<SolicitudCredito>()
        .HasCheckConstraint("CK_Solicitud_MontoSolicitado", "MontoSolicitado > 0");
        }
    }
}