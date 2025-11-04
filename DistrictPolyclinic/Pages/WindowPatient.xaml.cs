using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Configuration;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for WindowPatient.xaml
    /// </summary>
    public partial class WindowPatient : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        public WindowPatient()
        {
            InitializeComponent();
            LoadPatients();
        }
        private void LoadPatients()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ID_patient, Last_name + ' ' + First_name + ' ' + Patronymic AS FullName FROM Patient", conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbPatient.ItemsSource = dt.DefaultView;
                    cmbPatient.DisplayMemberPath = "FullName";
                    cmbPatient.SelectedValuePath = "ID_patient";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження пацієнтів: " + ex.Message);
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatient.SelectedValue != null)
            {
                string patientId = cmbPatient.SelectedValue.ToString();
                ReportMedicalCard reportWindow = new ReportMedicalCard(patientId);
                reportWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Оберіть пацієнта зі списку!", "Помилка!");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}