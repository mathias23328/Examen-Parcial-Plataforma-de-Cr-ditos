using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Data;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;


[Authorize]
public class SolicitudController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SolicitudController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MisSolicitudes(
        string estado,
        decimal? montoMin,
        decimal? montoMax,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var user = await _userManager.GetUserAsync(User);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

        if (cliente == null)
            return Problem("Cliente no encontrado");

        var query = _context.SolicitudesCredito
            .Where(s => s.ClienteId == cliente.Id)
            .AsQueryable();

        // Filtros
        if (!string.IsNullOrEmpty(estado))
            query = query.Where(s => s.Estado == estado);

        if (montoMin.HasValue && montoMin > 0)
            query = query.Where(s => s.MontoSolicitado >= montoMin);

        if (montoMax.HasValue && montoMax > 0)
            query = query.Where(s => s.MontoSolicitado <= montoMax);

        if (fechaInicio.HasValue)
            query = query.Where(s => s.FechaSolicitud >= fechaInicio);

        if (fechaFin.HasValue)
            query = query.Where(s => s.FechaSolicitud <= fechaFin);

        // Validaciones server-side
        ViewBag.Error = null;
        if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            ViewBag.Error = "La fecha inicio no puede ser mayor a la fecha fin";

        if ((montoMin.HasValue && montoMin < 0) || (montoMax.HasValue && montoMax < 0))
            ViewBag.Error = "Los montos no pueden ser negativos";

        var solicitudes = await query.ToListAsync();
        return View(solicitudes);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

        var solicitud = await _context.SolicitudesCredito
            .FirstOrDefaultAsync(s => s.Id == id && s.ClienteId == cliente.Id);

        if (solicitud == null)
            return NotFound();

        return View(solicitud);
    }
}