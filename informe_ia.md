# Informe de Uso de Inteligencia Artificial
**Proyecto:** Sistema de Gestión de Envíos (Envíos Rápidos GT)  
**Asignatura:** Análisis de Sistemas I  
**Fecha:** 13/Jun/2026

---

## 1. Introducción y Herramientas Utilizadas
Para el desarrollo y resolución de este examen final, se empleó un enfoque híbrido utilizando dos modelos de Lenguaje de Gran Escala (LLMs) complementarios:
* **Claude (Modelo Sonnet 4.6 - Nivel de pensamiento alto):** Utilizado para la fase analítica inicial, planificación estructurada, diseño del modelo de datos y escritura del código base (API y Reglas de Negocio).
* **Gemini (Modelo 3.1 PRO):** Utilizado para la fase de depuración (troubleshooting), ejecución de pruebas manuales y unitarias, integración de Git y despliegue final en la nube (Render).

---

## 2. Prompts Utilizados (Fase 1: Claude Sonnet 4.6)
Los siguientes comandos se utilizaron para la estructuración conceptual y la escritura del código base:

1. *"Puedes leer todo esto? Antes de empezar a resolver, dime si puedes leerlo [Se pegó el contexto del examen, reglas del sistema y entregables]"*
2. *"Bien, vamos por partes, ¿con que sugieres empezar? Ordena cada punto a realizar siendo el 1 el primero en hacer y subiendo de manera continua."*
3. *"Ok, realiza las 10 historias de usuario. Realízalos algo rebuscados y/o avanzados para que no sean los genéricos de siempre. Obviamente deben de ir algunos que son los básicos ya que son las bases."*
4. *"Correcto, ese es el formato a usar. Supongo que de hacerlos es en un ¿txt?. Ya para el tema de la API si pues c#."*
5. *"Bueno, me parece, hazlo en un markdown"*
6. *"Adelante, sigue con el modelo de datos."*
7. *"Podrías de reducir de manera significativa la cantidad de comentarios agregados al models? Son algo excesivos"*
8. *"Antes de continuar, estos Models son las bases para las llamadas en el API REST, ¿Correcto?"*
9. *"Ok, continua entonces con la API REST."*
10. *"Antes de pasar a las pruebas unitarias, cual es la forma de correr esto para comprobar el correcto funcionamiento?"*
11. *"Antes de continuar, realiza un resumen de lo que se ha trabajado, que falta y por qué se ha trabajado así."*

## 3. Prompts Utilizados (Fase 2: Gemini 3.1 PRO)
Los siguientes comandos se utilizaron para levantar el entorno local, depurar errores, crear pruebas unitarias y desplegar:

12. *"Hola, estoy trabajando en este examen final... La API la ejecute de esta forma: dotnet new webapi... El problema es que cuando me meto a la dirección que me sale ahí la http://localhost:5237 me sale esto: No se encontró ninguna página web"*
13. *"Oye, pero entonces lo que debería de haber hecho es, los archivos que te pase primero, meterlos dentro de la carpeta que se creo con el comando dotnet y ahí volver a ejecutar?"*
14. *"Antes de continuar si me gustaría realizar una prueba manual para comprobar el funcionamiento, dame una."*
15. *"Me respondió con un error: Invoke-RestMethod : Error en el servidor remoto: (400) Solicitud incorrecta."*
16. *"Use sin el acento, pero al ejecutar el paso 2 del destinatario, error de nuevo: Invoke-RestMethod : Error en el servidor remoto: (409) Conflicto."*
17. *"Dio errores y advertencias: warning MSB3026... The process cannot access the file because it is being used by another process."*
18. *"Como lo subo a github, es a este repositorio... Como lo subo todo, antes de levantarlo en render."*
19. *"los comandos de git me dieron esto: Author identity unknown... Hice mal algo?"*
20. *"en esto [formulario de Render en 'Start Command']: No debo de poner nada?"*
21. *"Estas instrucciones [de Render] estan en el readme?"*

---

## 4. Correcciones Realizadas durante el Desarrollo
A pesar de la precisión de los modelos, fue necesaria la intervención y análisis para solucionar obstáculos técnicos en el flujo de trabajo:

* **Refactorización de Modelos:** El código inicial generado contenía documentación excesiva en las propiedades de las clases. Se solicitó explícitamente a la IA limpiar el código y mantener solo los comentarios esenciales sobre lógica de negocio.
* **Solución de Arquitectura y Error 404:** Al iniciar el proyecto en .NET 8, los archivos autogenerados no fueron sobreescritos correctamente. Se reestructuró manualmente el árbol de carpetas introduciendo los Controladores, Servicios y reemplazando el `Program.cs` por defecto para que los endpoints fueran reconocidos.
* **Encoding JSON en PowerShell (Error 400):** Durante las pruebas manuales, el envío de caracteres especiales (tildes en apellidos) corrompió la carga útil JSON. Se corrigió omitiendo tildes temporalmente para asegurar la interpretación UTF-8 estricta del servidor.
* **Validación de Regla de Negocio de NIT (Error 409):** Al intentar registrar un segundo cliente de prueba con el NIT "CF", el sistema bloqueó la acción. Lejos de ser un fallo, la IA ayudó a identificar que la base de datos (Unique Index) y el controlador estaban funcionando perfectamente, ya que se configuró para no aceptar duplicados. Se procedió a registrar el segundo cliente sin NIT.
* **Desbloqueo del motor xUnit:** Las pruebas unitarias arrojaban errores de compilación (`MSB3026`) porque el proceso del servidor local estaba bloqueando el archivo `apphost.exe`. Se corrigió interrumpiendo el servidor web antes de ejecutar `dotnet test`.
* **Configuración Local de Git y README:** Se corrigió un bloqueo de versión al confirmar credenciales de GitHub locales, y se realizó una última actualización al archivo `README.md` inyectando la URL pública de producción generada por Render.

---

## 5. Reflexiones sobre el uso de la Inteligencia Artificial
El desarrollo de este examen demostró que las herramientas de IA son asistentes invaluables, pero requieren el juicio crítico y la dirección estratégica del analista/desarrollador:

1. **La importancia del contexto paso a paso:** Solicitar a la IA que ordenara los pasos lógicamente (ir desde historias de usuario $\rightarrow$ modelos $\rightarrow$ controladores) previno alucinaciones y garantizó que la arquitectura tuviera sentido desde sus cimientos.
2. **IA como herramienta de Troubleshooting:** Gemini demostró una capacidad excepcional no solo para escribir código, sino para leer *stack traces* y errores de la terminal de Windows. Entender que el Error 409 no era un "bug", sino el cumplimiento de una regla de negocio programada previamente, fue un aprendizaje clave.
3. **Optimización del tiempo:** Tareas repetitivas como la escritura de los 25 casos de prueba en xUnit o la estructuración de un archivo `Dockerfile` multipropósito se realizaron en segundos, permitiendo dedicar el enfoque analítico a la comprensión de las lógicas de negocio (cálculo de módulo 11 del SAT, gestión de transiciones de estados, etc.).