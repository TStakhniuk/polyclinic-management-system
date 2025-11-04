USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sysobjects 
WHERE name = 'check_id_employee' AND type = 'TR') 
    DROP TRIGGER check_id_employee

IF EXISTS (SELECT name FROM sysobjects 
WHERE name = 'check_id_patient' AND type = 'TR') 
    DROP TRIGGER check_id_patient

IF EXISTS (SELECT name FROM sysobjects 
WHERE name = 'check_consultation_overlap' AND type = 'TR') 
    DROP TRIGGER check_consultation_overlap
GO

-- Employee identification code verification
CREATE TRIGGER check_id_employee 
ON Employee
FOR INSERT, UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 
               FROM inserted 
               WHERE LEN(ID_employee) <> 10)
    BEGIN
        RAISERROR('Ідентифікаційний код має містити 10 цифр.', 16, 1)
        ROLLBACK
    END
END
GO

-- Patient ID code verification
CREATE TRIGGER check_id_patient 
ON Patient
FOR INSERT, UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 
               FROM inserted 
               WHERE LEN(ID_patient) <> 10)
    BEGIN
        RAISERROR('Ідентифікаційний код має містити 10 цифр.', 16, 1)
        ROLLBACK
    END
END
GO

-- Check for overlap of doctor's appointment times
CREATE TRIGGER check_consultation_overlap 
ON Consultation_report
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Consultation_report c ON i.ID_employee = c.ID_employee
        WHERE (
            (i.Start_date_time >= c.Start_date_time AND i.Start_date_time < c.End_date_time) OR
            (i.End_date_time > c.Start_date_time AND i.End_date_time <= c.End_date_time) OR
            (i.Start_date_time <= c.Start_date_time AND i.End_date_time >= c.End_date_time)
        )
          AND i.ID_consultation <> c.ID_consultation
    )
    BEGIN
        RAISERROR('На цей час у лікаря вже є інший запис!', 16, 1)
        ROLLBACK
    END
END
GO