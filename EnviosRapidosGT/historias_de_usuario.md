# Historias de Usuario — Envíos Rápidos GT
**Proyecto:** Sistema de Gestión de Envíos  
**Asignatura:** Análisis de Sistemas I  
**Fecha:** 13/Jun/2026

---

## HU-01 — Registro de envío con tarifa automática

**Como** operador de oficina,  
**quiero** registrar un nuevo envío ingresando los datos del remitente, destinatario y peso del paquete,  
**para que** el sistema calcule y asigne automáticamente la tarifa correspondiente y genere el código de rastreo sin intervención manual.

### Criterios de aceptación
- El sistema calcula la tarifa según el peso: ≤1 kg → Q25.00, 1.01–5 kg → Q45.00, 5.01–10 kg → Q75.00, >10 kg → Q100.00.
- El código de rastreo se genera automáticamente con el formato `ENV-YYYYMMDD-XXXX` donde XXXX es un número secuencial del día.
- El envío queda en estado `Registrado` al momento de su creación.
- Si el remitente o destinatario tiene NIT válido, se aplica automáticamente un descuento del 5% sobre la tarifa calculada.
- No se permite registrar un envío sin remitente, destinatario o peso.

---

## HU-02 — Consulta de estado de envío por código de rastreo

**Como** cliente (remitente o destinatario),  
**quiero** consultar el estado actual de mi paquete ingresando el código de rastreo,  
**para** saber en qué punto del proceso se encuentra mi envío sin necesidad de llamar a una oficina.

### Criterios de aceptación
- El sistema retorna el estado actual, la última ubicación registrada y el timestamp de la última actualización.
- Si el código de rastreo no existe, el sistema responde con un mensaje de error claro (`404 Not Found`).
- La consulta es pública; no requiere autenticación.
- Se muestra el historial completo de cambios de estado ordenado cronológicamente.

---

## HU-03 — Actualización de estado con validación de transición

**Como** repartidor o agente de oficina,  
**quiero** actualizar el estado de un envío indicando la nueva etapa y la oficina donde ocurre el cambio,  
**para** mantener el historial de tránsito actualizado y que el cliente pueda rastrearlo en tiempo real.

### Criterios de aceptación
- El sistema solo permite transiciones válidas según el flujo: `Registrado → EnTransito → EnReparto → Entregado`, `EnReparto → Devuelto`, `EnReparto → EnDevolucion → Devuelto`.
- Si se intenta una transición inválida (ej. de `Entregado` a `EnTransito`), el sistema rechaza la operación con error `400 Bad Request` y un mensaje descriptivo.
- Cada actualización registra automáticamente el timestamp del servidor.
- La ubicación (oficina) es obligatoria en cada actualización de estado.
- Las notas son opcionales pero se almacenan si se proporcionan.

---

## HU-04 — Control automático de intentos de entrega fallidos

**Como** sistema,  
**quiero** llevar un conteo de los intentos de entrega fallidos por envío,  
**para** que al alcanzar 3 intentos fallidos, el estado cambie automáticamente a `EnDevolucion` sin requerir intervención manual del operador.

### Criterios de aceptación
- Cada intento fallido de entrega incrementa el contador de intentos del envío en 1.
- Al registrar el tercer intento fallido, el sistema cambia automáticamente el estado a `EnDevolucion`.
- No se permite registrar un cuarto intento de entrega si el envío ya está en `EnDevolucion` o `Devuelto`.
- El sistema registra en el historial el motivo del cambio automático indicando "Máximo de intentos alcanzado".
- El contador de intentos es visible en la consulta de detalle del envío.

---

## HU-05 — Validación de NIT y aplicación de descuento

**Como** operador de oficina,  
**quiero** que el sistema valide automáticamente el NIT del cliente al momento del registro,  
**para** aplicar el descuento del 5% solo a clientes con NIT válido, evitando descuentos incorrectos o fraudes.

### Criterios de aceptación
- El sistema valida el NIT usando el algoritmo de verificación del SAT de Guatemala (módulo 11).
- Si el NIT es válido, se aplica el 5% de descuento sobre la tarifa calculada por peso y se muestra el monto descontado.
- Si el NIT es inválido o está vacío, la tarifa se aplica sin descuento.
- El campo NIT acepta el formato estándar guatemalteco (números seguidos de guion y dígito verificador, ej. `1234567-8`).
- El sistema almacena si el descuento fue aplicado y el monto original vs. el monto final.

---

## HU-06 — Registro de historial de cambios de estado

**Como** supervisor de operaciones,  
**quiero** que cada cambio de estado de un envío quede registrado en un historial inmutable con todos sus detalles,  
**para** poder auditar el recorrido completo de cualquier paquete y resolver disputas con clientes.

### Criterios de aceptación
- Cada entrada del historial contiene: estado nuevo, ubicación (oficina), timestamp automático del servidor y notas opcionales.
- El historial no permite edición ni eliminación de registros una vez creados.
- El historial está disponible tanto en la consulta pública por código de rastreo como en el panel administrativo.
- Se puede filtrar el historial por rango de fechas y por oficina.
- El historial se retorna ordenado de más reciente a más antiguo por defecto, con opción de invertir el orden.

---

## HU-07 — Listado y filtrado de envíos por estado y oficina

**Como** gerente de sucursal,  
**quiero** obtener un listado de todos los envíos filtrado por estado actual y/o por oficina de origen o de última actualización,  
**para** tener visibilidad operativa en tiempo real y tomar decisiones sobre la carga de trabajo de cada sucursal.

### Criterios de aceptación
- El endpoint acepta filtros opcionales: `estado`, `oficinaOrigen`, `oficinaActual`, `fechaDesde`, `fechaHasta`.
- Si no se aplica ningún filtro, retorna todos los envíos paginados (máximo 50 por página).
- El resultado incluye para cada envío: código de rastreo, estado actual, remitente, destinatario, peso, tarifa y última ubicación.
- Los filtros son combinables (ej. estado=EnReparto y oficinaActual=Jalapa).
- Se retorna el total de registros encontrados junto con los datos paginados.

---

## HU-08 — Reporte de envíos en devolución con detalle de intentos

**Como** coordinador logístico,  
**quiero** obtener un reporte de todos los envíos que están en estado `EnDevolucion` o `Devuelto`, incluyendo el número de intentos fallidos y las fechas de cada intento,  
**para** analizar patrones de entregas fallidas por zona o repartidor y tomar acciones correctivas.

### Criterios de aceptación
- El reporte incluye: código de rastreo, remitente, destinatario, dirección de entrega, número de intentos fallidos, fechas de cada intento y estado actual.
- Se puede filtrar por rango de fechas de creación del envío.
- El reporte se puede obtener en formato JSON desde el endpoint.
- Los envíos se ordenan por número de intentos fallidos de mayor a menor.
- Se incluye un resumen al final con el total de envíos en devolución y el promedio de intentos fallidos.

---

## HU-09 — Gestión de clientes con reutilización en múltiples envíos

**Como** operador de oficina,  
**quiero** registrar clientes en el sistema y vincularlos a múltiples envíos como remitente o destinatario,  
**para** evitar ingresar los mismos datos de contacto cada vez que ese cliente realiza o recibe un envío.

### Criterios de aceptación
- Un cliente tiene: nombre completo, teléfono, dirección, NIT (opcional) e indicador de si el NIT es válido.
- Al registrar un envío, se puede seleccionar un cliente existente por su ID o NIT, o crear uno nuevo en el mismo flujo.
- El sistema no permite duplicar clientes con el mismo NIT válido.
- Un cliente puede aparecer como remitente en unos envíos y como destinatario en otros.
- Se puede consultar el historial de envíos asociados a un cliente (como remitente y como destinatario) desde el endpoint de detalle del cliente.

---

## HU-10 — Cancelación de envío solo en estado Registrado

**Como** operador de oficina o cliente,  
**quiero** poder cancelar un envío únicamente cuando su estado es `Registrado`,  
**para** corregir errores de captura antes de que el paquete entre en tránsito, sin afectar envíos que ya están en proceso de entrega.

### Criterios de aceptación
- La cancelación solo está permitida si el envío está en estado `Registrado`; cualquier otro estado retorna error `409 Conflict` con mensaje descriptivo.
- Al cancelar, el estado cambia a `Cancelado` y se registra en el historial con timestamp y motivo opcional.
- Un envío cancelado no puede transicionar a ningún otro estado.
- La cancelación requiere indicar un motivo (campo obligatorio para este caso).
- El sistema retorna los datos del envío cancelado con confirmación del cambio de estado.

---

*Total: 10 historias de usuario*  
*Repositorio:* `CARNET_ANALISISISA2026FINAL`
