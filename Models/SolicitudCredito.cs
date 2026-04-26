using Examen_Parcial_Plataforma_de_Cr_ditos.Models;  // ← Agrega esta línea

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Models
{
    public class SolicitudCredito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public decimal MontoSolicitado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; } // Pendiente, Aprobado, Rechazado
        public string? MotivoRechazo { get; set; } // ← Agrega esta propiedad
        
        // Propiedad de navegación
        public Cliente Cliente { get; set; }
    }
}