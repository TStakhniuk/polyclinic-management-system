using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DistrictPolyclinic.Pages.Appointment;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for EditAppointmentForm.xaml
    /// </summary>
    public partial class EditAppointmentForm : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private Dictionary<string, string> patientIdMap = new Dictionary<string, string>();
        private Dictionary<string, (string EmployeeId, string MedicalCardId)> doctorIdMap = new Dictionary<string, (string, string)>();
        public DoctorAppointment Appointment { get; set; }
        public Action OnAppointmentSaved { get; set; }

        public EditAppointmentForm(DoctorAppointment appointment)
        {
            InitializeComponent();
            LoadPatients();
            LoadDoctors();
            Appointment = appointment;
            cmbPatient.IsEnabled = false;
            cmbDoctor.IsEnabled = false;
            dpAppointmentDate.IsEnabled = false;

            cmbPatient.SelectedItem = cmbPatient.Items
                .Cast<string>()
                .FirstOrDefault(p => patientIdMap[p] == Appointment.PatientId);

            cmbDoctor.SelectedItem = cmbDoctor.Items
                .Cast<string>()
                .FirstOrDefault(d => doctorIdMap[d].EmployeeId == Appointment.DoctorId);

            dpAppointmentDate.SelectedDate = Appointment.StartDateTime.Date;
            tpStartTime.Value = Appointment.StartDateTime;
            tpEndTime.Value = Appointment.EndDateTime;
        }

        private void tpStartTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tpStartTime.Value.HasValue)
            {
                DateTime startTime = tpStartTime.Value.Value;
                tpEndTime.Value = startTime.AddMinutes(30);
            }
        }

        private void LoadPatients()
        {
            patientIdMap.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID_patient, Last_name, First_name, Patronymic FROM Patient WHERE Status_patient = 'Активний'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader["ID_patient"].ToString();
                        string fullName = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}";
                        cmbPatient.Items.Add(fullName);
                        patientIdMap[fullName] = id;
                    }
                }
            }
        }

        private void LoadDoctors()
        {
            doctorIdMap.Clear();
            cmbDoctor.Items.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT e.ID_employee, e.Last_name, e.First_name, e.Patronymic
                FROM Employee e
                JOIN Doctor d ON e.ID_employee = d.ID_employee
                WHERE e.Status_employee = 'Активний'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader["ID_employee"].ToString();
                        string fullName = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}";

                        if (!doctorIdMap.ContainsKey(fullName))
                        {
                            doctorIdMap[fullName] = (id, null);
                            cmbDoctor.Items.Add(fullName);
                        }
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви дійсно хочете видалити цей запис?", "Підтвердження", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string deleteAppointment = @"DELETE FROM Appointment_record 
                                          WHERE ID_patient = @patientId 
                                            AND ID_employee = @doctorId 
                                            AND Date_time = @dateTime";

                    using (SqlCommand cmd = new SqlCommand(deleteAppointment, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@patientId", Appointment.PatientId);
                        cmd.Parameters.AddWithValue("@doctorId", Appointment.DoctorId);
                        cmd.Parameters.AddWithValue("@dateTime", Appointment.StartDateTime);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteConsultation = @"DELETE FROM Consultation_report 
                                          WHERE ID_consultation = @id";

                    using (SqlCommand cmd = new SqlCommand(deleteConsultation, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", Appointment.ConsultationId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Запис успішно видалено!", "Інформація!");
                    
                    OnAppointmentSaved?.Invoke();
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Помилка при видаленні: " + ex.Message, "Помилка!");
                }
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!tpStartTime.Value.HasValue || !tpEndTime.Value.HasValue)
            {
                MessageBox.Show("Будь ласка, час прийому!", "Помилка!");
                return;
            }

            DateTime newStart = dpAppointmentDate.SelectedDate.Value.Date
                                + tpStartTime.Value.Value.TimeOfDay;

            DateTime newEnd = dpAppointmentDate.SelectedDate.Value.Date
                              + tpEndTime.Value.Value.TimeOfDay;

            if (newEnd <= newStart)
            {
                MessageBox.Show("Час завершення має бути пізніше часу початку!", "Помилка!");
                return;
            }

            // Check working hours (08:00 - 17:00)
            TimeSpan workStart = new TimeSpan(8, 0, 0);
            TimeSpan workEnd = new TimeSpan(17, 0, 0);

            if (newStart.TimeOfDay < workStart || newEnd.TimeOfDay > workEnd)
            {
                MessageBox.Show("Запис можливий лише з 08:00 до 17:00!", "Помилка!");
                return;
            }

            // Check for record intersection
            using (SqlConnection checkConn = new SqlConnection(connectionString))
            {
                checkConn.Open();

                string checkQuery = @"
SELECT COUNT(*) 
FROM Consultation_report
WHERE ID_employee = @DoctorId
    AND ID_consultation != @CurrentConsultationId
    AND CAST(Start_date_time AS DATE) = @SelectedDate
    AND (
        (@NewStartTime < End_date_time AND @NewEndTime > Start_date_time)
    )";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, checkConn))
                {
                    checkCmd.Parameters.AddWithValue("@DoctorId", Appointment.DoctorId);
                    checkCmd.Parameters.AddWithValue("@CurrentConsultationId", Appointment.ConsultationId);
                    checkCmd.Parameters.AddWithValue("@SelectedDate", newStart.Date);
                    checkCmd.Parameters.AddWithValue("@NewStartTime", newStart);
                    checkCmd.Parameters.AddWithValue("@NewEndTime", newEnd);

                    int overlapCount = (int)checkCmd.ExecuteScalar();

                    if (overlapCount > 0)
                    {
                        MessageBox.Show("На цей час у лікаря вже є інший запис!", "Помилка!");
                        return;
                    }
                }
            }

            // Update the record
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string updateAppointment = @"UPDATE Appointment_record
                                 SET Date_time = @newDateTime
                                 WHERE ID_patient = @patientId 
                                   AND ID_employee = @doctorId 
                                   AND Date_time = @oldDateTime";

                    using (SqlCommand cmd = new SqlCommand(updateAppointment, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@newDateTime", newStart);
                        cmd.Parameters.AddWithValue("@patientId", Appointment.PatientId);
                        cmd.Parameters.AddWithValue("@doctorId", Appointment.DoctorId);
                        cmd.Parameters.AddWithValue("@oldDateTime", Appointment.StartDateTime);
                        cmd.ExecuteNonQuery();
                    }

                    string updateConsultation = @"UPDATE Consultation_report
                                  SET Start_date_time = @start,
                                      End_date_time = @end
                                  WHERE ID_consultation = @id";

                    using (SqlCommand cmd = new SqlCommand(updateConsultation, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@start", newStart);
                        cmd.Parameters.AddWithValue("@end", newEnd);
                        cmd.Parameters.AddWithValue("@id", Appointment.ConsultationId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Запис оновлено!", "Інформація!");
                    
                    OnAppointmentSaved?.Invoke();
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Помилка при оновленні: " + ex.Message, "Помилка!");
                }
            }
        }

    }
}
