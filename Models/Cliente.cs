using Microsoft.AspNetCore.Identity;  // ← Agrega esta línea

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public decimal IngresosMensuales { get; set; }
        public bool Activo { get; set; }  // ← Te faltaba esta propiedad
        
        // Propiedades de navegación
        public IdentityUser Usuario { get; set; }
        public ICollection<SolicitudCredito> SolicitudesCredito { get; set; }
    }
}