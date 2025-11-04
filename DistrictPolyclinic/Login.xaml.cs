using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using DistrictPolyclinic.Pages;
using DistrictPolyclinic.Services;

namespace DistrictPolyclinic
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, введіть логін та пароль!", "Помилка!");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                            SELECT D.Role, D.IsActive, D.ID_employee, 
                                   E.Last_name, E.First_name
                            FROM DPUser D
                            LEFT JOIN Employee E ON D.ID_employee = E.ID_employee
                            WHERE D.Username = @Username AND D.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["IsActive"].ToString();
                                string role = reader["Role"].ToString();

                                if (status != "Активний")
                                {
                                    MessageBox.Show("Ваш обліковий запис неактивний!", "Доступ заборонено!");
                                    return;
                                }

                                SessionData.UserRole = role;
                                SessionData.EmployeeID = reader["ID_employee"] != DBNull.Value
                                    ? reader["ID_employee"].ToString()
                                    : null;

                                if (reader["Last_name"] != DBNull.Value && reader["First_name"] != DBNull.Value)
                                    SessionData.FullName = $"{reader["Last_name"]} {reader["First_name"]}";
                                else
                                    SessionData.FullName = "Адміністратор";

                                // Successful login
                                MainMenu menu = new MainMenu(role);
                                this.Hide();
                                txtUser.Clear();
                                txtPass.Clear();

                                menu.Closed += (s, args) => this.Show();
                                menu.Show();
                            }

                            else
                            {
                                MessageBox.Show("Невірний логін або пароль!", "Помилка!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення до БД:\n{ex.Message}", "Помилка!");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
