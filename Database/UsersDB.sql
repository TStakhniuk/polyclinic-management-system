USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'administrator' AND type_desc = 'SQL_LOGIN')
    DROP LOGIN administrator

IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'registry' AND type_desc = 'SQL_LOGIN')
    DROP LOGIN registry

IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'doctor' AND type_desc = 'SQL_LOGIN')
    DROP LOGIN doctor
GO

sp_addlogin
@loginame = 'administrator',
@passwd = 'administrator'
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'administrator' AND type_desc = 'SQL_USER')
BEGIN
    DROP SCHEMA administrator
    DROP USER administrator
END
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'Administrator')
    EXEC sp_droprole 'Administrator'
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'Registry')
    EXEC sp_droprole 'Registry'
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'Doctor')
    EXEC sp_droprole 'Doctor'
GO

sp_adduser
@loginame = 'administrator',
@name_in_db = 'administrator'
GO

sp_addrole 'Administrator'
GRANT SELECT ON Patient TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Employee TO Administrator
GRANT SELECT ON Medicinal_product TO Administrator
GRANT SELECT ON Office TO Administrator
GRANT SELECT ON Specialization TO Administrator
GRANT SELECT ON Medical_card TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Registry_employee TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Doctor TO Administrator
GRANT SELECT ON Workplace TO Administrator
GRANT SELECT ON Appointment_record TO Administrator
GRANT SELECT ON Consultation_report TO Administrator
GRANT SELECT ON Administered_drugs TO Administrator
GRANT SELECT ON vw_MedicalCard TO Administrator
GRANT SELECT ON vw_DoctorAppointments TO Administrator
GRANT SELECT ON vw_AdministeredDrugs TO Administrator
GO

sp_addrole 'Registry'
GRANT SELECT, INSERT, UPDATE, DELETE ON Patient TO Administrator
GRANT SELECT ON Employee TO Administrator
GRANT SELECT ON Medicinal_product TO Administrator
GRANT SELECT ON Office TO Administrator
GRANT SELECT ON Specialization TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Medical_card TO Administrator
GRANT SELECT ON Registry_employee TO Administrator
GRANT SELECT ON Doctor TO Administrator
GRANT SELECT ON Workplace TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Appointment_record TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Consultation_report TO Administrator
GRANT SELECTE ON Administered_drugs TO Administrator
GRANT SELECT ON vw_MedicalCard TO Administrator
GRANT SELECT ON vw_DoctorAppointments TO Administrator
GRANT SELECT ON vw_AdministeredDrugs TO Administrator
GO

sp_addrole 'Doctor'
GRANT SELECT ON Patient TO Administrator
GRANT SELECT ON Employee TO Administrator
GRANT SELECT ON Medicinal_product TO Administrator
GRANT SELECT ON Office TO Administrator
GRANT SELECT ON Specialization TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Medical_card TO Administrator
GRANT SELECT ON Registry_employee TO Administrator
GRANT SELECT ON Doctor TO Administrator
GRANT SELECT ON Workplace TO Administrator
GRANT SELECT ON Appointment_record TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Consultation_report TO Administrator
GRANT SELECT, INSERT, UPDATE, DELETE ON Administered_drugs TO Administrator
GRANT SELECT ON vw_MedicalCard TO Administrator
GRANT SELECT ON vw_DoctorAppointments TO Administrator
GRANT SELECT ON vw_AdministeredDrugs TO Administrator
GO

sp_addrolemember 'Administrator', 'administrator'
GO
