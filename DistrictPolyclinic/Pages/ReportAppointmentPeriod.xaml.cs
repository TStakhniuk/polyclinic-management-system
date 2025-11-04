using System;
using System.Collections.Generic;
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
using Microsoft.Reporting.WinForms;
using System.Data;
using System.Windows.Forms.Integration;
using System.Collections;
using System.Configuration;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for ReportAppointmentPeriod.xaml
    /// </summary>
    public partial class ReportAppointmentPeriod : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public ReportAppointmentPeriod(string doctorID, DateTime startDate, DateTime endDate)
        {
            InitializeComponent();
            LoadReport(doctorID, startDate, endDate);
        }

        private void LoadReport(string doctorID, DateTime startDate, DateTime endDate)
        {
            try
            {
                reportViewerControl.ProcessingMode = ProcessingMode.Local;

                string reportPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "ReportAppointmentPeriod.rdlc");
                reportViewerControl.LocalReport.ReportPath = reportPath;

                DataSet ds = new DataSet();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(@"
                    SELECT 
                    DoctorID,
	                DoctorName,
	                Specialization,
	                ConsultationID,
                    StartDate,
                    EndDate,
	                PatientName,
	                MedicalCardNumber,
                    (SELECT SUM(AppointmentCount)
                     FROM vw_DoctorAppointments
                     WHERE DoctorID = @DoctorID 
                       AND StartDate BETWEEN @StartDate AND @EndDate) AS AppointmentCount
                FROM vw_DoctorAppointments
                WHERE DoctorID = @DoctorID
                  AND StartDate BETWEEN @StartDate AND @EndDate
                ORDER BY StartDate;", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@DoctorID", doctorID);
                    adapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                    adapter.Fill(ds, "vw_DoctorAppointments");

                    if (ds.Tables["vw_DoctorAppointments"].Rows.Count == 0)
                        MessageBox.Show("Немає даних про прийоми для обраного лікаря за вказаний період!", "Інформація!");
                }

                ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["vw_DoctorAppointments"]);
                reportViewerControl.LocalReport.DataSources.Clear();
                reportViewerControl.LocalReport.DataSources.Add(rds);

                reportViewerControl.LocalReport.SetParameters(new ReportParameter[]
                {
                new ReportParameter("DoctorID", doctorID),
                new ReportParameter("StartDate", startDate.ToShortDateString()),
                new ReportParameter("EndDate", endDate.ToShortDateString())
                });

                reportViewerControl.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні звіту: " + ex.Message, "Помилка!");
            }
        }
    }
}
