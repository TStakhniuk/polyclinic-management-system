USE DistrictPolyclinic
GO

IF EXISTS (SELECT name FROM sys.objects
 WHERE name = 'vw_MedicalCard' AND type_desc = 'VIEW')
DROP VIEW vw_MedicalCard

IF EXISTS (SELECT name FROM sys.objects
 WHERE name = 'vw_DoctorAppointments ' AND type_desc = 'VIEW')
DROP VIEW vw_DoctorAppointments

IF EXISTS (SELECT name FROM sys.objects
 WHERE name = 'vw_AdministeredDrugs  ' AND type_desc = 'VIEW')
DROP VIEW vw_AdministeredDrugs

GO

-- Patient's medical record
CREATE VIEW vw_MedicalCard AS
SELECT
    p.ID_patient,
    p.Last_name, p.First_name, p.Patronymic,
    p.Gender, p.Date_birth,
    p.Home_address, p.Phone_number, p.Email,
    
    mc.ID_medical_card,
    mc.Blood_type, mc.Chronic_diseases, mc.Allergies,
    mc.Start_date AS Card_start_date,
    mc.End_date AS Card_end_date,

    cr.ID_consultation,
    cr.Start_date_time, cr.End_date_time,
    cr.Complaints, cr.Diagnosis, cr.Conclusion,

    e.Last_name + ' ' + e.First_name + ' ' + e.Patronymic AS Doctor_name,

    mp.ATC_code,
    mp.Drug_name,
    mp.Release_form,
    ad.Dosage,
    mp.Dosage_unit,
    ad.Quantity_used,
    mp.Quantity_unit

FROM Patient p
JOIN Medical_card mc ON p.ID_patient = mc.ID_patient
LEFT JOIN Consultation_report cr ON mc.ID_medical_card = cr.ID_medical_card
LEFT JOIN Employee e ON cr.ID_employee = e.ID_employee
LEFT JOIN Administered_drugs ad ON cr.ID_consultation = ad.ID_consultation
LEFT JOIN Medicinal_product mp ON ad.ID_drug = mp.ID_drug

GO

-- Doctor's appointments
CREATE VIEW vw_DoctorAppointments AS
SELECT 
    d.ID_employee AS DoctorID,
    e.Last_name + ' ' + e.First_name + ' ' + e.Patronymic AS DoctorName,
    s.Name_specialization AS Specialization,
    cr.ID_consultation AS ConsultationID,
	cr.Start_date_time AS StartDate,
    cr.End_date_time AS EndDate,
    p.Last_name + ' ' + p.First_name + ' ' + p.Patronymic AS PatientName,
    mc.ID_medical_card AS MedicalCardNumber,
    COUNT(cr.ID_consultation) AS AppointmentCount
FROM Doctor d
JOIN Employee e ON d.ID_employee = e.ID_employee
JOIN Specialization s ON d.ID_specialization = s.ID_specialization
JOIN Consultation_report cr ON d.ID_employee = cr.ID_employee
JOIN Medical_card mc ON cr.ID_medical_card = mc.ID_medical_card
JOIN Patient p ON mc.ID_patient = p.ID_patient
GROUP BY d.ID_employee, e.Last_name, e.First_name, e.Patronymic, s.Name_specialization, cr.ID_consultation, cr.Start_date_time, cr.End_date_time, p.Last_name, p.First_name, p.Patronymic, mc.ID_medical_card;

GO

-- Medications used
CREATE VIEW vw_AdministeredDrugs AS
SELECT 
    cr.ID_consultation,
    cr.Start_date_time,
    e.Last_name + ' ' + e.First_name + ' ' + e.Patronymic AS DoctorName,
    mp.ATC_code,
    mp.Drug_name,
    mp.Release_form,
    ad.Dosage,
    mp.Dosage_unit,
    ad.Quantity_used,
    mp.Quantity_unit
FROM Administered_drugs ad
JOIN Consultation_report cr ON ad.ID_consultation = cr.ID_consultation
JOIN Employee e ON cr.ID_employee = e.ID_employee
JOIN Medicinal_product mp ON ad.ID_drug = mp.ID_drug;