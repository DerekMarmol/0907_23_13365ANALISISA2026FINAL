using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnviosRapidosGT.Models
{
    // Flujo: Registrado → EnTransito → EnReparto → Entregado / Devuelto / EnDevolucion → Devuelto
    public enum EstadoEnvio
    {
        Registrado, EnTransito, EnReparto,
        Entregado, EnDevolucion, Devuelto, Cancelado
    }

    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Nit { get; set; }

        public bool NitValido { get; set; } = false;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Envio> EnviosComoRemitente { get; set; } = new List<Envio>();
        public ICollection<Envio> EnviosComoDestinatario { get; set; } = new List<Envio>();
    }

    public class Envio
    {
        [Key]
        public int Id { get; set; }

        // Formato: ENV-YYYYMMDD-XXXX
        [Required, MaxLength(20)]
        public string CodigoRastreo { get; set; } = string.Empty;

        [Required]
        public int RemitenteId { get; set; }
        [ForeignKey(nameof(RemitenteId))]
        public Cliente Remitente { get; set; } = null!;

        [Required]
        public int DestinatarioId { get; set; }
        [ForeignKey(nameof(DestinatarioId))]
        public Cliente Destinatario { get; set; } = null!;

        [Required, Range(0.01, 9999.99)]
        public decimal PesoKg { get; set; }

        [Required, MaxLength(250)]
        public string DireccionEntrega { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string OficinaOrigen { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TarifaBase { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Descuento { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TarifaFinal { get; set; }

        public bool DescuentoAplicado { get; set; } = false;

        public EstadoEnvio Estado { get; set; } = EstadoEnvio.Registrado;

        [MaxLength(100)]
        public string? UltimaUbicacion { get; set; }

        [Range(0, 3)]
        public int IntentosEntrega { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimaActualizacion { get; set; } = DateTime.UtcNow;

        public ICollection<HistorialEstado> Historial { get; set; } = new List<HistorialEstado>();
    }

    // Registro inmutable — no se edita ni elimina
    public class HistorialEstado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnvioId { get; set; }
        [ForeignKey(nameof(EnvioId))]
        public Envio Envio { get; set; } = null!;

        [Required]
        public EstadoEnvio EstadoNuevo { get; set; }
        public EstadoEnvio EstadoAnterior { get; set; }

        [Required, MaxLength(100)]
        public string Ubicacion { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notas { get; set; }

        public bool EsAutomatico { get; set; } = false;
    }
}
