:on error exit

USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppUser)')
BEGIN
    CREATE LOGIN [$(AppUser)] WITH PASSWORD = N'$(AppPassword)', CHECK_POLICY = ON;
END
GO

USE [$(Database)];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppUser)')
BEGIN
    CREATE USER [$(AppUser)] FOR LOGIN [$(AppUser)];
END
GO

ALTER ROLE db_datareader ADD MEMBER [$(AppUser)];
ALTER ROLE db_datawriter ADD MEMBER [$(AppUser)];
GO
