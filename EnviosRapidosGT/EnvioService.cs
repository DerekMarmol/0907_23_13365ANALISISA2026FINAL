using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.Services
{
    public class EnvioService
    {
        // Transiciones válidas según las reglas del sistema
        private static readonly Dictionary<EstadoEnvio, List<EstadoEnvio>> _transicionesValidas = new()
        {
            { EstadoEnvio.Registrado,    new() { EstadoEnvio.EnTransito, EstadoEnvio.Cancelado } },
            { EstadoEnvio.EnTransito,    new() { EstadoEnvio.EnReparto } },
            { EstadoEnvio.EnReparto,     new() { EstadoEnvio.Entregado, EstadoEnvio.Devuelto, EstadoEnvio.EnDevolucion } },
            { EstadoEnvio.EnDevolucion,  new() { EstadoEnvio.Devuelto } },
            { EstadoEnvio.Entregado,     new() { } },
            { EstadoEnvio.Devuelto,      new() { } },
            { EstadoEnvio.Cancelado,     new() { } },
        };

        public bool TransicionEsValida(EstadoEnvio actual, EstadoEnvio nuevo)
        {
            return _transicionesValidas.TryGetValue(actual, out var permitidos)
                   && permitidos.Contains(nuevo);
        }

        // Tarifa por peso según reglas del examen
        public decimal CalcularTarifa(decimal pesoKg) => pesoKg switch
        {
            <= 1m          => 25.00m,
            <= 5m          => 45.00m,
            <= 10m         => 75.00m,
            _              => 100.00m
        };

        // Validación de NIT guatemalteco — algoritmo módulo 11 del SAT
        public bool ValidarNit(string? nit)
        {
            if (string.IsNullOrWhiteSpace(nit)) return false;

            nit = nit.Replace("-", "").Trim().ToUpper();
            if (nit == "CF") return true; // Consumidor Final es válido

            if (!nit.All(char.IsDigit) || nit.Length < 2) return false;

            string numeros = nit[..^1];
            int digitoVerificador = int.Parse(nit[^1..]);

            int factor = numeros.Length + 1;
            int suma = 0;
            foreach (char c in numeros)
            {
                suma += int.Parse(c.ToString()) * factor;
                factor--;
            }

            int residuo = suma % 11;
            int esperado = residuo == 0 ? 0 : 11 - residuo;

            return esperado == digitoVerificador;
        }

        // Genera ENV-YYYYMMDD-XXXX; el secuencial se pasa desde el controlador
        public string GenerarCodigoRastreo(int secuencial)
        {
            string fecha = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"ENV-{fecha}-{secuencial:D4}";
        }
    }
}
