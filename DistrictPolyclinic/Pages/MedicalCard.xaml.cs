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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for MedicalCard.xaml
    /// </summary>
    public partial class MedicalCard : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private Dictionary<string, string> patientDict = new Dictionary<string, string>();
        private string preselectedPatient;

        public MedicalCard(string patientName = null)
        {
            InitializeComponent();
            preselectedPatient = patientName;
            LoadPatients();
        }

        private void LoadPatients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID_patient, Last_name + ' ' + First_name + ' ' + ISNULL(Patronymic, '') AS FullName FROM Patient";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string name = reader.GetString(1);
                    patientDict[name] = id;
                    cmbPatient.Items.Add(name);
                }
                reader.Close();
            }

            // Set the preselected patient after loading all patients
            if (!string.IsNullOrEmpty(preselectedPatient) && cmbPatient.Items.Contains(preselectedPatient))
            {
                cmbPatient.SelectedItem = preselectedPatient;
            }
        }

        private void cmbPatient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPatient.SelectedItem != null)
            {
                string fullName = cmbPatient.SelectedItem.ToString();
                string patientId = patientDict[fullName];
                LoadPatientDetails(patientId);
            }
        }

        private void LoadPatientDetails(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Request to retrieve patient's basic data
                string patientQuery = "SELECT Last_name, First_name, Patronymic, Gender, Date_birth, Home_address, Phone_number FROM Patient WHERE ID_patient = @PatientId";
                SqlCommand patientCmd = new SqlCommand(patientQuery, conn);
                patientCmd.Parameters.AddWithValue("@PatientId", patientId);

                SqlDataReader reader = patientCmd.ExecuteReader();
                if (reader.Read())
                {
                    txtFullName.Text = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}";
                    txtGender.Text = reader["Gender"].ToString();
                    txtBirthDate.Text = Convert.ToDateTime(reader["Date_birth"]).ToShortDateString();
                    txtAddress.Text = reader["Home_address"].ToString();
                    txtPhone.Text = reader["Phone_number"].ToString();
                }
                reader.Close();

                // Request to obtain the patient's medical record
                string cardQuery = "SELECT ID_medical_card, Blood_type, Chronic_diseases, Allergies, Start_date, End_date FROM Medical_card WHERE ID_patient = @PatientId";
                SqlCommand cardCmd = new SqlCommand(cardQuery, conn);
                cardCmd.Parameters.AddWithValue("@PatientId", patientId);

                reader = cardCmd.ExecuteReader();
                if (reader.Read())
                {
                    txtIDCard.Text = "№" + reader["ID_medical_card"].ToString();

                    txtBloodGroup.Text = reader["Blood_type"].ToString();
                    txtChronicDiseases.Text = reader["Chronic_diseases"].ToString();
                    txtAllergies.Text = reader["Allergies"].ToString();
                    txtCardOpening.Text = Convert.ToDateTime(reader["Start_date"]).ToShortDateString();

                    if (reader["End_date"] != DBNull.Value)
                    {
                        txtCardClosure.Text = Convert.ToDateTime(reader["End_date"]).ToShortDateString();
                    }
                    else
                    {
                        txtCardClosure.Text = "-"; 
                    }
                }
                reader.Close();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedItem != null && !string.IsNullOrEmpty(txtIDCard.Text))
            {
                var selectedPatient = cmbPatient.SelectedItem.ToString();
                var medicalCardId = txtIDCard.Text.Replace("№", "");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Consultation_report WHERE ID_medical_card = @CardId";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@CardId", medicalCardId);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        var mainWindow = Window.GetWindow(this) as MainMenu;
                        if (mainWindow != null)
                        {
                            mainWindow.frameContent.Navigate(new ResultMedicalCard(selectedPatient, "№" + medicalCardId));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Цей пацієнт ще не має жодного запису про прийом!", "Інформація!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть пацієнта!", "Помилка!");
            }
        }


        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedItem != null && !string.IsNullOrEmpty(txtIDCard.Text))
            {
                MessageBox.Show("Це перший запис!", "Інформація!");
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть пацієнта!", "Помилка!");
            }
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedItem != null && !string.IsNullOrEmpty(txtIDCard.Text))
            {
                MessageBox.Show("Ви вже на першому записі!", "Інформація!");
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть пацієнта!", "Помилка!");
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedItem != null && !string.IsNullOrEmpty(txtIDCard.Text))
            {
                var selectedPatient = cmbPatient.SelectedItem.ToString();
                var medicalCardId = txtIDCard.Text.Replace("№", "");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Consultation_report WHERE ID_medical_card = @CardId";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@CardId", medicalCardId);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        var mainWindow = Window.GetWindow(this) as MainMenu;
                        if (mainWindow != null)
                        {
                            mainWindow.frameContent.Navigate(new ResultMedicalCard(selectedPatient, "№" + medicalCardId, true));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Цей пацієнт ще не має жодного запису про прийом!", "Інформація!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть пацієнта!", "Помилка!");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainMenu;
            if (mainWindow != null)
            {
                mainWindow.ResetSelectedSubButton();         // Remove the backlight
                mainWindow.CollapseAllSubmenus();            // Collapse all submenus
                mainWindow.msNews.IsChecked = true;          // Activate News
                mainWindow.frameContent.Navigate(new News()); // Go to News
            }
        }
    }
}
