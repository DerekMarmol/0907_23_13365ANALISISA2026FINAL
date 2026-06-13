using Xunit;
using EnviosRapidosGT.Services;
using EnviosRapidosGT.Models;

namespace EnviosRapidosGT.Tests
{
    public class EnvioServiceTests
    {
        private readonly EnvioService _service;

        public EnvioServiceTests()
        {
            // Inicializamos el servicio antes de cada prueba
            _service = new EnvioService();
        }

        // ── 1. PRUEBAS DE TÁRIFAS AUTOMÁTICAS (Reglas del Examen) ──
        [Theory]
        [InlineData(0.5, 25.00)]   // Menor o igual a 1kg
        [InlineData(1.0, 25.00)]   // Límite exacto de 1kg
        [InlineData(2.5, 45.00)]   // Entre 1.01kg y 5kg
        [InlineData(5.0, 45.00)]   // Límite exacto de 5kg
        [InlineData(7.2, 75.00)]   // Entre 5.01kg y 10kg
        [InlineData(10.0, 75.00)]  // Límite exacto de 10kg
        [InlineData(12.8, 100.00)] // Mayor a 10kg
        public void CalcularTarifa_DeberiaAsignarPrecioSegunRangoDePeso(decimal peso, decimal tarifaEsperada)
        {
            // Act
            decimal tarifaCalculada = _service.CalcularTarifa(peso);

            // Assert
            Assert.Equal(tarifaEsperada, tarifaCalculada);
        }

        // ── 2. PRUEBAS DE VALIDACIÓN DE NIT (Módulo 11 SAT & CF) ──
        [Fact]
        public void ValidarNit_DeberiaRetornarTrue_CuandoEsConsumidorFinal()
        {
            // Act
            bool resultado = _service.ValidarNit("CF");
            bool resultadoMinuscula = _service.ValidarNit("cf");

            // Assert
            Assert.True(resultado);
            Assert.True(resultadoMinuscula);
        }

        [Theory]
        [InlineData("43214")]  // NIT numérico válido calculado con Módulo 11
        [InlineData("123455")] // Otro NIT numérico válido bajo el algoritmo
        public void ValidarNit_DeberiaRetornarTrue_CuandoElNitEsValido(string nitValido)
        {
            // Act
            bool resultado = _service.ValidarNit(nitValido);

            // Assert
            Assert.True(resultado);
        }

        [Theory]
        [InlineData("123456")] // Dígito verificador incorrecto
        [InlineData("ABC-1")]  // Contiene letras no permitidas
        [InlineData("")]       // Vacío
        [InlineData(null)]     // Nulo
        public void ValidarNit_DeberiaRetornarFalse_CuandoElNitEsInvalido(string? nitInvalido)
        {
            // Act
            bool resultado = _service.ValidarNit(nitInvalido);

            // Assert
            Assert.False(resultado);
        }

        // ── 3. PRUEBAS DE TRANSICIONES DE ESTADO (Flujo del Proceso) ──
        [Theory]
        [InlineData(EstadoEnvio.Registrado, EstadoEnvio.EnTransito)]
        [InlineData(EstadoEnvio.Registrado, EstadoEnvio.Cancelado)]
        [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.EnReparto)]
        [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.EnDevolucion)]
        [InlineData(EstadoEnvio.EnDevolucion, EstadoEnvio.Devuelto)]
        public void TransicionEsValida_DeberiaRetornarTrue_CuandoElCambioEstaPermitido(EstadoEnvio actual, EstadoEnvio nuevo)
        {
            // Act
            bool esValida = _service.TransicionEsValida(actual, nuevo);

            // Assert
            Assert.True(esValida);
        }

        [Theory]
        [InlineData(EstadoEnvio.Registrado, EstadoEnvio.Entregado)]    // No puede saltarse pasos
        [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.Devuelto)]     // Debe pasar primero por reparto/devolución
        [InlineData(EstadoEnvio.Entregado, EstadoEnvio.EnTransito)]    // Un estado final no puede retroceder
        [InlineData(EstadoEnvio.Cancelado, EstadoEnvio.Registrado)]    // Un paquete cancelado muere ahí
        public void TransicionEsValida_DeberiaRetornarFalse_CuandoElCambioEstaProhibido(EstadoEnvio actual, EstadoEnvio nuevo)
        {
            // Act
            bool esValida = _service.TransicionEsValida(actual, nuevo);

            // Assert
            Assert.False(esValida);
        }

        // ── 4. PRUEBA DE GENERACIÓN DE CÓDIGO DE RASTREO ──
        [Fact]
        public void GenerarCodigoRastreo_DeberiaTenerElFormatoCorrecto()
        {
            // Arrange
            int secuencial = 7;
            string fechaHoy = DateTime.UtcNow.ToString("yyyyMMdd");
            string formatoEsperado = $"ENV-{fechaHoy}-0007";

            // Act
            string codigoGenerado = _service.GenerarCodigoRastreo(secuencial);

            // Assert
            Assert.NotNull(codigoGenerado);
            Assert.Equal(formatoEsperado, codigoGenerado);
            Assert.StartsWith("ENV-", codigoGenerado);
        }
    }
}