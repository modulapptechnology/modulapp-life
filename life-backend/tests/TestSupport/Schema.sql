CREATE TABLE dbo.Users (
    Id uniqueidentifier NOT NULL PRIMARY KEY,
    Email nvarchar(256) NOT NULL UNIQUE,
    Name nvarchar(100) NOT NULL,
    CreatedAt datetime2 NOT NULL
);

CREATE TABLE dbo.Workspaces (
    Id uniqueidentifier NOT NULL PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    OwnerId uniqueidentifier NOT NULL REFERENCES dbo.Users (Id),
    CreatedAt datetime2 NOT NULL
);
