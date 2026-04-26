// Ruta: /Models/SolicitudCredito.cs
namespace Examen_Parcial_Plataforma_de_Cr_ditos.Models
{
    public class SolicitudCredito
    {
        public int Id { get; set; }             // Identificador único de la solicitud
        public int ClienteId { get; set; }      // Relación con el cliente que hace la solicitud
        public decimal MontoSolicitado { get; set; }  // Monto solicitado en crédito
        public DateTime FechaSolicitud { get; set; }  // Fecha en la que se realizó la solicitud
        public string Estado { get; set; }      // Estado de la solicitud (Pendiente, Aprobado, Rechazado)
        public string MotivoRechazo { get; set; }  // Motivo del rechazo, si la solicitud fue rechazada
    }
}