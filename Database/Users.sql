USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sys.objects WHERE name = 'DPUser' AND type_desc = 'USER_TABLE') 
    DROP TABLE DPUser

CREATE TABLE DPUser (
    ID_user INT PRIMARY KEY IDENTITY(1,1),
    ID_employee CHAR(10) NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    IsActive NVARCHAR(255) NOT NULL CHECK (IsActive IN ('Активний', 'Неактивний')),
    Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Адміністратор', 'Лікар', 'Працівник реєстратури')),
    FOREIGN KEY (ID_employee) REFERENCES Employee(ID_employee)
);