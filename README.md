# Documentación del Proyecto

## 1. Resumen general

`ProyectoSocioEconomico` es una solución ASP.NET Core con Blazor Server y Entity Framework Core orientada a gestionar:

- usuarios con roles (`Donante`, `Beneficiario`, `Voluntario`, `Administrador`)
- casos solidarios creados por beneficiarios
- programas institucionales de apoyo
- donaciones a casos y programas
- retiros de fondos por parte de beneficiarios
- postulaciones y aprobación de voluntariado

La solución está separada por capas para mantener responsabilidades claras:

- `ProyectoSocioEconomico.WebUI`: interfaz, navegación, layouts, componentes Razor, estado de sesión y estado temporal de formularios.
- `ProyectoSocioEconomico.Infrastructure`: acceso a datos, `DbContext`, migraciones y servicios concretos.
- `ProyectoSocioEconomico.Application`: contratos de servicios.
- `ProyectoSocioEconomico.Domain`: entidades del dominio.

## 2. Estructura principal del repositorio

### `ProyectoSocioEconomico.WebUI`

Proyecto web principal. Contiene la aplicación Blazor Server, páginas Razor, layouts, archivos estáticos y servicios de UI.

Subcarpetas relevantes:

- `Components/`: raíz de componentes Blazor.
- `Components/Layout/`: layouts compartidos como navegación principal, layout de perfil y modal de reconexión.
- `Components/Pages/`: páginas funcionales de la aplicación.
- `Services/`: servicios de estado y autenticación usados solo por la capa Web.
- `ViewModels/`: modelos auxiliares para la UI.
- `wwwroot/`: archivos estáticos, CSS, librerías frontend y uploads.
- `Properties/`: configuración de lanzamiento local.

Archivos importantes:

- `Program.cs`: arranque de la app, DI, Blazor Server, autenticación y middleware.
- `appsettings.json`: logging y cadena de conexión `DefaultConnection`.
- `ProyectoSocioEconomico.WebUI.csproj`: proyecto web en `net10.0`.

### `ProyectoSocioEconomico.Infrastructure`

Capa de infraestructura. Implementa persistencia con EF Core y las reglas de negocio que interactúan directamente con la base de datos.

Subcarpetas relevantes:

- `Data/`: `AppDbContext`.
- `Services/`: implementaciones de interfaces de aplicación.
- `Migrations/`: migraciones EF Core y snapshot del modelo.

Archivos importantes:

- `DependencyInjection.cs`: registra contexto, factory y servicios concretos.
- `Data/AppDbContext.cs`: define tablas, relaciones, restricciones, seed data y detalles del modelo.
- `Migrations/20260411062304_InitialCreate.cs`: migración inicial existente en el repositorio.

### `ProyectoSocioEconomico.Application`

Capa de contratos. No contiene lógica de persistencia; define las interfaces que consume la UI y que implementa Infrastructure.

Subcarpetas relevantes:

- `Interfaces/`: contratos de servicios.

Archivos importantes:

- `IUsuarioService.cs`
- `ICasoService.cs`
- `IProgramaService.cs`
- `IRetiroService.cs`

### `ProyectoSocioEconomico.Domain`

Capa de dominio. Contiene las entidades del sistema y sus relaciones.

Subcarpetas relevantes:

- `Entities/`: clases de dominio persistidas por EF Core.

Entidades principales:

- `Usuario`: identidad, perfil, rol, DUI, imagen y relaciones con casos, donaciones, programas y retiros.
- `Role`: catálogo de roles del sistema.
- `Caso`: campañas individuales creadas por beneficiarios.
- `Programa`: programas institucionales, financieros, de voluntariado o híbridos.
- `Donacione`: aportes económicos asociados a caso o programa.
- `Retiro`: solicitudes de retiro hechas por beneficiarios.
- `InscripcionesVoluntario`: postulaciones de usuarios a programas.
- `Categoria`: clasificación de casos y programas.
- `Comprobante`, `Notificacione`, `LogsFinanciero`: soporte documental y operativo.

## 3. Flujo arquitectónico

El flujo general del proyecto es este:

1. Un componente Razor en `WebUI` llama una interfaz de `Application`.
2. La implementación concreta vive en `Infrastructure/Services`.
3. El servicio usa `AppDbContext` o `IDbContextFactory<AppDbContext>`.
4. EF Core opera sobre entidades de `Domain`.
5. La UI renderiza el resultado y, en algunos casos, mantiene estado temporal con servicios `scoped`.

Esto permite que la WebUI no conozca detalles de SQL Server ni del mapeo EF, y que la capa `Application` funcione como punto de desacoplamiento.

## 4. Detalle por proyecto

### 4.1 `ProyectoSocioEconomico.WebUI`

#### `Components/`

Contiene el armazón de la aplicación Blazor.

- `App.razor`: documento raíz HTML. Carga CSS, scripts, `Routes` y `ReconnectModal`.
- `Routes.razor`: enrutador central. Define `MainLayout` como layout por defecto y `NotFound` como página no encontrada.
- `_Imports.razor`: imports globales para componentes Razor.

#### `Components/Layout/`

Define la estructura visual compartida entre páginas.

- `MainLayout.razor`: navbar pública, footer y renderizado del `Body`. También muestra el perfil del usuario autenticado leyendo claims.
- `ProfileLayout.razor`: layout de cuenta/perfil con menú lateral dependiente del rol. Tiene lógica para:
  - consultar el usuario actual
  - detectar si es beneficiario, admin o voluntario
  - resolver la ruta de “Mi caso”
  - activar enlaces laterales según el rol
- `EmptyLayout.razor`: layout minimalista para pantallas como login/registro.
- `NavMenu.razor`: menú de navegación reutilizable.
- `ReconnectModal.razor` y `.js`: experiencia de reconexión en Blazor Server.

#### `Components/Pages/`

Contiene las páginas funcionales. Se puede agrupar así:

- públicas:
  - `Home.razor`
  - `AboutUs.razor`
  - `CommunityAidPrograms.razor`
  - `ProgramDetail.razor`
  - `AidCasesDirectory.razor`
  - `CaseDetail.razor`
- autenticación y registro:
  - `Auth/Login.razor`
  - `Auth/Step1.razor`
  - `Auth/Step2.razor`
  - `Auth/Step3.razor`
- creación y administración de casos:
  - `Cases/NewCaseStep1.razor`
  - `Cases/NewCaseStep2.razor`
  - `Cases/EditCase.razor`
  - `Cases/BeneficiaryCaseDashboard.razor`
- programas:
  - `CreateProgram.razor`
  - `EditProgram.razor`
  - `AdminPrograms.razor`
  - `VolunteerProgramData.razor`
- donaciones y retiros:
  - `DonationCheckout.razor`
  - `DonacionProgram.razor`
  - `AdminDonations.razor`
  - `AdminDonationActivities.razor`
  - `BeneficiaryWithdrawalHistory.razor`
  - `AdminWithdrawals.razor`
- cuenta:
  - `AccountSettings.razor`
  - `DonorDashboard.razor`
  - `AdminVolunteerApplications.razor`

También hay componentes reutilizables como:

- `ProgramCardComponent.razor`
- `CategoryComponent.razor`
- `CaseCardComponent.razor`
- `CardComponent.razor`
- `CardsComponent.razor`

#### `Services/`

Esta carpeta es especialmente importante porque guarda el estado de UI y la autenticación del lado Blazor.

##### `CustomAuthenticationStateProvider.cs`

Es el servicio más importante de `WebUI/Services`.

Responsabilidades:

- implementa `AuthenticationStateProvider`
- guarda la sesión del usuario en `ProtectedLocalStorage` con la clave `UserSession`
- reconstruye el `ClaimsPrincipal` para Blazor
- refresca el usuario desde base de datos cada vez que reconstruye la sesión
- agrega claims como:
  - `NameIdentifier`
  - `Name`
  - `Email`
  - `ImagenPerfil`
  - `Role`

Por qué es importante:

- centraliza toda la autenticación de la UI
- permite que layouts y páginas usen `AuthorizeView`
- hace posible que cambios de perfil o rol se reflejen sin reiniciar manualmente la sesión

Riesgo o detalle técnico relevante:

- serializa la entidad `Usuario` completa en almacenamiento protegido del navegador
- depende de `IUsuarioService.ObtenerPorId` para mantener la sesión sincronizada con la base

##### `RegistrationState.cs`

Servicio `scoped` para el registro por pasos.

Qué conserva:

- progreso del wizard (`Step1Completed`, `Step2Completed`)
- nombres, teléfono y país
- DUI, fecha de nacimiento, archivos del DUI
- email y contraseña

Por qué existe:

- evita perder datos al navegar entre `Step1`, `Step2` y `Step3`
- funciona como memoria temporal del flujo de registro

##### `NewCaseState.cs`

Servicio `scoped` para el wizard de creación de casos.

Qué conserva:

- categoría, título, descripción y meta financiera
- evidencias cargadas
- miniatura del caso
- método de desembolso y datos asociados
- certificación final del usuario

Por qué existe:

- separa el estado temporal del formulario de la entidad `Caso`
- permite dividir el proceso en `NewCaseStep1` y `NewCaseStep2`

#### `ViewModels/`

Modelos auxiliares de interfaz.

- `ProfileViewModel.cs`
- `CategoryViewModel.cs`
- `ButtonViewModel.cs`

Actualmente el proyecto usa más estado directo en componentes y servicios `scoped` que view models complejos.

#### `wwwroot/`

Activos estáticos servidos por la app.

- `app.css`: estilos globales.
- `css/styles.css`: estilos adicionales de UI.
- `uploads/`: imágenes y archivos cargados por usuarios.
  - `uploads/profiles/`: fotos de perfil.
  - durante la ejecución también se usan rutas como `uploads/cases` y `uploads/documents/dui`.
- `lib/bulma/`: Bulma local y variantes prefijadas.
- `lib/bootstrap/`: Bootstrap aún está presente aunque la guía del proyecto prioriza Bulma.

### 4.2 `ProyectoSocioEconomico.Infrastructure`

#### `Data/AppDbContext.cs`

Es el archivo central de persistencia.

Responsabilidades:

- define todos los `DbSet`
- configura relaciones y `Foreign Keys`
- define tipos como `decimal(18, 2)`
- agrega índices para consultas
- ignora propiedades no mapeadas (`IdProgramas`, `IdCasos`)
- inserta datos semilla de:
  - `Categoria`
  - `Role`

Aspectos importantes:

- tiene `OnConfiguring` con una cadena por defecto a `.\SQLEXPRESS`
- también recibe configuración desde DI
- contiene toda la configuración Fluent API real del modelo

#### `DependencyInjection.cs`

Expone `AddInfrastructure`.

Registra:

- `AppDbContext`
- `IDbContextFactory<AppDbContext>`
- `IUsuarioService -> UsuarioService`
- `ICasoService -> CasoService`
- `IProgramaService -> ProgramaService`
- `IRetiroService -> RetiroService`

Detalle técnico relevante:

- `AppDbContext` se registra con `ServiceLifetime.Singleton` en `AddDbContext`
- adicionalmente existe `AddDbContextFactory`, que es lo que consumen la mayoría de servicios

#### `Services`

Esta es la carpeta más importante de backend del proyecto porque concentra la lógica de negocio y reglas operativas.

##### `UsuarioService.cs`

Responsabilidades:

- CRUD básico de usuarios
- obtener usuario por id con rol incluido
- actualizar contraseña
- validar credenciales
- generar hash SHA-256 de contraseña

Importancia:

- se usa en login, sesión, ajustes de cuenta y refresco de autenticación
- es la base del sistema de identidad actual

Observaciones:

- no usa ASP.NET Identity; la autenticación está implementada manualmente
- `VerificarCredenciales` compara email normalizado y `PasswordHash`

##### `CasoService.cs`

Responsabilidades:

- listar casos con relaciones
- obtener casos activos para home
- obtener caso por id con donaciones y donadores
- obtener el caso del beneficiario actual
- evitar duplicidad de caso por usuario
- crear, actualizar y eliminar casos
- sincronizar estado del caso según la meta alcanzada
- obtener categorías activas

Lógica de negocio importante:

- al crear un caso, fuerza el rol del usuario a `Beneficiario` si corresponde
- al eliminar un caso:
  - desvincula donaciones
  - elimina retiros relacionados
  - intenta restablecer el rol del usuario a `Donante`
- al detectar que las donaciones completadas alcanzan la meta, marca el caso como `Inactivo`

Es uno de los servicios más importantes porque conecta identidad, campañas solidarias y flujo financiero.

##### `ProgramaService.cs`

Es probablemente el archivo con más lógica de negocio del proyecto.

Responsabilidades:

- consultar programas públicos o administrativos con relaciones completas
- consultar postulaciones de voluntariado
- aprobar o rechazar postulaciones
- remover voluntarios por incumplimiento
- actualizar disponibilidad de días del voluntario
- permitir salida voluntaria de un programa
- crear, editar y eliminar programas
- cerrar automáticamente programas al cumplir meta financiera
- finalizar voluntarios cuando un programa pasa a inactivo
- restaurar rol de `Donante` si el usuario deja de pertenecer a programas activos

Reglas de negocio destacadas:

- un beneficiario o usuario con caso creado no puede ser aprobado como voluntario
- no se puede aprobar una postulación rechazada
- no se puede superar la meta de voluntarios del programa
- al aprobar una inscripción:
  - el usuario cambia a rol `Voluntario`
  - se rechazan otras solicitudes pendientes del mismo usuario
- al inactivar o eliminar programas:
  - se actualiza el estado de las inscripciones
  - se reevalúa el rol del usuario

Por complejidad, este archivo merece atención especial en cualquier mantenimiento futuro.

##### `RetiroService.cs`

Responsabilidades:

- listar retiros por beneficiario
- listar retiros con detalles para administración
- calcular total retirado por caso
- calcular balance disponible
- crear retiro
- actualizar estado del retiro

Importancia:

- es el servicio financiero que conecta donaciones acumuladas con desembolsos
- usa directamente `AppDbContext` en vez de `IDbContextFactory`

#### `Migrations/`

Carpeta generada por EF Core.

Archivos relevantes:

- `20260411062304_InitialCreate.cs`: define la creación inicial del esquema.
- `20260411062304_InitialCreate.Designer.cs`: metadata de la migración.
- `AppDbContextModelSnapshot.cs`: snapshot actual del modelo.

Su función es permitir recrear y evolucionar la base de datos de forma controlada.

### 4.3 `ProyectoSocioEconomico.Application`

#### `Interfaces/`

Define lo que la UI espera de la infraestructura.

- `IUsuarioService`: gestión de usuarios, credenciales y contraseña.
- `ICasoService`: gestión de casos y categorías.
- `IProgramaService`: gestión de programas y voluntariado.
- `IRetiroService`: gestión de retiros y balances.

Esta capa existe para desacoplar la UI de la implementación concreta.

### 4.4 `ProyectoSocioEconomico.Domain`

#### `Entities/`

Representa el modelo de negocio persistente.

##### `Usuario.cs`

Entidad principal del sistema.

Incluye:

- datos personales
- correo y contraseña hash
- DUI frontal y reverso
- imagen de perfil
- rol
- relaciones a casos, programas, donaciones, retiros y notificaciones

Detalle relevante:

- varias colecciones tienen `JsonIgnore` para evitar ciclos al serializar
- esto es importante porque `CustomAuthenticationStateProvider` serializa usuarios

##### `Caso.cs`

Representa una campaña individual de ayuda.

Incluye:

- beneficiario dueño del caso
- título, descripción, imagen, meta y estado
- categoría
- donaciones y retiros asociados

##### `Programa.cs`

Representa programas institucionales.

Incluye:

- tipo de programa (`Financiero`, `Voluntariado`, `Hibrido`)
- metas financieras y/o de voluntarios
- días permitidos de voluntariado
- creador del programa
- relaciones con donaciones e inscripciones

##### `InscripcionesVoluntario.cs`

Modela la postulación de un usuario a un programa.

Incluye:

- usuario
- programa
- fecha
- categoría de voluntariado
- días disponibles
- estado (`Pendiente`, `Aprobado`, `Activo`, `Rechazado`, `Removido`, `Finalizado`)

##### `Donacione.cs`

Representa una transacción de donación.

Puede estar ligada a:

- un caso
- un programa

Incluye monto, anonimato, fecha, estado y método de pago.

##### `Retiro.cs`

Representa una solicitud de retiro de fondos.

Incluye:

- beneficiario
- caso
- monto
- método y datos de pago
- estado y fecha de procesamiento

## 5. Archivos con más lógica que conviene conocer

### `ProyectoSocioEconomico.WebUI/Program.cs`

Archivo de arranque de la aplicación.

Qué hace:

- obtiene `DefaultConnection`
- llama `AddInfrastructure`
- configura Razor Components + Interactive Server
- aumenta `MaximumReceiveMessageSize` a 15 MB para soportar cargas de archivos
- registra:
  - autorización
  - estado de autenticación en cascada
  - `RegistrationState`
  - `NewCaseState`
  - `CustomAuthenticationStateProvider`
- configura middleware de errores, HTTPS, antiforgery y assets

### `ProyectoSocioEconomico.WebUI/Components/Layout/ProfileLayout.razor`

Archivo clave de navegación privada.

Lógica importante:

- obtiene usuario autenticado desde claims
- carga usuario real desde base
- determina si mostrar menús de admin, voluntario o beneficiario
- resuelve si “Mi caso” debe llevar a crear un caso o al dashboard del beneficiario

### `ProyectoSocioEconomico.WebUI/Components/Pages/Auth/Step3.razor`

Cierra el flujo de registro.

Lógica importante:

- valida correo y fortaleza mínima de contraseña
- comprueba si el correo ya existe
- asegura que exista el rol `Donador`
- guarda los archivos del DUI en disco
- crea el `Usuario`
- hashea la contraseña
- inicia sesión automáticamente

### `ProyectoSocioEconomico.WebUI/Components/Pages/Auth/Login.razor`

Login manual del sistema.

Lógica importante:

- valida campos básicos
- usa `UsuarioService.VerificarCredenciales`
- llama `NotifyUserLogin`
- redirige al home si el login es exitoso

### `ProyectoSocioEconomico.WebUI/Components/Pages/Cases/NewCaseStep1.razor`

Primer paso del wizard de caso.

Lógica importante:

- obliga a estar autenticado
- evita que un usuario cree más de un caso
- carga categorías desde `CasoService`
- guarda datos intermedios en `NewCaseState`

### `ProyectoSocioEconomico.WebUI/Components/Pages/Cases/NewCaseStep2.razor`

Segundo paso del wizard de caso y uno de los archivos más operativos de la UI.

Lógica importante:

- valida que `Step1` ya haya sido completado
- gestiona carga de evidencias y miniatura
- guarda archivos físicamente en `wwwroot/uploads/...`
- valida certificación y campos obligatorios
- crea el `Caso`
- guarda evidencias en carpeta por `casoId`
- limpia el estado temporal y navega al dashboard del beneficiario

### `ProyectoSocioEconomico.WebUI/Components/Pages/ProgramDetail.razor`

Uno de los archivos más ricos de UI y reglas.

Lógica importante:

- combina renderizado público con lógica de autorización
- calcula progreso financiero y de voluntarios
- decide si el usuario puede donar o postularse
- bloquea admins, beneficiarios y usuarios con caso
- detecta solicitudes pendientes o membresía en otro programa
- registra postulaciones de voluntariado directamente en la base
- gestiona modales de bloqueo y confirmación

### `ProyectoSocioEconomico.WebUI/Components/Pages/EditProgram.razor`

Pantalla administrativa con mucha lógica de negocio delegada en servicios.

Lógica importante:

- restringe acceso solo a administradores
- carga programa, categorías y solicitudes
- permite cambiar metas, tipo y estado
- permite aprobar, rechazar o remover voluntarios
- reconsulta programa y solicitudes tras cada operación

### `ProyectoSocioEconomico.WebUI/Components/Pages/DonationCheckout.razor`

Checkout de donación a casos.

Lógica importante:

- carga caso y usuario actual
- bloquea donación al propio caso
- bloquea donaciones si el caso está cerrado o ya cumplió meta
- valida método de pago seleccionado
- crea registro en `Donaciones`
- sincroniza estado del caso después de donar

### `ProyectoSocioEconomico.WebUI/Components/Pages/AccountSettings.razor`

Pantalla de autogestión de cuenta.

Lógica importante:

- obtiene usuario autenticado
- actualiza perfil básico
- sube o elimina foto de perfil
- cambia contraseña validando la actual
- refresca la sesión para reflejar nombre, imagen o rol actualizados

## 6. Relación entre `WebUI/Services` e `Infrastructure/Services`

Estas dos carpetas cumplen papeles distintos y complementarios:

### `ProyectoSocioEconomico.WebUI/Services`

Gestiona estado y comportamiento del lado de la interfaz.

- no debería contener acceso intensivo a base de datos
- conserva información temporal entre páginas
- reconstruye sesión de usuario para Blazor
- sirve como “pegamento” entre componentes y autenticación

### `ProyectoSocioEconomico.Infrastructure/Services`

Gestiona persistencia y reglas de negocio reales.

- consulta y modifica la base de datos
- aplica reglas de rol, disponibilidad, metas y estados
- encapsula operaciones complejas para que la UI no repita lógica
- actúa como backend interno de la aplicación

En resumen:

- `WebUI/Services` = estado de interfaz y sesión
- `Infrastructure/Services` = lógica de negocio + acceso a datos

## 7. Observaciones técnicas importantes

- La autenticación está implementada manualmente; no se usa ASP.NET Identity.
- Parte de la lógica de archivos vive en componentes Razor, especialmente en registro y creación de casos.
- El proyecto usa Bulma como guía principal de UI, pero todavía carga Bootstrap en `App.razor`.
- Varias páginas combinan lógica de UI y negocio; los componentes más grandes podrían dividirse más a futuro.
- `ProgramaService.cs` y `CasoService.cs` concentran la mayor parte de reglas del negocio.
- `CustomAuthenticationStateProvider.cs` es crítico para toda la experiencia autenticada.
