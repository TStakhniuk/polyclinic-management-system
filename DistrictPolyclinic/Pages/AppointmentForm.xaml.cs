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

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for AppointmentForm.xaml
    /// </summary>
    public partial class AppointmentForm : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private Dictionary<string, string> patientIdMap = new Dictionary<string, string>();
        private Dictionary<string, (string EmployeeId, string MedicalCardId)> doctorIdMap = new Dictionary<string, (string, string)>();
        public Action OnAppointmentSaved { get; set; }


        public AppointmentForm(DateTime date, TimeSpan startTime, string doctorId)
        {
            InitializeComponent();
            LoadPatients();
            LoadDoctors(doctorId);
            SetDefaultDateTime();
            dpAppointmentDate.SelectedDate = date;
            tpStartTime.Value = date.Date + startTime;
        }

        private void tpStartTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tpStartTime.Value.HasValue)
            {
                DateTime startTime = tpStartTime.Value.Value;
                tpEndTime.Value = startTime.AddMinutes(30);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SetDefaultDateTime()
        {
            dpAppointmentDate.SelectedDate = DateTime.Today;

            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var endTime = startTime.AddHours(1);

            tpStartTime.Value = startTime;
            tpEndTime.Value = endTime;
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

        private void LoadDoctors(string selectedDoctorId = null)
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

                            // If this is the selected doctor — select in ComboBox
                            if (selectedDoctorId != null && id == selectedDoctorId)
                            {
                                cmbDoctor.SelectedItem = fullName;
                            }
                        }
                    }
                }
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedItem == null || cmbDoctor.SelectedItem == null || !dpAppointmentDate.SelectedDate.HasValue || !tpStartTime.Value.HasValue || !tpEndTime.Value.HasValue)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка!");
                return;
            }

            DateTime startTime = tpStartTime.Value.Value;
            DateTime endTime = tpEndTime.Value.Value;

            if (startTime >= endTime)
            {
                MessageBox.Show("Час завершення має бути пізніше часу початку!", "Помилка!");
                return;
            }

            DateTime selectedDate = dpAppointmentDate.SelectedDate.Value;

            // Check for weekends (Saturday or Sunday)
            if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("Не можна створити запис на вихідні (суботу або неділю).", "Помилка!");
                return;
            }

            // Check working hours (08:00 - 17:00)
            TimeSpan workStart = new TimeSpan(8, 0, 0); 
            TimeSpan workEnd = new TimeSpan(17, 0, 0); 

            if (startTime.TimeOfDay < workStart || endTime.TimeOfDay > workEnd)
            {
                MessageBox.Show("Запис можливий лише з 08:00 до 17:00!", "Помилка!");
                return;
            }

            string selectedPatient = cmbPatient.SelectedItem.ToString();
            string selectedDoctor = cmbDoctor.SelectedItem.ToString();
            string patientId = patientIdMap[selectedPatient];
            string doctorId = doctorIdMap[selectedDoctor].EmployeeId;
            string medicalCardId = GetMedicalCardId(patientId);

            DateTime appointmentStartDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, startTime.Hour, startTime.Minute, 0);
            DateTime appointmentEndDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, endTime.Hour, endTime.Minute, 0);

            // RECORDS IN THE PAST
            DateTime now = DateTime.Now;
            if (appointmentStartDateTime < now)
            {
                MessageBox.Show("Не можна створити запис у минулому!", "Помилка!");
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
                AND CAST(Start_date_time AS DATE) = @SelectedDate
                AND (
                    (@NewStartTime < End_date_time AND @NewEndTime > Start_date_time)
                )";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, checkConn))
                {
                    checkCmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    checkCmd.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                    checkCmd.Parameters.AddWithValue("@NewStartTime", appointmentStartDateTime);
                    checkCmd.Parameters.AddWithValue("@NewEndTime", appointmentEndDateTime);

                    int overlapCount = (int)checkCmd.ExecuteScalar();

                    if (overlapCount > 0)
                    {
                        MessageBox.Show("На цей час у лікаря вже є інший запис!", "Помилка!");
                        return;
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Add to Appointment_record
                    string insertAppointment = "INSERT INTO Appointment_record (ID_patient, ID_employee, Date_time) VALUES (@PatientId, @DoctorId, @DateTime)";
                    using (SqlCommand cmd = new SqlCommand(insertAppointment, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PatientId", patientId);
                        cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                        cmd.Parameters.AddWithValue("@DateTime", appointmentStartDateTime);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Generate ID_consultation
                    string idConsultation = GenerateConsultationId(conn, transaction, selectedDate);

                    // 3. Add to Consultation_report
                    string insertConsultation = @"
                INSERT INTO Consultation_report (ID_consultation, Start_date_time, End_date_time, ID_medical_card, ID_employee)
                VALUES (@IdConsultation, @StartTime, @EndTime, @MedicalCardId, @DoctorId)";
                    using (SqlCommand cmd = new SqlCommand(insertConsultation, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IdConsultation", idConsultation);
                        cmd.Parameters.AddWithValue("@StartTime", appointmentStartDateTime);
                        cmd.Parameters.AddWithValue("@EndTime", new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, endTime.Hour, endTime.Minute, 0));
                        cmd.Parameters.AddWithValue("@MedicalCardId", medicalCardId);
                        cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                        cmd.ExecuteNonQuery();
                    }

                    // 4. Get the doctor's office number
                    string officeNumber = GetOfficeNumberForDoctor(conn, transaction, doctorId);

                    transaction.Commit();

                    // 5. Output a successful message with the consultation number and office
                    MessageBox.Show($"Запис успішно додано!\nНомер запису: {idConsultation}, кабінет №: {officeNumber}!", "Інформація!");

                    ResetFields();
                    OnAppointmentSaved?.Invoke();
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Помилка при збереженні: " + ex.Message, "Помилка!");
                }
            }
        }

        private string GenerateConsultationId(SqlConnection conn, SqlTransaction transaction, DateTime selectedDate)
        {
            DateTime startOfDay = selectedDate.Date;
            DateTime endOfDay = startOfDay.AddDays(1);

            string query = @"
        SELECT ID_consultation 
        FROM Consultation_report 
        WHERE Start_date_time >= @StartOfDay AND Start_date_time < @EndOfDay";

            List<int> existingNumbers = new List<int>();

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@StartOfDay", startOfDay);
                cmd.Parameters.AddWithValue("@EndOfDay", endOfDay);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        string[] parts = id.Split('-');
                        if (parts.Length > 0 && int.TryParse(parts[0], out int num))
                        {
                            existingNumbers.Add(num);
                        }
                    }
                }
            }

            int newNumber = 1;
            existingNumbers.Sort();
            foreach (int number in existingNumbers)
            {
                if (number != newNumber)
                    break;
                newNumber++;
            }

            return $"{newNumber.ToString("D3")}-{startOfDay:ddMMyyyy}";
        }


        private string GetMedicalCardId(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID_medical_card FROM Medical_card WHERE ID_patient = @PatientId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PatientId", patientId);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        throw new Exception("Не знайдено медичну картку для цього пацієнта!");
                    }
                }
            }
        }

        private string GetOfficeNumberForDoctor(SqlConnection conn, SqlTransaction transaction, string doctorId)
        {
            string query = "SELECT Office_number FROM Workplace WHERE ID_employee = @DoctorId";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    throw new Exception("Для цього лікаря не закріплений кабінет!");
                }
            }
        }

        private void ResetFields()
        {
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            SetDefaultDateTime();
        }
    }
}
