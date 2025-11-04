using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Timers;

namespace DistrictPolyclinic.Services
{
    public class AppointmentReminderService
    {
        private readonly Timer _timer;
        private readonly string _connectionString;

        public AppointmentReminderService(string connectionString, double intervalMs = 180000) // 3 min
        {
            _connectionString = connectionString;

            _timer = new Timer(intervalMs);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckAndSendReminders();
        }

        private void CheckAndSendReminders()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    p.Email, 
                    p.Phone_number,
                    p.Last_name + ' ' + p.First_name + ' ' + ISNULL(p.Patronymic, '') AS FullName,
                    ar.Date_time,
                    cr.ID_consultation,
                    e.Last_name + ' ' + e.First_name + ' ' + ISNULL(e.Patronymic, '') AS DoctorName,
                    s.Name_specialization,
                    o.Office_number,
                    o.Office_name
                FROM Appointment_record ar
                JOIN Patient p ON p.ID_patient = ar.ID_patient
                JOIN Doctor d ON d.ID_employee = ar.ID_employee
                JOIN Employee e ON e.ID_employee = d.ID_employee
                JOIN Workplace w ON w.ID_employee = d.ID_employee
                JOIN Office o ON o.Office_number = w.Office_number
                JOIN Specialization s ON s.ID_specialization = d.ID_specialization
                JOIN Consultation_report cr ON cr.ID_employee = ar.ID_employee AND cr.Start_date_time = ar.Date_time
                WHERE 
                    DATEDIFF(MINUTE, GETDATE(), ar.Date_time) BETWEEN 59 AND 61";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string email = reader["Email"]?.ToString();
                            string fullName = reader["FullName"].ToString();
                            string doctorName = reader["DoctorName"].ToString();
                            string spec = reader["Name_specialization"].ToString();
                            string office = reader["Office_number"].ToString() + " - " + reader["Office_name"].ToString();
                            string consultationId = reader["ID_consultation"].ToString();

                            if (!string.IsNullOrEmpty(email))
                                SendEmailReminder(email, fullName, consultationId, doctorName, spec, office);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час надсилання нагадування: {ex.Message}");
            }
        }


        private void SendEmailReminder(string toEmail, string patientName, string consultationId, string doctorName, string spec, string office)
        {
            try
            {
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPassword"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

                var msg = new MailMessage(smtpUser, toEmail)
                {
                    Subject = "Нагадування про прийом!",
                    Body = $"Шановний(а) {patientName}, через годину у Вас прийом №{consultationId} у лікаря {doctorName} ({spec}) у кабінеті №{office}."
                };

                var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося відправити листа: {ex.Message}");
            }
        }
    }
}
