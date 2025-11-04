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
    /// Interaction logic for ResultMedicalCard.xaml
    /// </summary>
    /// 
    public class AdministeredDrugViewModel
    {
        public string Name { get; set; }
        public string DosageDisplay { get; set; }
        public string QuantityDisplay { get; set; }
    }

    public partial class ResultMedicalCard : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private Dictionary<string, string> patientDict = new Dictionary<string, string>();
        private string selectedPatient;
        private string medicalCardId;
        private List<string> consultationIds = new List<string>();
        private int currentIndex = 0;

        public ResultMedicalCard(string patientName, string idCard, bool showLastRecord = false)
        {
            InitializeComponent();
            selectedPatient = patientName;
            medicalCardId = idCard;
            LoadPatients();
            LoadConsultationIds();

            if (showLastRecord)
            {
                currentIndex = consultationIds.Count - 1; // Set the index to the last record
            }
            LoadConsultationRecord();
        }

        private void LoadPatients()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID_patient, Last_name + ' ' + First_name + ' ' + ISNULL(Patronymic, '') AS FullName FROM Patient";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string name = reader.GetString(1);
                    patientDict[name] = id;
                    cmbPatient.Items.Add(name);
                }
                reader.Close();

                if (cmbPatient.Items.Contains(selectedPatient))
                {
                    cmbPatient.SelectedItem = selectedPatient;
                    cmbPatient.IsEnabled = false;
                }
            }
        }

        private void LoadConsultationIds()
        {
            consultationIds.Clear();
            string cleanedCardId = medicalCardId.Replace("№", "").Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID_consultation FROM Consultation_report WHERE ID_medical_card = @IDCard ORDER BY Start_date_time";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IDCard", cleanedCardId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    consultationIds.Add(reader.GetString(0));
                }
            }
            currentIndex = 0; // on the first record
        }

        private void LoadConsultationRecord()
        {
            string consultationId = consultationIds[currentIndex];
            string cleanedCardId = medicalCardId.Replace("№", "").Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT 
            c.ID_consultation, c.Start_date_time, c.End_date_time, 
            c.Complaints, c.Diagnosis, c.Conclusion,
            e.Last_name, e.First_name, ISNULL(e.Patronymic, ''),
            d.Drug_name, d.Release_form, 
            d.Quantity_unit, d.Dosage_unit,
            a.Dosage, a.Quantity_used
        FROM Consultation_report c
        JOIN Employee e ON c.ID_employee = e.ID_employee
        JOIN Medical_card m ON c.ID_medical_card = m.ID_medical_card
        LEFT JOIN Administered_drugs a ON c.ID_consultation = a.ID_consultation
        LEFT JOIN Medicinal_product d ON a.ID_drug = d.ID_drug
        WHERE c.ID_consultation = @ID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", consultationId);

                var administeredDrugs = new List<AdministeredDrugViewModel>();
                string start = "", end = "", complaints = "", diagnosis = "", conclusion = "", doctor = "";

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (start == "")
                        {
                            start = reader.GetDateTime(1).ToString("dd.MM.yyyy HH:mm");
                            end = reader.GetDateTime(2).ToString("HH:mm");
                            complaints = reader.IsDBNull(3) ? "-" : reader.GetString(3);
                            diagnosis = reader.IsDBNull(4) ? "-" : reader.GetString(4);
                            conclusion = reader.IsDBNull(5) ? "-" : reader.GetString(5);
                            doctor = $"{reader.GetString(6)} {reader.GetString(7)} {reader.GetString(8)}";
                        }

                        if (!reader.IsDBNull(9))
                        {
                            var drug = new AdministeredDrugViewModel
                            {
                                Name = $"{reader.GetString(9)} ({reader.GetString(10)})",
                                DosageDisplay = $"{reader.GetInt32(13)} {reader.GetString(12)}",
                                QuantityDisplay = $"{reader.GetInt32(14)} {reader.GetString(11)}"
                            };
                            administeredDrugs.Add(drug);
                        }
                    }
                }

                txtIDCard.Text = $"№{cleanedCardId}";
                txtNumberAppointment.Text = $"№{consultationId}";
                txtDate.Text = $"{start}-{end}";
                txtComplaints.Text = complaints;
                txtDiagnosis.Text = diagnosis;
                txtConclusion.Text = conclusion;
                txtDoctor.Text = doctor;

                if (administeredDrugs.Count == 0)
                {
                    administeredDrugs.Add(new AdministeredDrugViewModel
                    {
                        Name = "-",
                        DosageDisplay = "",
                        QuantityDisplay = ""
                    });
                }

                lstMedicinal.ItemsSource = administeredDrugs;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex == 0)
            {
                var mainWindow = Window.GetWindow(this) as MainMenu;
                mainWindow?.frameContent.Navigate(new MedicalCard(selectedPatient));
            }
            else if (currentIndex == 1)
            {
                currentIndex = 0;
                LoadConsultationRecord();
            }
            else
            {
                currentIndex--;
                LoadConsultationRecord();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < consultationIds.Count - 1)
            {
                currentIndex++;
                LoadConsultationRecord();
            }
            else
            {
                MessageBox.Show("Це останній запис!", "Інформація!");
            }
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainMenu;
            if (mainWindow != null)
            {
                mainWindow.frameContent.Navigate(new MedicalCard(selectedPatient));
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex == consultationIds.Count - 1)
            {
                MessageBox.Show("Ви вже на останньому записі!", "Інформація!");
            }
            else
            {
                currentIndex = consultationIds.Count - 1;
                LoadConsultationRecord();
            }
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
    }
}
