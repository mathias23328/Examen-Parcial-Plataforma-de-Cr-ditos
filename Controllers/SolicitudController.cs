using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial_Plataforma_de_Cr_ditos.Data;
using Examen_Parcial_Plataforma_de_Cr_ditos.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Examen_Parcial_Plataforma_de_Cr_ditos.Controllers;

[Authorize]
public class SolicitudController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IDistributedCache _cache; 

    public SolicitudController(ApplicationDbContext context, UserManager<IdentityUser> userManager,IDistributedCache cache)
    {
        _context = context;
        _userManager = userManager;
         _cache = cache;
    }

   // GET: Mis Solicitudes con filtros
public async Task<IActionResult> MisSolicitudes(
    string estado,
    decimal? montoMin,
    decimal? montoMax,
    DateTime? fechaInicio,
    DateTime? fechaFin)
{
    // Validaciones server-side
    if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
    {
        ViewBag.Error = "La fecha inicio no puede ser mayor a la fecha fin";
        return View(new List<SolicitudCredito>());
    }

    if ((montoMin.HasValue && montoMin < 0) || (montoMax.HasValue && montoMax < 0))
    {
        ViewBag.Error = "Los montos no pueden ser negativos";
        return View(new List<SolicitudCredito>());
    }

    var user = await _userManager.GetUserAsync(User);
    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

    if (cliente == null)
    {
        ViewBag.Error = "Cliente no encontrado";
        return View(new List<SolicitudCredito>());
    }

    // Verificar si tiene solicitud pendiente
    var tienePendiente = await _context.SolicitudesCredito
        .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");
    ViewBag.TienePendiente = tienePendiente;

    // Datos desde base de datos
    var query = _context.SolicitudesCredito
        .Where(s => s.ClienteId == cliente.Id)
        .AsQueryable();

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

    var solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();

    // ========== SESIÓN (Redis) ==========
    // Guardar última solicitud visitada en SESIÓN
    if (solicitudes.Any())
    {
        var ultimaSolicitud = solicitudes.OrderByDescending(s => s.FechaSolicitud).First();
        HttpContext.Session.SetString("UltimaSolicitudId", ultimaSolicitud.Id.ToString());
        HttpContext.Session.SetString("UltimaSolicitudMonto", ultimaSolicitud.MontoSolicitado.ToString("C"));
    }
    // ========== FIN SESIÓN ==========

    ViewBag.Estado = estado;
    ViewBag.MontoMin = montoMin;
    ViewBag.MontoMax = montoMax;
    ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
    ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

    return View(solicitudes);
}
    // GET: Detalle de solicitud
    public async Task<IActionResult> Detalle(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

        if (cliente == null)
        {
            TempData["Error"] = "Cliente no encontrado";
            return RedirectToAction("MisSolicitudes");
        }

        var solicitud = await _context.SolicitudesCredito
            .FirstOrDefaultAsync(s => s.Id == id && s.ClienteId == cliente.Id);

        if (solicitud == null)
        {
            TempData["Error"] = "Solicitud no encontrada";
            return RedirectToAction("MisSolicitudes");
        }

        return View(solicitud);
    }

    // GET: Formulario para crear solicitud
  [HttpGet]
public async Task<IActionResult> Crear()
{
    var user = await _userManager.GetUserAsync(User);
    var cliente = await _context.Clientes
        .FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
    
    if (cliente == null)
    {
        TempData["Error"] = "No tienes un perfil de cliente asociado";
        return RedirectToAction("MisSolicitudes");
    }
    
    if (!cliente.Activo)
    {
        TempData["Error"] = "Tu cuenta de cliente está inactiva";
        return RedirectToAction("MisSolicitudes");
    }
    
    var solicitudPendiente = await _context.SolicitudesCredito
        .FirstOrDefaultAsync(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");
    
    if (solicitudPendiente != null)
    {
        // Alerta más amigable con enlace para cancelar
        TempData["Error"] = $"Ya tienes una solicitud pendiente de {solicitudPendiente.MontoSolicitado:C}. " +
                           $"Debes cancelarla antes de crear una nueva. " +
                           $"<a href='{Url.Action("MisSolicitudes", "Solicitud")}' class='alert-link'>Ver mis solicitudes</a> " +
                           $"o <a href='{Url.Action("Cancelar", "Solicitud", new { id = solicitudPendiente.Id })}' " +
                           $"class='alert-link' onclick='return confirm(\"¿Cancelar solicitud #{solicitudPendiente.Id}?\")'>Cancelar solicitud</a>";
        return RedirectToAction("MisSolicitudes");
    }
    
    return View();
}
    // POST: Procesar creación de solicitud
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(decimal monto)
    {
        var user = await _userManager.GetUserAsync(User);
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

        if (cliente == null)
        {
            ModelState.AddModelError("", "Cliente no encontrado");
            return View();
        }

        if (!cliente.Activo)
        {
            ModelState.AddModelError("", "El cliente no está activo");
            return View();
        }

        var tienePendiente = await _context.SolicitudesCredito
            .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");

        if (tienePendiente)
        {
            ModelState.AddModelError("", "Ya existe una solicitud pendiente para este cliente");
            return View();
        }

        if (monto <= 0)
        {
            ModelState.AddModelError("monto", "El monto debe ser mayor a 0");
            return View();
        }

        var maximoPermitido = cliente.IngresosMensuales * 10;
        if (monto > maximoPermitido)
        {
            ModelState.AddModelError("monto", $"El monto solicitado no puede superar 10 veces tus ingresos mensuales. Máximo permitido: {maximoPermitido:C}");
            return View();
        }

        if (ModelState.IsValid)
        {
            var solicitud = new SolicitudCredito
            {
                ClienteId = cliente.Id,
                MontoSolicitado = monto,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente",
                MotivoRechazo = null
            };

            _context.SolicitudesCredito.Add(solicitud);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Solicitud creada exitosamente! Será evaluada por un analista.";
            return RedirectToAction("MisSolicitudes");
        }

        return View();
    }
    // POST: Cancelar solicitud pendiente
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Cancelar(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
    
    if (cliente == null)
    {
        TempData["Error"] = "Cliente no encontrado";
        return RedirectToAction("MisSolicitudes");
    }
    
    var solicitud = await _context.SolicitudesCredito
        .FirstOrDefaultAsync(s => s.Id == id && s.ClienteId == cliente.Id && s.Estado == "Pendiente");
    
    if (solicitud == null)
    {
        TempData["Error"] = "Solicitud no encontrada o no se puede cancelar";
        return RedirectToAction("MisSolicitudes");
    }
    
    _context.SolicitudesCredito.Remove(solicitud);
    await _context.SaveChangesAsync();
    
    TempData["Success"] = "Solicitud cancelada exitosamente";
    return RedirectToAction("MisSolicitudes");
}
}