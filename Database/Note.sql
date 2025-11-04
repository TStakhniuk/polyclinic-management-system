USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sys.objects WHERE name = 'NewsNote' AND type_desc = 'USER_TABLE') 
    DROP TABLE NewsNote

GO

CREATE TABLE NewsNote (
    ID INT PRIMARY KEY IDENTITY(1,1),
    NoteDate DATE NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);