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
using DistrictPolyclinic.Services;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for ChangePasswordForm.xaml
    /// </summary>
    public partial class ChangePasswordForm : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = txtOldPassword.Password;
            string newPassword = txtNewPassword.Password;
            string repeatPassword = txtReplay.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(repeatPassword))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка!");
                return;
            }

            if (newPassword != repeatPassword)
            {
                MessageBox.Show("Новий пароль і його повторення не співпадають!", "Помилка!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string usernameCondition;
                    SqlCommand checkCmd;

                    if (SessionData.UserRole == "Адміністратор")
                    {
                        usernameCondition = "Username = 'administrator'";
                        checkCmd = new SqlCommand($"SELECT Password FROM DPUser WHERE {usernameCondition}", connection);
                    }
                    else
                    {
                        usernameCondition = "ID_employee = @ID";
                        checkCmd = new SqlCommand($"SELECT Password FROM DPUser WHERE {usernameCondition}", connection);
                        checkCmd.Parameters.AddWithValue("@ID", SessionData.EmployeeID);
                    }

                    object result = checkCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Користувача не знайдено!", "Помилка!");
                        return;
                    }

                    string currentPassword = result.ToString();
                    if (currentPassword != oldPassword)
                    {
                        MessageBox.Show("Старий пароль введено неправильно!", "Помилка!");
                        return;
                    }

                    if (currentPassword == newPassword)
                    {
                        MessageBox.Show("Новий пароль не може бути таким самим як поточний!", "Помилка!");
                        return;
                    }

                    SqlCommand updateCmd = new SqlCommand($"UPDATE DPUser SET Password = @NewPassword WHERE {usernameCondition}", connection);
                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    if (SessionData.UserRole != "Адміністратор")
                        updateCmd.Parameters.AddWithValue("@ID", SessionData.EmployeeID);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пароль успішно змінено!", "Інформація!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Помилка при зміні пароля!", "Помилка!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення до бази даних:\n{ex.Message}", "Помилка!");
            }
        }


    }
}
