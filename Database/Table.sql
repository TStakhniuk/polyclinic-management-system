USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Administered_drugs' AND type_desc = 'USER_TABLE') 
    DROP TABLE Administered_drugs
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Consultation_report' AND type_desc = 'USER_TABLE') 
    DROP TABLE Consultation_report
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Appointment_record' AND type_desc = 'USER_TABLE') 
    DROP TABLE Appointment_record
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Workplace' AND type_desc = 'USER_TABLE') 
    DROP TABLE Workplace
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Doctor' AND type_desc = 'USER_TABLE') 
    DROP TABLE Doctor
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Registry_employee' AND type_desc = 'USER_TABLE') 
    DROP TABLE Registry_employee
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Medical_card' AND type_desc = 'USER_TABLE') 
    DROP TABLE Medical_card
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Specialization' AND type_desc = 'USER_TABLE') 
    DROP TABLE Specialization
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Office' AND type_desc = 'USER_TABLE') 
    DROP TABLE Office
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Medicinal_product' AND type_desc = 'USER_TABLE') 
    DROP TABLE Medicinal_product
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Employee' AND type_desc = 'USER_TABLE') 
    DROP TABLE Employee
IF EXISTS (SELECT name FROM sys.objects WHERE name = 'Patient' AND type_desc = 'USER_TABLE') 
    DROP TABLE Patient
GO

-- Creating tables
CREATE TABLE Patient (
    ID_patient CHAR(10) PRIMARY KEY NOT NULL,
    Last_name NVARCHAR(50) NOT NULL,
    First_name NVARCHAR(50) NOT NULL,
    Patronymic NVARCHAR(50),
    Gender NVARCHAR(50),
    Date_birth DATE NOT NULL,
    Home_address NVARCHAR(255),
    Phone_number VARCHAR(20),
    Email VARCHAR(100),
    Status_patient NVARCHAR(50) NOT NULL
)

CREATE TABLE Employee (
    ID_employee CHAR(10) PRIMARY KEY NOT NULL,
    Last_name NVARCHAR(50) NOT NULL,
    First_name NVARCHAR(50) NOT NULL,
    Patronymic NVARCHAR(50),
    Phone_number VARCHAR(20),
    Email VARCHAR(100),
    Status_employee NVARCHAR(50),
    Type_employee NVARCHAR(50) CHECK (Type_employee IN ('Лікар', 'Працівник реєстратури'))
)

CREATE TABLE Medicinal_product (
	ID_drug CHAR(3) PRIMARY KEY NOT NULL,
    ATC_code NVARCHAR(7) NOT NULL,
    Drug_name NVARCHAR(100) NOT NULL,
    Release_form NVARCHAR(50) NOT NULL,
	Dosage_unit NVARCHAR(20) NOT NULL,
    Quantity_unit NVARCHAR(20) NOT NULL
)

CREATE TABLE Office (
    Office_number CHAR(3) PRIMARY KEY NOT NULL,
    Office_name NVARCHAR(100) NOT NULL,
    Floor_office INT NOT NULL
)

CREATE TABLE Specialization (
    ID_specialization CHAR(2) PRIMARY KEY NOT NULL,
    Name_specialization NVARCHAR(100) NOT NULL 
)

CREATE TABLE Medical_card (
    ID_medical_card CHAR(10) PRIMARY KEY NOT NULL,
    Blood_type NVARCHAR(15),
    Chronic_diseases NVARCHAR(MAX),
    Allergies NVARCHAR(MAX),
    Start_date DATE NOT NULL,
    End_date DATE,
    ID_patient CHAR(10) NOT NULL,
    FOREIGN KEY (ID_patient) REFERENCES Patient(ID_patient)
)

CREATE TABLE Registry_employee (
    ID_employee CHAR(10) PRIMARY KEY NOT NULL,
    FOREIGN KEY (ID_employee) REFERENCES Employee(ID_employee)
)

CREATE TABLE Doctor (
    ID_employee CHAR(10) PRIMARY KEY NOT NULL,
    ID_specialization CHAR(2) NOT NULL,
    FOREIGN KEY (ID_employee) REFERENCES Employee(ID_employee),
    FOREIGN KEY (ID_specialization) REFERENCES Specialization(ID_specialization)
)

CREATE TABLE Workplace (
    ID_employee CHAR(10) NOT NULL,
    Office_number CHAR(3) NOT NULL,
    PRIMARY KEY (ID_employee, Office_number),
    FOREIGN KEY (ID_employee) REFERENCES Doctor(ID_employee),
    FOREIGN KEY (Office_number) REFERENCES Office(Office_number)
)

CREATE TABLE Appointment_record (
    ID_patient CHAR(10) NOT NULL,
    ID_employee CHAR(10) NOT NULL,
    Date_time DATETIME NOT NULL,
    PRIMARY KEY (ID_patient, ID_employee, Date_time),
    FOREIGN KEY (ID_patient) REFERENCES Patient(ID_patient),
    FOREIGN KEY (ID_employee) REFERENCES Doctor(ID_employee)
)

CREATE TABLE Consultation_report (
    ID_consultation CHAR(12) PRIMARY KEY NOT NULL,
    Start_date_time DATETIME NOT NULL,
    End_date_time DATETIME NOT NULL,
    Complaints NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    Conclusion NVARCHAR(MAX),
    ID_medical_card CHAR(10) NOT NULL,
    ID_employee CHAR(10) NOT NULL,
    FOREIGN KEY (ID_medical_card) REFERENCES Medical_card(ID_medical_card),
    FOREIGN KEY (ID_employee) REFERENCES Doctor(ID_employee)
)

CREATE TABLE Administered_drugs (
    ID_consultation CHAR(12) NOT NULL,
    ID_drug CHAR(3) NOT NULL,
    Dosage INT NOT NULL,
    Quantity_used INT NOT NULL,
    PRIMARY KEY (ID_consultation, ID_drug),
    FOREIGN KEY (ID_consultation) REFERENCES Consultation_report(ID_consultation),
    FOREIGN KEY (ID_drug) REFERENCES Medicinal_product(ID_drug)
)
