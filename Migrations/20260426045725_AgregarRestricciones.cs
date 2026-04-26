using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRestricciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Solicitud_MontoSolicitado",
                table: "SolicitudesCredito",
                sql: "MontoSolicitado > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cliente_IngresosMensuales",
                table: "Clientes",
                sql: "IngresosMensuales > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Solicitud_MontoSolicitado",
                table: "SolicitudesCredito");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cliente_IngresosMensuales",
                table: "Clientes");
        }
    }
}
