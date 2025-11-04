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
    /// Interaction logic for PatientList.xaml
    /// </summary>
    public partial class PatientList : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private int currentPage = 1;
        private int pageSize = 12;
        private int totalRecords = 0;
        private string searchText = "";

        public PatientList()
        {
            InitializeComponent();
            LoadPatients();
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

        public class PatientModel
        {
            public int Number { get; set; }
            public string ID { get; set; }
            public string FullName => $"{LastName} {FirstName} {Patronymic}";
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string Patronymic { get; set; }
            public string Gender { get; set; }
            public DateTime BirthDate { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string MedicalCardNumber { get; set; }
        }

        private void LoadPatients(string searchText = "")
        {
            List<PatientModel> patients = new List<PatientModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SqlCommand countCommand;
                    SqlCommand command;

                    int startRow = ((currentPage - 1) * pageSize) + 1;
                    int endRow = startRow + pageSize;

                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        countCommand = new SqlCommand("SELECT COUNT(*) FROM Patient", connection);

                        command = new SqlCommand(@"
                    SELECT *
                    FROM (
                        SELECT 
                            ROW_NUMBER() OVER (
                                ORDER BY 
                                    CASE WHEN P.Status_patient = N'Активний' THEN 0 ELSE 1 END,
                                    P.Last_name
                            ) AS RowNum,
                            P.ID_patient,
                            P.Last_name,
                            P.First_name,
                            P.Patronymic,
                            P.Gender,
                            P.Date_birth,
                            P.Home_address,
                            P.Phone_number,
                            P.Email,
                            P.Status_patient,
                            MC.ID_medical_card
                        FROM Patient P
                        LEFT JOIN Medical_card MC ON P.ID_patient = MC.ID_patient
                    ) AS RowConstrainedResult
                    WHERE RowNum >= @StartRow AND RowNum < @EndRow
                    ORDER BY RowNum", connection);
                    }
                    else
                    {
                        countCommand = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM Patient 
                    WHERE CONCAT(Last_name, ' ', First_name, ' ', Patronymic) LIKE @SearchText", connection);
                        countCommand.Parameters.AddWithValue("@SearchText", $"%{searchText}%");

                        command = new SqlCommand(@"
                    SELECT *
                    FROM (
                        SELECT 
                            ROW_NUMBER() OVER (
                                ORDER BY 
                                    CASE WHEN P.Status_patient = N'Активний' THEN 0 ELSE 1 END,
                                    CASE 
                                        WHEN CONCAT(P.Last_name, ' ', P.First_name, ' ', P.Patronymic) LIKE @SearchText THEN 0 
                                        ELSE 1 
                                    END,
                                    P.Last_name
                            ) AS RowNum,
                            P.ID_patient,
                            P.Last_name,
                            P.First_name,
                            P.Patronymic,
                            P.Gender,
                            P.Date_birth,
                            P.Home_address,
                            P.Phone_number,
                            P.Email,
                            P.Status_patient,
                            MC.ID_medical_card
                        FROM Patient P
                        LEFT JOIN Medical_card MC ON P.ID_patient = MC.ID_patient
                        WHERE CONCAT(P.Last_name, ' ', P.First_name, ' ', P.Patronymic) LIKE @SearchText
                    ) AS RowConstrainedResult
                    WHERE RowNum >= @StartRow AND RowNum < @EndRow
                    ORDER BY RowNum", connection);

                        command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                    }

                    // General parameters
                    command.Parameters.AddWithValue("@StartRow", startRow);
                    command.Parameters.AddWithValue("@EndRow", endRow);

                    totalRecords = (int)countCommand.ExecuteScalar();

                    SqlDataReader reader = command.ExecuteReader();
                    int number = startRow;

                    while (reader.Read())
                    {
                        patients.Add(new PatientModel
                        {
                            Number = number++,
                            ID = reader["ID_patient"].ToString(),
                            LastName = reader["Last_name"].ToString(),
                            FirstName = reader["First_name"].ToString(),
                            Patronymic = reader["Patronymic"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            BirthDate = Convert.ToDateTime(reader["Date_birth"]),
                            Address = reader["Home_address"].ToString(),
                            Phone = string.IsNullOrWhiteSpace(reader["Phone_number"].ToString()) ? "-" : reader["Phone_number"].ToString(),
                            Email = string.IsNullOrWhiteSpace(reader["Email"].ToString()) ? "-" : reader["Email"].ToString(),
                            Status = reader["Status_patient"].ToString(),
                            MedicalCardNumber = "№" + reader["ID_medical_card"].ToString()
                        });
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка завантаження пацієнтів: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            PatientDataGrid.ItemsSource = patients;
            TotalRecordsTextBlock.Text = $"Всього {totalRecords} записів";
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentPage = 1; // reset to the first page
            LoadPatients(txtSearch.Text.Trim());
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            LoadPatients();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPatients();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < totalRecords)
            {
                currentPage++;
                LoadPatients();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = (int)Math.Ceiling((double)totalRecords / pageSize);
            LoadPatients();
        }
    }
}
