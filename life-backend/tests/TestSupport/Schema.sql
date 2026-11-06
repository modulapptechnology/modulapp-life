CREATE TABLE dbo.Usuarios (
    Id uniqueidentifier NOT NULL PRIMARY KEY,
    Email nvarchar(256) NOT NULL UNIQUE,
    Nombre nvarchar(100) NOT NULL,
    CreadoEn datetime2 NOT NULL
);

CREATE TABLE dbo.EspaciosTrabajo (
    Id uniqueidentifier NOT NULL PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    PropietarioId uniqueidentifier NOT NULL REFERENCES dbo.Usuarios (Id),
    CreadoEn datetime2 NOT NULL
);
