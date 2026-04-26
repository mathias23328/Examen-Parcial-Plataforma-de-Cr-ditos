using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Data;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AnalistaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Panel de Analista - Lista de solicitudes pendientes
    public async Task<IActionResult> Panel()
    {
        var solicitudesPendientes = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .ThenInclude(c => c.Usuario)
            .Where(s => s.Estado == "Pendiente")
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();

        return View(solicitudesPendientes);
    }

    // POST: Aprobar solicitud
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
        {
            TempData["Error"] = "Solicitud no encontrada";
            return RedirectToAction("Panel");
        }

        // Validación: No procesar solicitudes ya aprobadas o rechazadas
        if (solicitud.Estado != "Pendiente")
        {
            TempData["Error"] = $"Esta solicitud ya fue {solicitud.Estado.ToLower()}";
            return RedirectToAction("Panel");
        }

        // Validación: No aprobar si el monto excede 5 veces los ingresos
        if (solicitud.MontoSolicitado > solicitud.Cliente.IngresosMensuales * 5)
        {
            TempData["Error"] = $"No se puede aprobar. El monto {solicitud.MontoSolicitado:C} excede 5 veces los ingresos mensuales ({solicitud.Cliente.IngresosMensuales * 5:C})";
            return RedirectToAction("Panel");
        }

        solicitud.Estado = "Aprobado";
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Solicitud #{solicitud.Id} aprobada exitosamente";
        return RedirectToAction("Panel");
    }

    // GET: Mostrar formulario de rechazo
    [HttpGet]
    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await _context.SolicitudesCredito
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
        {
            TempData["Error"] = "Solicitud no encontrada";
            return RedirectToAction("Panel");
        }

        if (solicitud.Estado != "Pendiente")
        {
            TempData["Error"] = $"Esta solicitud ya fue {solicitud.Estado.ToLower()}";
            return RedirectToAction("Panel");
        }

        ViewBag.SolicitudId = id;
        ViewBag.Monto = solicitud.MontoSolicitado;
        return View();
    }

    // POST: Rechazar solicitud
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string motivo)
    {
        var solicitud = await _context.SolicitudesCredito
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
        {
            TempData["Error"] = "Solicitud no encontrada";
            return RedirectToAction("Panel");
        }

        // Validación: No procesar solicitudes ya aprobadas o rechazadas
        if (solicitud.Estado != "Pendiente")
        {
            TempData["Error"] = $"Esta solicitud ya fue {solicitud.Estado.ToLower()}";
            return RedirectToAction("Panel");
        }

        // Validación: Motivo obligatorio
        if (string.IsNullOrWhiteSpace(motivo))
        {
            ModelState.AddModelError("", "El motivo de rechazo es obligatorio");
            ViewBag.SolicitudId = id;
            ViewBag.Monto = solicitud.MontoSolicitado;
            return View();
        }

        solicitud.Estado = "Rechazado";
        solicitud.MotivoRechazo = motivo;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Solicitud #{solicitud.Id} rechazada exitosamente";
        return RedirectToAction("Panel");
    }
}