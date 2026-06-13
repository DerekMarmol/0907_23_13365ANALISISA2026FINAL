using Microsoft.EntityFrameworkCore;
using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Envio> Envios => Set<Envio>();
        public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Envio tiene dos FK hacia Cliente; hay que indicar las relaciones explícitamente
            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Remitente)
                .WithMany(c => c.EnviosComoRemitente)
                .HasForeignKey(e => e.RemitenteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Destinatario)
                .WithMany(c => c.EnviosComoDestinatario)
                .HasForeignKey(e => e.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // CodigoRastreo único por envío
            modelBuilder.Entity<Envio>()
                .HasIndex(e => e.CodigoRastreo)
                .IsUnique();

            // NIT único entre clientes que lo tengan (solo cuando no es null)
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Nit)
                .IsUnique()
                .HasFilter("Nit IS NOT NULL AND NitValido = 1");
        }
    }
}
