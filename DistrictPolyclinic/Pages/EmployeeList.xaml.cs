using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EmployeeList.xaml
    /// </summary>
    public partial class EmployeeList : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private int currentPage = 1;
        private int pageSize = 12;
        private int filteredTotalRecords = 0;
        private string selectedPositionFilter = null;

        public EmployeeList()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
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

        public class EmployeeViewModel
        {
            public string ID { get; set; }
            public int Number { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Position { get; set; }
            public string Specialization { get; set; }
            public string Status { get; set; }
            public string Office { get; set; }

        }

        private void LoadEmployees()
        {
            try
            {
                var employees = new ObservableCollection<EmployeeViewModel>();
                int offset = (currentPage - 1) * pageSize;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string countQuery = selectedPositionFilter == null
                        ? "SELECT COUNT(*) FROM Employee"
                        : "SELECT COUNT(*) FROM Employee WHERE Type_employee = @Position";

                    SqlCommand countCmd = new SqlCommand(countQuery, connection);
                    if (selectedPositionFilter != null)
                        countCmd.Parameters.AddWithValue("@Position", selectedPositionFilter);

                    filteredTotalRecords = (int)countCmd.ExecuteScalar();

                    string query = selectedPositionFilter == null
                        ? $@"
                    SELECT 
                        E.ID_employee,
                        E.Last_name,
                        E.First_name,
                        E.Patronymic,
                        E.Phone_number,
                        E.Email,
                        E.Status_employee,
                        E.Type_employee,
                        S.Name_specialization AS Specialization,
                        CASE 
                            WHEN E.Type_employee = 'Лікар' AND O.Office_number IS NOT NULL THEN 
                                CONCAT('№', O.Office_number, ' - ', O.Office_name)
                            ELSE NULL 
                        END AS Office
                    FROM Employee E
                    LEFT JOIN Doctor D ON E.ID_employee = D.ID_employee
                    LEFT JOIN Specialization S ON D.ID_specialization = S.ID_specialization
                    LEFT JOIN Workplace W ON D.ID_employee = W.ID_employee
                    LEFT JOIN Office O ON W.Office_number = O.Office_number
                    ORDER BY 
                        CASE 
                            WHEN E.Status_employee = 'Активний' THEN 0 
                            ELSE 1 
                        END,
                        E.Last_name
                    OFFSET {offset} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY"
                        : $@"
                    SELECT 
                        E.ID_employee,
                        E.Last_name,
                        E.First_name,
                        E.Patronymic,
                        E.Phone_number,
                        E.Email,
                        E.Status_employee,
                        E.Type_employee,
                        S.Name_specialization AS Specialization,
                        CASE 
                            WHEN E.Type_employee = 'Лікар' AND O.Office_number IS NOT NULL THEN 
                                CONCAT('№', O.Office_number, ' - ', O.Office_name)
                            ELSE NULL 
                        END AS Office
                    FROM Employee E
                    LEFT JOIN Doctor D ON E.ID_employee = D.ID_employee
                    LEFT JOIN Specialization S ON D.ID_specialization = S.ID_specialization
                    LEFT JOIN Workplace W ON D.ID_employee = W.ID_employee
                    LEFT JOIN Office O ON W.Office_number = O.Office_number
                    WHERE E.Type_employee = @Position
                    ORDER BY 
                        CASE 
                            WHEN E.Status_employee = 'Активний' THEN 0 
                            ELSE 1 
                        END,
                        E.Last_name
                    OFFSET {offset} ROWS
                    FETCH NEXT {pageSize} ROWS ONLY";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    if (selectedPositionFilter != null)
                        cmd.Parameters.AddWithValue("@Position", selectedPositionFilter);

                    SqlDataReader reader = cmd.ExecuteReader();
                    int counter = offset + 1;
                    while (reader.Read())
                    {
                        employees.Add(new EmployeeViewModel
                        {
                            ID = reader["ID_employee"]?.ToString(),
                            Number = counter++,
                            FullName = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}",
                            Phone = reader["Phone_number"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            Status = reader["Status_employee"]?.ToString(),
                            Position = reader["Type_employee"]?.ToString(),
                            Specialization = string.IsNullOrEmpty(reader["Specialization"]?.ToString()) ? "-" : reader["Specialization"]?.ToString(),
                            Office = string.IsNullOrEmpty(reader["Office"]?.ToString()) ? "-" : reader["Office"]?.ToString()
                        });
                    }
                }

                EmployeeDataGrid.ItemsSource = employees;
                TotalRecordsTextBlock.Text = $"Всього {filteredTotalRecords} записів";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні працівників:\n{ex.Message}", "Помилка");
            }
        }


        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Content.ToString() == "Всі працівники")
                {
                    selectedPositionFilter = null;
                }
                else
                {
                    selectedPositionFilter = selectedItem.Content.ToString();
                }

                currentPage = 1;
                LoadEmployees();
            }
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            LoadEmployees();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadEmployees();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = GetTotalPages();
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadEmployees();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = GetTotalPages();
            LoadEmployees();
        }

        private int GetTotalPages()
        {
            return (int)Math.Ceiling(filteredTotalRecords / (double)pageSize);
        }
    }
}
