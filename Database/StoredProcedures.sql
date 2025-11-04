USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sysobjects 
WHERE name = 'add_employee' AND type = 'P') 
    DROP PROCEDURE add_employee

IF EXISTS (SELECT name FROM sysobjects 
WHERE name = 'update_patient' AND type = 'P') 
    DROP PROCEDURE update_patient
GO

-- Procedure for adding an employee
CREATE PROCEDURE add_employee
    @ID_employee CHAR(10),
    @Last_name NVARCHAR(50),
    @First_name NVARCHAR(50),
    @Patronymic NVARCHAR(50),
    @Phone_number VARCHAR(20),
    @Email VARCHAR(100),
    @Status_employee NVARCHAR(50),
    @Type_employee NVARCHAR(50)
AS
BEGIN
    INSERT INTO Employee (ID_employee, Last_name, First_name, Patronymic, Phone_number, Email, Status_employee, Type_employee)
    VALUES (@ID_employee, @Last_name, @First_name, @Patronymic, @Phone_number, @Email, @Status_employee, @Type_employee)
END
GO

-- Patient data editing procedure
CREATE PROCEDURE update_patient
    @ID_patient CHAR(10),
    @Last_name NVARCHAR(50),
    @First_name NVARCHAR(50),
    @Patronymic NVARCHAR(50),
    @Gender NVARCHAR(50),
    @Date_birth DATE,
    @Home_address NVARCHAR(255),
    @Phone_number VARCHAR(20),
    @Email VARCHAR(100),
    @Status_patient NVARCHAR(50)
AS
BEGIN
    UPDATE Patient
    SET 
        Last_name = @Last_name,
        First_name = @First_name,
        Patronymic = @Patronymic,
        Gender = @Gender,
        Date_birth = @Date_birth,
        Home_address = @Home_address,
        Phone_number = @Phone_number,
        Email = @Email,
        Status_patient = @Status_patient
    WHERE ID_patient = @ID_patient
END
GO
