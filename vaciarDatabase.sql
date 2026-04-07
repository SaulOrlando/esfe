USE [ProyectoSocioeconomicoDB]
GO

-- =============================================
-- SCRIPT PARA VACIAR LA BASE DE DATOS
-- Borra TODOS los registros de todas las tablas
-- Orden correcto según las claves foráneas (FK)
-- para evitar errores de restricción
-- =============================================
-- Tablas "hijas" (dependientes)
DELETE FROM [dbo].[InscripcionesVoluntarios];
DELETE FROM [dbo].[Notificaciones];
DELETE FROM [dbo].[Comprobantes];
DELETE FROM [dbo].[Retiros];

-- Tablas intermedias
DELETE FROM [dbo].[Donaciones];
DELETE FROM [dbo].[Casos];
DELETE FROM [dbo].[Programas];

-- Tablas "padres" (referenciadas por muchas otras)
DELETE FROM [dbo].[Usuarios];
DELETE FROM [dbo].[Categorias];
DELETE FROM [dbo].[Roles];

GO
