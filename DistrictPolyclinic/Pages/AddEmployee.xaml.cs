using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Configuration;


namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for AddEmployee.xaml
    /// </summary>
    public partial class AddEmployee : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public AddEmployee()
        {
            InitializeComponent();
            LoadSpecializations();
            LoadOffices();
            cmbSpecialization.IsEnabled = false;
            cmbOffice.IsEnabled = false;
        }

        private void LoadSpecializations()
        {
            string query = "SELECT ID_specialization, Name_specialization FROM Specialization";
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cmbSpecialization.Items.Add(new ComboBoxItem
                    {
                        Content = reader["Name_specialization"].ToString(),
                        Tag = reader["ID_specialization"]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження спеціалізацій: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void LoadOffices()
        {
            string query = "SELECT Office_number, Office_name FROM Office";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    cmbOffice.Items.Clear();
                    while (reader.Read())
                    {
                        string officeNumber = reader["Office_number"].ToString();
                        string officeName = reader["Office_name"].ToString();

                        cmbOffice.Items.Add(new ComboBoxItem
                        {
                            Content = $"№{officeNumber} - {officeName}",
                            Tag = officeNumber
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження кабінетів: {ex.Message}");
                }
            }
        }

        // Processing changes in ComboBox "Position"
        private void cmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isDoctor = cmbPosition.SelectedItem != null &&
                    ((ComboBoxItem)cmbPosition.SelectedItem).Content.ToString() == "Лікар";

            cmbSpecialization.IsEnabled = isDoctor;
            cmbOffice.IsEnabled = isDoctor;

            if (!isDoctor)
            {
                cmbSpecialization.SelectedIndex = -1;
                cmbOffice.SelectedIndex = -1;
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string idCode = txtIdCode.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                if (string.IsNullOrWhiteSpace(idCode) ||
                    string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    cmbPosition.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtLogin.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Будь ласка, заповніть всі поля та оберіть необхідні параметри.", "Помилка!");
                    return;
                }

                string position = ((ComboBoxItem)cmbPosition.SelectedItem).Content.ToString();

                if (position == "Лікар")
                {
                    if (cmbSpecialization.SelectedItem == null)
                    {
                        MessageBox.Show("Будь ласка, оберіть спеціалізацію для лікаря.", "Помилка!");
                        return;
                    }

                    if (cmbOffice.SelectedItem == null)
                    {
                        MessageBox.Show("Будь ласка, оберіть кабінет для лікаря.", "Помилка!");
                        return;
                    }
                }

                if (idCode.Length != 10 || !idCode.All(char.IsDigit))
                {
                    MessageBox.Show("Ідентифікаційний код повинен містити рівно 10 цифр.", "Помилка!");
                    return;
                }

                string[] fullNameParts = fullName.Split(' ');
                if (fullNameParts.Length != 3)
                {
                    MessageBox.Show("ПІБ має містити рівно три слова: Прізвище / Ім'я / По батькові.", "Помилка!");
                    return;
                }

                string lastName = fullNameParts[0];
                string firstName = fullNameParts[1];
                string patronymic = fullNameParts[2];

                // Checking the uniqueness of the ID
                using (SqlConnection checkConnection = new SqlConnection(connectionString))
                {
                    checkConnection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Employee WHERE ID_employee = @ID";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, checkConnection);
                    checkCommand.Parameters.AddWithValue("@ID", idCode);
                    int count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Працівник з таким ідентифікаційним кодом вже існує.", "Помилка!");
                        return;
                    }
                }

                // Checking the uniqueness of the login
                using (SqlConnection checkLoginConnection = new SqlConnection(connectionString))
                {
                    checkLoginConnection.Open();
                    string checkLoginQuery = "SELECT COUNT(*) FROM DPUser WHERE Username = @Username";
                    SqlCommand checkLoginCommand = new SqlCommand(checkLoginQuery, checkLoginConnection);
                    checkLoginCommand.Parameters.AddWithValue("@Username", txtLogin.Text.Trim());
                    int loginCount = (int)checkLoginCommand.ExecuteScalar();
                    if (loginCount > 0)
                    {
                        MessageBox.Show("Користувач з таким логіном вже існує. Виберіть інший логін.", "Помилка!");
                        return;
                    }
                }

                connection.Open();

                // Adding an employee
                string employeeQuery = "INSERT INTO Employee (ID_employee, Last_name, First_name, Patronymic, Phone_number, Email, Status_employee, Type_employee) " +
                                       "VALUES (@ID_employee, @Last_name, @First_name, @Patronymic, @Phone_number, @Email, 'Активний', @Type_employee)";
                SqlCommand employeeCommand = new SqlCommand(employeeQuery, connection);
                employeeCommand.Parameters.AddWithValue("@ID_employee", idCode);
                employeeCommand.Parameters.AddWithValue("@Last_name", lastName);
                employeeCommand.Parameters.AddWithValue("@First_name", firstName);
                employeeCommand.Parameters.AddWithValue("@Patronymic", patronymic);
                employeeCommand.Parameters.AddWithValue("@Phone_number", phone);
                employeeCommand.Parameters.AddWithValue("@Email", email);
                employeeCommand.Parameters.AddWithValue("@Type_employee", position);
                employeeCommand.ExecuteNonQuery();

                // Adding to a specialized table
                if (position == "Лікар")
                {
                    string specializationId = ((ComboBoxItem)cmbSpecialization.SelectedItem).Tag.ToString();
                    string doctorQuery = "INSERT INTO Doctor (ID_employee, ID_specialization) VALUES (@ID_employee, @ID_specialization)";
                    SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection);
                    doctorCommand.Parameters.AddWithValue("@ID_employee", idCode);
                    doctorCommand.Parameters.AddWithValue("@ID_specialization", specializationId);
                    doctorCommand.ExecuteNonQuery();

                    // Adding to Workplace
                    string officeNumber = ((ComboBoxItem)cmbOffice.SelectedItem).Tag.ToString();
                    string workplaceQuery = "INSERT INTO Workplace (ID_employee, Office_number) VALUES (@ID_employee, @Office_number)";
                    SqlCommand workplaceCommand = new SqlCommand(workplaceQuery, connection);
                    workplaceCommand.Parameters.AddWithValue("@ID_employee", idCode);
                    workplaceCommand.Parameters.AddWithValue("@Office_number", officeNumber);
                    workplaceCommand.ExecuteNonQuery();
                }
                else if (position == "Працівник реєстратури")
                {
                    string registryQuery = "INSERT INTO Registry_employee (ID_employee) VALUES (@ID_employee)";
                    SqlCommand registryCommand = new SqlCommand(registryQuery, connection);
                    registryCommand.Parameters.AddWithValue("@ID_employee", idCode);
                    registryCommand.ExecuteNonQuery();
                }

                // Adding to DPUser
                string userQuery = "INSERT INTO DPUser (ID_employee, Username, Password, IsActive, Role) " +
                                   "VALUES (@ID_employee, @Username, @Password, 'Активний', @Role)";
                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@ID_employee", idCode);
                userCommand.Parameters.AddWithValue("@Username", txtLogin.Text.Trim());
                userCommand.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                userCommand.Parameters.AddWithValue("@Role", position == "Лікар" ? "Лікар" : "Працівник реєстратури");
                userCommand.ExecuteNonQuery();

                MessageBox.Show("Працівника успішно додано!", "Інформація!");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання працівника: {ex.Message}", "Помилка!");
            }
            finally
            {
                connection.Close();
            }
        }

        private void ClearForm()
        {
            txtIdCode.Clear();
            txtFullName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtLogin.Clear();
            txtPassword.Clear();
            cmbPosition.SelectedIndex = -1;
            cmbSpecialization.SelectedIndex = -1;
            cmbSpecialization.IsEnabled = false;
            cmbOffice.SelectedIndex = -1;
            cmbOffice.IsEnabled = false;
        }

    }
}
