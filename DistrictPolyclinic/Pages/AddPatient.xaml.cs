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
    /// Interaction logic for AddPatient.xaml
    /// </summary>
    public partial class AddPatient : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public AddPatient()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainMenu;
            if (mainWindow != null)
            {
                mainWindow.ResetSelectedSubButton();    
                mainWindow.CollapseAllSubmenus();          
                mainWindow.msNews.IsChecked = true;      
                mainWindow.frameContent.Navigate(new News()); 
            }
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            string idCode = txtIdCode.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            DateTime? birthDate = dpBirthDate.SelectedDate;
            string address = txtAddress.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            string gender = (cmbGender.SelectedItem as ComboBoxItem)?.Content.ToString();
            string bloodGroup = (cmbBloodGroup.SelectedItem as ComboBoxItem)?.Content.ToString();
            string chronicDiseases = txtChronicDiseases.Text.Trim();
            string allergies = txtAllergies.Text.Trim();

            if (string.IsNullOrWhiteSpace(idCode) ||
                string.IsNullOrWhiteSpace(fullName) ||
                birthDate == null ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phone) ||
                cmbGender.SelectedItem == null ||
                cmbBloodGroup.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля та оберіть необхідні параметри!", "Помилка!");
                return;
            }

            if (idCode.Length != 10 || !idCode.All(char.IsDigit))
            {
                MessageBox.Show("Ідентифікаційний код має містити 10 цифр.", "Помилка!");
                return;
            }

            string[] fullNameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fullNameParts.Length != 3)
            {
                MessageBox.Show("ПІБ має містити рівно три слова: Прізвище / Ім'я / По батькові.", "Помилка!");
                return;
            }

            string lastName = fullNameParts[0];
            string firstName = fullNameParts[1];
            string patronymic = fullNameParts[2];

            string status = "Активний";
            string medicalCardId = new string(idCode.Reverse().ToArray());
            DateTime startDate = DateTime.Now;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Check if a patient with this ID already exists
                    string checkQuery = "SELECT COUNT(*) FROM Patient WHERE ID_patient = @ID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@ID", idCode);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Пацієнт з таким ідентифікаційним кодом вже існує!", "Помилка!");
                            return;
                        }
                    }

                    // 2. Adding a patient
                    string insertPatient = @"INSERT INTO Patient 
                (ID_patient, Last_name, First_name, Patronymic, Gender, Date_birth, Home_address, Phone_number, Email, Status_patient) 
                VALUES 
                (@ID, @LastName, @FirstName, @Patronymic, @Gender, @BirthDate, @Address, @Phone, @Email, @Status)";
                    using (SqlCommand cmd = new SqlCommand(insertPatient, connection))
                    {
                        cmd.Parameters.AddWithValue("@ID", idCode);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@Patronymic", patronymic);
                        cmd.Parameters.AddWithValue("@Gender", gender);
                        cmd.Parameters.AddWithValue("@BirthDate", birthDate.Value);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Status", status);

                        cmd.ExecuteNonQuery();
                    }

                    // 3. Adding a medical card
                    string insertCard = @"INSERT INTO Medical_card 
                (ID_medical_card, Blood_type, Chronic_diseases, Allergies, Start_date, ID_patient) 
                VALUES 
                (@CardID, @Blood, @Chronic, @Allergy, @StartDate, @PatientID)";
                    using (SqlCommand cmd = new SqlCommand(insertCard, connection))
                    {
                        cmd.Parameters.AddWithValue("@CardID", medicalCardId);
                        cmd.Parameters.AddWithValue("@Blood", bloodGroup);
                        cmd.Parameters.AddWithValue("@Chronic", chronicDiseases);
                        cmd.Parameters.AddWithValue("@Allergy", allergies);
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@PatientID", idCode);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Пацієнта успішно зареєстровано!", "Інформація!");

                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні пацієнта: " + ex.Message, "Помилка!");
            }
        }

        private void ClearForm()
        {
            txtIdCode.Text = "";
            txtFullName.Text = "";
            dpBirthDate.SelectedDate = null;
            txtAddress.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            cmbGender.SelectedIndex = -1;
            cmbBloodGroup.SelectedIndex = -1;
            txtChronicDiseases.Text = "";
            txtAllergies.Text = "";
        }

    }
}
