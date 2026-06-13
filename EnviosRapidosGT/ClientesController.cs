using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnviosRapidosGT.Data;
using EnviosRapidosGT.DTOs;
using EnviosRapidosGT.Models;
using EnviosRapidosGT.Services;

namespace EnviosRapidosGT.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly EnvioService _svc;

        public ClientesController(AppDbContext db, EnvioService svc)
        {
            _db = db;
            _svc = svc;
        }

        // GET api/clientes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _db.Clientes
                .Select(c => MapResponse(c))
                .ToListAsync();
            return Ok(clientes);
        }

        // GET api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { mensaje = "Cliente no encontrado." });

            var enviosRemitente = await _db.Envios
                .Where(e => e.RemitenteId == id)
                .Select(e => e.CodigoRastreo).ToListAsync();

            var enviosDestinatario = await _db.Envios
                .Where(e => e.DestinatarioId == id)
                .Select(e => e.CodigoRastreo).ToListAsync();

            return Ok(new
            {
                cliente = MapResponse(cliente),
                enviosComoRemitente = enviosRemitente,
                enviosComoDestinatario = enviosDestinatario
            });
        }

        // POST api/clientes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteRequest req)
        {
            bool nitValido = _svc.ValidarNit(req.Nit);

            // Verificar NIT duplicado
            if (nitValido && req.Nit != null)
            {
                bool existe = await _db.Clientes.AnyAsync(c => c.Nit == req.Nit && c.NitValido);
                if (existe)
                    return Conflict(new { mensaje = "Ya existe un cliente registrado con ese NIT." });
            }

            var cliente = new Cliente
            {
                NombreCompleto = req.NombreCompleto,
                Telefono       = req.Telefono,
                Direccion      = req.Direccion,
                Nit            = req.Nit,
                NitValido      = nitValido
            };

            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, MapResponse(cliente));
        }

        private static ClienteResponse MapResponse(Cliente c) => new()
        {
            Id             = c.Id,
            NombreCompleto = c.NombreCompleto,
            Telefono       = c.Telefono,
            Direccion      = c.Direccion,
            Nit            = c.Nit,
            NitValido      = c.NitValido,
            FechaRegistro  = c.FechaRegistro
        };
    }
}
