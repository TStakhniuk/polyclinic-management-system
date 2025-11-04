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
    /// Interaction logic for EditPatient.xaml
    /// </summary>
    public partial class EditPatient : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private Dictionary<string, string> patientDict = new Dictionary<string, string>();

        public EditPatient()
        {
            InitializeComponent();
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string address = txtAddress.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            string chronicDiseases = txtChronicDiseases.Text.Trim();
            string allergies = txtAllergies.Text.Trim();
            DateTime? endDate = dpCardСlosure.SelectedDate;

            if (cmbPatient.SelectedItem == null)
            {
                MessageBox.Show("Оберіть пацієнта для редагування.", "Помилка!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                string[] nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length != 3)
                {
                    MessageBox.Show("ПІБ повинен містити прізвище, ім’я та по батькові.", "Помилка!");
                    return;
                }
            }

            if (status == "Активний" && endDate != null)
            {
                MessageBox.Show("Неможливо встановити статус 'Активний', якщо вказана дата закриття картки.", "Помилка!");
                return;
            }

            string selectedPatient = cmbPatient.SelectedItem.ToString();
            if (!patientDict.TryGetValue(selectedPatient, out string patientId))
            {
                MessageBox.Show("Не вдалося визначити ID пацієнта.", "Помилка!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                SqlTransaction transaction = conn.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    if (!string.IsNullOrWhiteSpace(fullName))
                    {
                        string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string last = parts[0];
                        string first = parts[1];
                        string patronymic = parts[2];

                        cmd.CommandText = @"UPDATE Patient SET Last_name = @last, First_name = @first, Patronymic = @patronymic WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@last", last);
                        cmd.Parameters.AddWithValue("@first", first);
                        cmd.Parameters.AddWithValue("@patronymic", patronymic);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        cmd.CommandText = "UPDATE Patient SET Home_address = @address WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@address", address);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        cmd.CommandText = "UPDATE Patient SET Phone_number = @phone WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        cmd.CommandText = "UPDATE Patient SET Email = @mail WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@mail", email);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        cmd.CommandText = "UPDATE Patient SET Status_patient = @status WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        if (status == "Активний")
                        {
                            cmd.CommandText = "UPDATE Medical_card SET End_date = NULL WHERE ID_patient = @id";
                            cmd.Parameters.AddWithValue("@id", patientId);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(chronicDiseases))
                    {
                        cmd.CommandText = "UPDATE Medical_card SET Chronic_diseases = @cd WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@cd", chronicDiseases);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (!string.IsNullOrWhiteSpace(allergies))
                    {
                        cmd.CommandText = "UPDATE Medical_card SET Allergies = @all WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@all", allergies);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    if (endDate != null)
                    {
                        cmd.CommandText = "UPDATE Medical_card SET End_date = @endDate WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@endDate", endDate.Value);
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();

                        // Automatically set the status
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Patient SET Status_patient = 'Неактивний' WHERE ID_patient = @id";
                        cmd.Parameters.AddWithValue("@id", patientId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Інформація про пацієнта успішно відредаговано!", "Інформація!");
                    cmbPatient.Items.Clear();
                    patientDict.Clear();
                    LoadPatients();
                    ClearAllFields();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Помилка при редагуванні: " + ex.Message);
                }
            }
        }


        private void ClearAllFields()
        {
            cmbPatient.SelectedItem = null;
            txtFullName.Text = "";
            txtAddress.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            cmbStatus.SelectedItem = null;
            txtChronicDiseases.Text = "";
            txtAllergies.Text = "";
            dpCardСlosure.SelectedDate = null;
        }
    }
}
