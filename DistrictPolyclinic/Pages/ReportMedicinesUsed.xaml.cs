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
    public partial class ReportMedicinesUsed : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public ReportMedicinesUsed(DateTime startDate, DateTime endDate)
        {
            InitializeComponent();
            LoadReport(startDate, endDate);
        }

        private void LoadReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                reportViewerControl.ProcessingMode = ProcessingMode.Local;

                string reportPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "ReportMedicinesUsed.rdlc");
                reportViewerControl.LocalReport.ReportPath = reportPath;

                DataSet ds = new DataSet();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(@"
                        SELECT * 
                        FROM vw_AdministeredDrugs
                        WHERE Start_date_time BETWEEN @StartDate AND @EndDate", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate);
                    adapter.Fill(ds, "vw_AdministeredDrugs");

                    if (ds.Tables["vw_AdministeredDrugs"].Rows.Count == 0)
                    {
                        MessageBox.Show("Немає даних про застосовані медикаменти за вказаний період!", "Інформація");
                        //this.Dispatcher.InvokeAsync(() => this.Close());
                        return;
                    }
                }

                ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables["vw_AdministeredDrugs"]);
                reportViewerControl.LocalReport.DataSources.Clear();
                reportViewerControl.LocalReport.DataSources.Add(rds);

                reportViewerControl.LocalReport.SetParameters(new ReportParameter[]
                {
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
