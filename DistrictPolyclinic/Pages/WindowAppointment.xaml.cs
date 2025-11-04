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
    /// Interaction logic for WindowAppointment.xaml
    /// </summary>
    public partial class WindowAppointment : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        public WindowAppointment()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                SELECT E.ID_employee, Last_name + ' ' + First_name + ' ' + Patronymic AS FullName
                FROM Employee E
                JOIN Doctor D ON E.ID_employee = D.ID_employee", conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbDoctor.ItemsSource = dt.DefaultView;
                    cmbDoctor.DisplayMemberPath = "FullName";
                    cmbDoctor.SelectedValuePath = "ID_employee";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження лікарів: " + ex.Message);
            }
        }


        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDoctor.SelectedValue == null || dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Оберіть лікаря та період!", "Помилка!");
                return;
            }

            DateTime startDate = dpStartDate.SelectedDate.Value.Date; 
            DateTime endDate = dpEndDate.SelectedDate.Value.Date;
            DateTime today = DateTime.Today;

            if (startDate > endDate)
            {
                MessageBox.Show("Початкова дата не може бути пізніше кінцевої!", "Помилка!");
                return;
            }

            if (endDate > today)
            {
                MessageBox.Show("Кінцева дата не може бути пізніше сьогоднішнього дня!", "Помилка!");
                return;
            }

            if (endDate == today)
            {
                endDate = DateTime.Now;
            }
            else
            {
                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            string doctorID = cmbDoctor.SelectedValue.ToString();

            var reportWindow = new ReportAppointmentPeriod(doctorID, startDate, endDate);
            reportWindow.Show();
            this.Close();
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
