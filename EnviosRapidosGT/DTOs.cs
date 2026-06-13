using System.ComponentModel.DataAnnotations;
using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.DTOs
{
    // ── Cliente ──────────────────────────────────────────────────

    public class ClienteRequest
    {
        [Required, MaxLength(150)] public string NombreCompleto { get; set; } = string.Empty;
        [Required, MaxLength(20)]  public string Telefono { get; set; } = string.Empty;
        [Required, MaxLength(250)] public string Direccion { get; set; } = string.Empty;
        [MaxLength(20)]            public string? Nit { get; set; }
    }

    public class ClienteResponse
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Nit { get; set; }
        public bool NitValido { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // ── Envio ────────────────────────────────────────────────────

    public class EnvioRequest
    {
        [Required] public int RemitenteId { get; set; }
        [Required] public int DestinatarioId { get; set; }
        [Required, Range(0.01, 9999.99)] public decimal PesoKg { get; set; }
        [Required, MaxLength(250)] public string DireccionEntrega { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string OficinaOrigen { get; set; } = string.Empty;
    }

    public class EnvioResponse
    {
        public int Id { get; set; }
        public string CodigoRastreo { get; set; } = string.Empty;
        public ClienteResponse Remitente { get; set; } = null!;
        public ClienteResponse Destinatario { get; set; } = null!;
        public decimal PesoKg { get; set; }
        public string DireccionEntrega { get; set; } = string.Empty;
        public string OficinaOrigen { get; set; } = string.Empty;
        public decimal TarifaBase { get; set; }
        public decimal Descuento { get; set; }
        public decimal TarifaFinal { get; set; }
        public bool DescuentoAplicado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? UltimaUbicacion { get; set; }
        public int IntentosEntrega { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
    }

    public class EnvioResumenResponse
    {
        public int Id { get; set; }
        public string CodigoRastreo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Remitente { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public decimal PesoKg { get; set; }
        public decimal TarifaFinal { get; set; }
        public string? UltimaUbicacion { get; set; }
    }

    // ── Actualización de estado ───────────────────────────────────

    public class ActualizarEstadoRequest
    {
        [Required] public string EstadoNuevo { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string Ubicacion { get; set; } = string.Empty;
        [MaxLength(500)] public string? Notas { get; set; }
    }

    public class IntentoDeFallidoRequest
    {
        [Required, MaxLength(100)] public string Ubicacion { get; set; } = string.Empty;
        [MaxLength(500)] public string? Notas { get; set; }
    }

    public class CancelarEnvioRequest
    {
        [Required, MaxLength(500)] public string Motivo { get; set; } = string.Empty;
    }

    // ── Historial ────────────────────────────────────────────────

    public class HistorialResponse
    {
        public int Id { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Notas { get; set; }
        public bool EsAutomatico { get; set; }
    }

    // ── Paginación ───────────────────────────────────────────────

    public class PagedResponse<T>
    {
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public List<T> Data { get; set; } = new();
    }

    // ── Reporte devoluciones ─────────────────────────────────────

    public class ReporteDevolucionResponse
    {
        public int Total { get; set; }
        public double PromedioIntentos { get; set; }
        public List<EnvioDevolucionDetalle> Envios { get; set; } = new();
    }

    public class EnvioDevolucionDetalle
    {
        public string CodigoRastreo { get; set; } = string.Empty;
        public string Remitente { get; set; } = string.Empty;
        public string Destinatario { get; set; } = string.Empty;
        public string DireccionEntrega { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IntentosEntrega { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
