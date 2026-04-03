---
name: proyecto-esfe
description: Especializado en el desarrollo y mantenimiento del Proyecto Socio Económico ESFE (ASP.NET Core, Blazor Server, EF Core, Bulma CSS).
---

# Proyecto Socio Económico ESFE

Instrucciones para el agente al trabajar en este proyecto de gestión socioeconómica y donaciones.

## Cuando usar

Este skill debe activarse siempre que se realicen tareas de desarrollo en el repositorio `ProyectoESFE`, tales como:
- Creación o modificación de componentes Blazor (`.razor`).
- Implementación de lógica de negocio en la capa de Aplicación o Infraestructura.
- Definición de entidades de Dominio o configuraciones de base de datos.
- Ejecución de migraciones de Entity Framework Core.
- Estilización de la interfaz de usuario con Bulma CSS.

## Instrucciones

1. **Confirmación Inicial:** Siempre comienza tu primera respuesta confirmando que has leído y comprendido `GEMINI.md` diciendo: "He leído GEMINI.md".
2. **Estándares de UI:**
   - **Prioridad:** Usa exclusivamente **Bulma CSS** (evita Tailwind o Bootstrap).
   - **Clases:** Si usas clases de Bulma, antepón `bulma-` a tus clases personalizadas para evitar colisiones (ej. `bulma-custom-box`).
   - **Especificidad:** Mantén una alta especificidad en CSS personalizado (ej. `section.section-redondeado`).
   - **Componentes:** Prefiere helpers y componentes de Bulma (`columns`, `box`, `card`, `level`).
3. **Arquitectura:**
   - Mantén los componentes Blazor ligeros; delega la lógica pesada a los servicios.
   - Respeta la separación de capas: `Domain` (Entidades), `Application` (Interfaces/DTOs), `Infrastructure` (Datos/Servicios), `WebUI` (Interfaz).
4. **Base de Datos (EF Core):**
   - Usa los comandos específicos de `GEMINI.md` para migraciones, especificando siempre `--project ProyectoSocioEconomico.Infrastructure` y `--startup-project ProyectoSocioEconomico.WebUI`.
5. **Calidad de Código:**
   - Usa **PascalCase** para clases/propiedades y **camelCase** para variables locales.
   - Aplica validaciones con Data Annotations o Fluent Validation.
   - Usa `async/await` para accesos a datos y operaciones de red.
6. **Mantenibilidad:**
   - Evita estilos en línea (inline styles).
   - Usa componentes reutilizables en lugar de duplicar marcado HTML.
   - Mantén el HTML semántico y los atributos de accesibilidad.
