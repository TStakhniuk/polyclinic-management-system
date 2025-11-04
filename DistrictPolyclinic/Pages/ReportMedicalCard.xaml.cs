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
    /// Interaction logic for ReportMedicalCard.xaml
    /// </summary>
    public partial class ReportMedicalCard : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public ReportMedicalCard(string patientId)
        {
            InitializeComponent();
            LoadReport(patientId);
        }

        private void LoadReport(string patientId)
        {
            try
            {
                reportViewerControl.ProcessingMode = ProcessingMode.Local;

                string reportPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "ReportMedicalCard.rdlc");
                reportViewerControl.LocalReport.ReportPath = reportPath;

                DataSet ds = new DataSet();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(
                        @"SELECT * FROM vw_MedicalCard 
                          WHERE ID_patient = @PatientID 
                            AND (End_date_time IS NULL OR End_date_time < GETDATE()) 
                          ORDER BY Start_date_time", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@PatientID", patientId);
                    adapter.Fill(ds, "vw_MedicalCard");

                    if (ds.Tables["vw_MedicalCard"].Rows.Count == 0)
                        MessageBox.Show("Немає даних для цього пацієнта!", "Інформація!");
                }

                ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["vw_MedicalCard"]);
                reportViewerControl.LocalReport.DataSources.Clear();
                reportViewerControl.LocalReport.DataSources.Add(rds);

                reportViewerControl.LocalReport.SetParameters(new ReportParameter("PatientID", patientId));

                reportViewerControl.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні звіту: " + ex.Message, "Помилка!");
            }
        }
    }
}
