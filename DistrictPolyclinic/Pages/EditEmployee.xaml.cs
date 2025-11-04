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
    /// Interaction logic for EditEmployee.xaml
    /// </summary>
    public partial class EditEmployee : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public EditEmployee()
        {
            InitializeComponent();
            LoadSpecializations();
            LoadOffices();
            cmbEmployee.IsEnabled = false;
            cmbSpecialization.IsEnabled = false;
            cmbOffice.IsEnabled = false;
        }

        // Loading into ComboBox
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

        private void cmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPosition.SelectedItem is ComboBoxItem selectedItem)
            {
                string position = selectedItem.Content.ToString();
                bool isDoctor = position == "Лікар";

                cmbEmployee.IsEnabled = true;
                cmbSpecialization.IsEnabled = isDoctor;
                cmbOffice.IsEnabled = isDoctor;

                if (!isDoctor)
                {
                    cmbSpecialization.SelectedIndex = -1;
                    cmbOffice.SelectedIndex = -1;
                }

                LoadEmployees(position);
            }
        }

        private void LoadEmployees(string type)
        {
            cmbEmployee.Items.Clear();

            string query = "SELECT ID_employee, Last_name, First_name, Patronymic FROM Employee WHERE Type_employee = @type";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@type", type);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string id = reader["ID_employee"].ToString();
                        string pib = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}";

                        cmbEmployee.Items.Add(new ComboBoxItem
                        {
                            Content = pib,
                            Tag = id
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження працівників: {ex.Message}");
                }
            }
        }

        private bool ValidateBeforeEdit()
        {
            if (cmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Оберіть працівника для редагування.", "Помилка!");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                string[] parts = txtFullName.Text.Trim().Split(' ');
                if (parts.Length != 3)
                {
                    MessageBox.Show("ПІБ повинен містити прізвище, ім’я та по батькові.", "Помилка!");
                    return false;
                }
            }

            return true;
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
            if (!ValidateBeforeEdit())
                return;

            string id = ((ComboBoxItem)cmbEmployee.SelectedItem).Tag.ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Update Employee
                    string updateEmployeeQuery = "UPDATE Employee SET ";
                    List<string> updates = new List<string>();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;

                    if (!string.IsNullOrWhiteSpace(txtFullName.Text))
                    {
                        string[] parts = txtFullName.Text.Trim().Split(' ');
                        updates.Add("Last_name = @ln");
                        updates.Add("First_name = @fn");
                        updates.Add("Patronymic = @pt");
                        cmd.Parameters.AddWithValue("@ln", parts[0]);
                        cmd.Parameters.AddWithValue("@fn", parts[1]);
                        cmd.Parameters.AddWithValue("@pt", parts[2]);
                    }

                    if (!string.IsNullOrWhiteSpace(txtPhone.Text))
                    {
                        updates.Add("Phone_number = @phone");
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                    {
                        updates.Add("Email = @email");
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    }

                    if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                    {
                        updates.Add("Status_employee = @status");
                        cmd.Parameters.AddWithValue("@status", statusItem.Content.ToString());
                    }

                    if (updates.Count > 0)
                    {
                        updateEmployeeQuery += string.Join(", ", updates) + " WHERE ID_employee = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.CommandText = updateEmployeeQuery;
                        cmd.ExecuteNonQuery();
                    }

                    // Doctor Update
                    if (cmbPosition.SelectedItem is ComboBoxItem positionItem && positionItem.Content.ToString() == "Лікар")
                    {
                        if (cmbSpecialization.SelectedItem is ComboBoxItem specItem)
                        {
                            string specUpdate = "UPDATE Doctor SET ID_specialization = @spec WHERE ID_employee = @id";
                            SqlCommand specCmd = new SqlCommand(specUpdate, connection, transaction);
                            specCmd.Parameters.AddWithValue("@spec", specItem.Tag.ToString());
                            specCmd.Parameters.AddWithValue("@id", id);
                            specCmd.ExecuteNonQuery();
                        }

                        if (cmbOffice.SelectedItem is ComboBoxItem officeItem)
                        {
                            string checkWorkplace = "SELECT COUNT(*) FROM Workplace WHERE ID_employee = @id";
                            SqlCommand checkCmd = new SqlCommand(checkWorkplace, connection, transaction);
                            checkCmd.Parameters.AddWithValue("@id", id);
                            int count = (int)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                string updateWorkplace = "UPDATE Workplace SET Office_number = @office WHERE ID_employee = @id";
                                SqlCommand updateCmd = new SqlCommand(updateWorkplace, connection, transaction);
                                updateCmd.Parameters.AddWithValue("@office", officeItem.Tag.ToString());
                                updateCmd.Parameters.AddWithValue("@id", id);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Update DPUser
                    string dpUpdate = "UPDATE DPUser SET ";
                    List<string> dpUpdates = new List<string>();
                    SqlCommand dpCmd = new SqlCommand();
                    dpCmd.Connection = connection;
                    dpCmd.Transaction = transaction;

                    if (!string.IsNullOrWhiteSpace(txtLogin.Text))
                    {
                        dpUpdates.Add("Username = @username");
                        dpCmd.Parameters.AddWithValue("@username", txtLogin.Text.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        dpUpdates.Add("Password = @pass");
                        dpCmd.Parameters.AddWithValue("@pass", txtPassword.Text.Trim());
                    }

                    if (cmbStatus.SelectedItem is ComboBoxItem dpStatusItem)
                    {
                        dpUpdates.Add("IsActive = @active");
                        dpCmd.Parameters.AddWithValue("@active", dpStatusItem.Content.ToString());
                    }

                    if (dpUpdates.Count > 0)
                    {
                        dpUpdate += string.Join(", ", dpUpdates) + " WHERE ID_employee = @id";
                        dpCmd.Parameters.AddWithValue("@id", id);
                        dpCmd.CommandText = dpUpdate;
                        dpCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Дані успішно оновлено!", "Інформація!");
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Помилка під час оновлення: {ex.Message}");
                }
            }
        }

        private void ClearForm()
        {
            cmbPosition.SelectedIndex = -1;
            cmbEmployee.Items.Clear();
            cmbEmployee.IsEnabled = false;

            txtFullName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            cmbStatus.SelectedIndex = -1;

            cmbSpecialization.SelectedIndex = -1;
            cmbSpecialization.IsEnabled = false;

            cmbOffice.SelectedIndex = -1;
            cmbOffice.IsEnabled = false;

            txtLogin.Clear();
            txtPassword.Clear();
        }
    }
}
