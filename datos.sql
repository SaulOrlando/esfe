USE [ProyectoSocioeconomicoDB]
GO

-- =============================================
-- SCRIPT CORREGIDO PARA LLENAR LA BASE DE DATOS
-- Soluciona el error de FOREIGN KEY (IdRol)
-- =============================================

-- 2. Reiniciamos los IDENTITY para que los IDs empiecen desde 1 otra vez
-- (este era el problema: DELETE no reinicia los contadores)
DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Categorias]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Usuarios]', RESEED, 0);
GO

-- 3. ROLES (exactamente los 3 que pediste)
INSERT INTO [dbo].[Roles] (Nombre, Descripcion, Estado)
VALUES 
    ('Donante',        'Usuario que realiza donaciones a casos y programas', 'Activo'),
    ('Beneficiario',   'Usuario que crea casos y recibe ayuda',              'Activo'),
    ('Administrador',  'Administrador del sistema con acceso total',         'Activo');
GO

-- 4. CATEGORIAS (exactamente las 5 que pediste)
INSERT INTO [dbo].[Categorias] (Nombre, Descripcion, Estado)
VALUES 
    ('Infraestructura',    'Proyectos de construcción, vías, agua y saneamiento', 'Activo'),
    ('Naturaleza',         'Conservación ambiental, reforestación y ecología',   'Activo'),
    ('Educacion',          'Educación, becas y formación académica',             'Activo'),
    ('Salud',              'Salud, atención médica y bienestar',                 'Activo'),
    ('Desastres naturales', 'Ayuda humanitaria en emergencias y desastres',      'Activo');
GO
