using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnviosRapidosGT.Data;
using EnviosRapidosGT.DTOs;
using EnviosRapidosGT.Models;
using EnviosRapidosGT.Services;

namespace EnviosRapidosGT.Controllers
{
    [ApiController]
    [Route("api/envios")]
    public class EnviosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly EnvioService _svc;

        public EnviosController(AppDbContext db, EnvioService svc)
        {
            _db = db;
            _svc = svc;
        }

        // ── POST api/envios ──────────────────────────────────────
        // HU-01: Registrar envío con tarifa automática
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EnvioRequest req)
        {
            var remitente    = await _db.Clientes.FindAsync(req.RemitenteId);
            var destinatario = await _db.Clientes.FindAsync(req.DestinatarioId);

            if (remitente is null)    return NotFound(new { mensaje = "Remitente no encontrado." });
            if (destinatario is null) return NotFound(new { mensaje = "Destinatario no encontrado." });

            // Generar código de rastreo secuencial del día
            string hoy = DateTime.UtcNow.ToString("yyyyMMdd");
            int secuencial = await _db.Envios
                .CountAsync(e => e.CodigoRastreo.StartsWith($"ENV-{hoy}-")) + 1;
            string codigo = _svc.GenerarCodigoRastreo(secuencial);

            // Calcular tarifa y descuento
            decimal tarifaBase = _svc.CalcularTarifa(req.PesoKg);
            bool descuentoAplicado = remitente.NitValido || destinatario.NitValido;
            decimal descuento  = descuentoAplicado ? Math.Round(tarifaBase * 0.05m, 2) : 0;
            decimal tarifaFinal = tarifaBase - descuento;

            var envio = new Envio
            {
                CodigoRastreo        = codigo,
                RemitenteId          = req.RemitenteId,
                DestinatarioId       = req.DestinatarioId,
                PesoKg               = req.PesoKg,
                DireccionEntrega     = req.DireccionEntrega,
                OficinaOrigen        = req.OficinaOrigen,
                TarifaBase           = tarifaBase,
                Descuento            = descuento,
                TarifaFinal          = tarifaFinal,
                DescuentoAplicado    = descuentoAplicado,
                Estado               = EstadoEnvio.Registrado,
                UltimaUbicacion      = req.OficinaOrigen
            };

            _db.Envios.Add(envio);
            await _db.SaveChangesAsync();

            // Registrar estado inicial en historial
            _db.HistorialEstados.Add(new HistorialEstado
            {
                EnvioId        = envio.Id,
                EstadoAnterior = EstadoEnvio.Registrado,
                EstadoNuevo    = EstadoEnvio.Registrado,
                Ubicacion      = req.OficinaOrigen,
                Notas          = "Envío registrado"
            });
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByCodigo),
                new { codigo = envio.CodigoRastreo },
                await BuildResponse(envio.Id));
        }

        // ── GET api/envios/{codigo} ──────────────────────────────
        // HU-02: Consulta pública por código de rastreo
        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var envio = await _db.Envios
                .Include(e => e.Remitente)
                .Include(e => e.Destinatario)
                .Include(e => e.Historial)
                .FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);

            if (envio is null) return NotFound(new { mensaje = "Código de rastreo no encontrado." });

            var historial = envio.Historial
                .OrderByDescending(h => h.Timestamp)
                .Select(MapHistorial).ToList();

            return Ok(new { envio = MapEnvioResponse(envio), historial });
        }

        // ── GET api/envios ───────────────────────────────────────
        // HU-07: Listado filtrable con paginación
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? estado,
            [FromQuery] string? oficinaOrigen,
            [FromQuery] string? oficinaActual,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 50)
        {
            var query = _db.Envios
                .Include(e => e.Remitente)
                .Include(e => e.Destinatario)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoEnvio>(estado, out var estadoEnum))
                query = query.Where(e => e.Estado == estadoEnum);

            if (!string.IsNullOrEmpty(oficinaOrigen))
                query = query.Where(e => e.OficinaOrigen == oficinaOrigen);

            if (!string.IsNullOrEmpty(oficinaActual))
                query = query.Where(e => e.UltimaUbicacion == oficinaActual);

            if (fechaDesde.HasValue)
                query = query.Where(e => e.FechaCreacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(e => e.FechaCreacion <= fechaHasta.Value);

            int total = await query.CountAsync();

            var envios = await query
                .OrderByDescending(e => e.FechaCreacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(e => new EnvioResumenResponse
                {
                    Id              = e.Id,
                    CodigoRastreo   = e.CodigoRastreo,
                    Estado          = e.Estado.ToString(),
                    Remitente       = e.Remitente.NombreCompleto,
                    Destinatario    = e.Destinatario.NombreCompleto,
                    PesoKg          = e.PesoKg,
                    TarifaFinal     = e.TarifaFinal,
                    UltimaUbicacion = e.UltimaUbicacion
                })
                .ToListAsync();

            return Ok(new PagedResponse<EnvioResumenResponse>
            {
                Total       = total,
                Pagina      = pagina,
                TamanoPagina = tamanoPagina,
                Data        = envios
            });
        }

        // ── PUT api/envios/{codigo}/estado ───────────────────────
        // HU-03: Actualizar estado con validación de transición
        [HttpPut("{codigo}/estado")]
        public async Task<IActionResult> ActualizarEstado(string codigo, [FromBody] ActualizarEstadoRequest req)
        {
            var envio = await _db.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);
            if (envio is null) return NotFound(new { mensaje = "Envío no encontrado." });

            if (!Enum.TryParse<EstadoEnvio>(req.EstadoNuevo, out var nuevoEstado))
                return BadRequest(new { mensaje = $"Estado '{req.EstadoNuevo}' no es válido." });

            if (!_svc.TransicionEsValida(envio.Estado, nuevoEstado))
                return BadRequest(new { mensaje = $"Transición de '{envio.Estado}' a '{nuevoEstado}' no está permitida." });

            var estadoAnterior = envio.Estado;
            envio.Estado = nuevoEstado;
            envio.UltimaUbicacion = req.Ubicacion;
            envio.FechaUltimaActualizacion = DateTime.UtcNow;

            _db.HistorialEstados.Add(new HistorialEstado
            {
                EnvioId        = envio.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo    = nuevoEstado,
                Ubicacion      = req.Ubicacion,
                Notas          = req.Notas
            });

            await _db.SaveChangesAsync();
            return Ok(await BuildResponse(envio.Id));
        }

        // ── POST api/envios/{codigo}/intento-fallido ─────────────
        // HU-04: Registrar intento fallido; al 3ro pasa a EnDevolucion automáticamente
        [HttpPost("{codigo}/intento-fallido")]
        public async Task<IActionResult> IntentoDeFallido(string codigo, [FromBody] IntentoDeFallidoRequest req)
        {
            var envio = await _db.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);
            if (envio is null) return NotFound(new { mensaje = "Envío no encontrado." });

            if (envio.Estado != EstadoEnvio.EnReparto)
                return BadRequest(new { mensaje = "Solo se pueden registrar intentos fallidos cuando el envío está en estado EnReparto." });

            if (envio.IntentosEntrega >= 3)
                return BadRequest(new { mensaje = "El envío ya alcanzó el máximo de intentos." });

            envio.IntentosEntrega++;
            envio.FechaUltimaActualizacion = DateTime.UtcNow;

            var estadoAnterior = envio.Estado;
            bool cambioAutomatico = false;
            string notas = req.Notas ?? $"Intento fallido #{envio.IntentosEntrega}";

            if (envio.IntentosEntrega >= 3)
            {
                envio.Estado = EstadoEnvio.EnDevolucion;
                envio.UltimaUbicacion = req.Ubicacion;
                cambioAutomatico = true;
                notas = "Máximo de intentos alcanzado — cambio automático a EnDevolucion";
            }

            _db.HistorialEstados.Add(new HistorialEstado
            {
                EnvioId        = envio.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo    = envio.Estado,
                Ubicacion      = req.Ubicacion,
                Notas          = notas,
                EsAutomatico   = cambioAutomatico
            });

            await _db.SaveChangesAsync();
            return Ok(await BuildResponse(envio.Id));
        }

        // ── POST api/envios/{codigo}/cancelar ────────────────────
        // HU-10: Cancelar envío solo si está en Registrado
        [HttpPost("{codigo}/cancelar")]
        public async Task<IActionResult> Cancelar(string codigo, [FromBody] CancelarEnvioRequest req)
        {
            var envio = await _db.Envios.FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);
            if (envio is null) return NotFound(new { mensaje = "Envío no encontrado." });

            if (envio.Estado != EstadoEnvio.Registrado)
                return Conflict(new { mensaje = $"No se puede cancelar un envío en estado '{envio.Estado}'." });

            var estadoAnterior = envio.Estado;
            envio.Estado = EstadoEnvio.Cancelado;
            envio.FechaUltimaActualizacion = DateTime.UtcNow;

            _db.HistorialEstados.Add(new HistorialEstado
            {
                EnvioId        = envio.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo    = EstadoEnvio.Cancelado,
                Ubicacion      = envio.UltimaUbicacion ?? envio.OficinaOrigen,
                Notas          = req.Motivo
            });

            await _db.SaveChangesAsync();
            return Ok(await BuildResponse(envio.Id));
        }

        // ── GET api/envios/reporte/devoluciones ──────────────────
        // HU-08: Reporte de envíos en devolución ordenado por intentos
        [HttpGet("reporte/devoluciones")]
        public async Task<IActionResult> ReporteDevoluciones(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            var query = _db.Envios
                .Include(e => e.Remitente)
                .Include(e => e.Destinatario)
                .Where(e => e.Estado == EstadoEnvio.EnDevolucion || e.Estado == EstadoEnvio.Devuelto);

            if (fechaDesde.HasValue) query = query.Where(e => e.FechaCreacion >= fechaDesde.Value);
            if (fechaHasta.HasValue) query = query.Where(e => e.FechaCreacion <= fechaHasta.Value);

            var envios = await query
                .OrderByDescending(e => e.IntentosEntrega)
                .ToListAsync();

            return Ok(new ReporteDevolucionResponse
            {
                Total             = envios.Count,
                PromedioIntentos  = envios.Count > 0 ? envios.Average(e => e.IntentosEntrega) : 0,
                Envios            = envios.Select(e => new EnvioDevolucionDetalle
                {
                    CodigoRastreo    = e.CodigoRastreo,
                    Remitente        = e.Remitente.NombreCompleto,
                    Destinatario     = e.Destinatario.NombreCompleto,
                    DireccionEntrega = e.DireccionEntrega,
                    Estado           = e.Estado.ToString(),
                    IntentosEntrega  = e.IntentosEntrega,
                    FechaCreacion    = e.FechaCreacion
                }).ToList()
            });
        }

        // ── Helpers privados ─────────────────────────────────────

        private async Task<object> BuildResponse(int envioId)
        {
            var envio = await _db.Envios
                .Include(e => e.Remitente)
                .Include(e => e.Destinatario)
                .Include(e => e.Historial)
                .FirstAsync(e => e.Id == envioId);

            return new
            {
                envio    = MapEnvioResponse(envio),
                historial = envio.Historial
                    .OrderByDescending(h => h.Timestamp)
                    .Select(MapHistorial).ToList()
            };
        }

        private static EnvioResponse MapEnvioResponse(Envio e) => new()
        {
            Id                      = e.Id,
            CodigoRastreo           = e.CodigoRastreo,
            Remitente               = MapCliente(e.Remitente),
            Destinatario            = MapCliente(e.Destinatario),
            PesoKg                  = e.PesoKg,
            DireccionEntrega        = e.DireccionEntrega,
            OficinaOrigen           = e.OficinaOrigen,
            TarifaBase              = e.TarifaBase,
            Descuento               = e.Descuento,
            TarifaFinal             = e.TarifaFinal,
            DescuentoAplicado       = e.DescuentoAplicado,
            Estado                  = e.Estado.ToString(),
            UltimaUbicacion         = e.UltimaUbicacion,
            IntentosEntrega         = e.IntentosEntrega,
            FechaCreacion           = e.FechaCreacion,
            FechaUltimaActualizacion = e.FechaUltimaActualizacion
        };

        private static ClienteResponse MapCliente(Cliente c) => new()
        {
            Id             = c.Id,
            NombreCompleto = c.NombreCompleto,
            Telefono       = c.Telefono,
            Direccion      = c.Direccion,
            Nit            = c.Nit,
            NitValido      = c.NitValido,
            FechaRegistro  = c.FechaRegistro
        };

        private static HistorialResponse MapHistorial(HistorialEstado h) => new()
        {
            Id             = h.Id,
            EstadoAnterior = h.EstadoAnterior.ToString(),
            EstadoNuevo    = h.EstadoNuevo.ToString(),
            Ubicacion      = h.Ubicacion,
            Timestamp      = h.Timestamp,
            Notas          = h.Notas,
            EsAutomatico   = h.EsAutomatico
        };
    }
}
