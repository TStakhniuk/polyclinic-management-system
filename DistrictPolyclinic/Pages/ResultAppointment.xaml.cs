using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for ResultAppointment.xaml
    /// </summary>
    public class AdministeredDrugTemp
    {
        public string ID_drug { get; set; }
        public string Name { get; set; }
        public string DosageDisplay { get; set; }
        public string QuantityDisplay { get; set; }
        public int Dosage { get; set; }
        public int Quantity { get; set; }
    }



    public partial class ResultAppointment : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private ObservableCollection<AdministeredDrugTemp> administeredDrugs = new ObservableCollection<AdministeredDrugTemp>();

        public ResultAppointment()
        {
            InitializeComponent();
            lstMedicinal.ItemsSource = administeredDrugs;
            LoadDoctors();
            LoadMedicinalProducts();
        }

        private void LoadDoctors()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT E.ID_employee, CONCAT(E.Last_name, ' ', E.First_name, ' ', E.Patronymic) AS FullName
              FROM Employee E
              INNER JOIN Doctor D ON E.ID_employee = D.ID_employee
              WHERE E.Status_employee = 'Активний'", connection);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbDoctor.ItemsSource = dt.DefaultView;
                cmbDoctor.DisplayMemberPath = "FullName";
                cmbDoctor.SelectedValuePath = "ID_employee";
            }
        }

        private void cmbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDoctor.SelectedValue is string doctorId)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT CONCAT('№', ID_consultation) AS DisplayText, ID_consultation
                        FROM Consultation_report
                        WHERE ID_employee = @ID
                          AND Start_date_time <= GETDATE() -- прийом вже розпочався
                          AND End_date_time > DATEADD(HOUR, -72, GETDATE())
                          AND (Complaints IS NULL OR Complaints = '')
                          AND (Diagnosis IS NULL OR Diagnosis = '')
                          AND (Conclusion IS NULL OR Conclusion = '')", connection);
                    cmd.Parameters.AddWithValue("@ID", doctorId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("У вибраного лікаря немає незаповнених записів прийомів!", "Інформація!");
                        cmbNumberAppointment.ItemsSource = null;
                        return;
                    }

                    cmbNumberAppointment.ItemsSource = dt.DefaultView;
                    cmbNumberAppointment.DisplayMemberPath = "DisplayText";
                    cmbNumberAppointment.SelectedValuePath = "ID_consultation";
                }
            }
        }


        private void LoadMedicinalProducts()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ID_drug, ATC_code + ' - ' + Drug_name + ' (' + Release_form + ')' AS FullName
              FROM Medicinal_product", connection);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                txtMedicinalProduct.ItemsSource = dt.DefaultView;
                txtMedicinalProduct.DisplayMemberPath = "FullName";
                txtMedicinalProduct.SelectedValuePath = "ID_drug";
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

        private void AddMedicinal_Click(object sender, RoutedEventArgs e)
        {
            if (txtMedicinalProduct.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtDosage.Text) ||
                string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Будь ласка, оберіть лікарський засіб та введіть дозу із кількістю!", "Інформація!");
                return;
            }

            if (!int.TryParse(txtDosage.Text, out int dosage) || dosage <= 0)
            {
                MessageBox.Show("Доза повинна бути додатнім числом!", "Помилка!");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Кількість повинна бути додатнім числом!", "Помилка!");
                return;
            }

            string drugId = txtMedicinalProduct.SelectedValue.ToString();

            if (administeredDrugs.Any(d => d.ID_drug == drugId))
            {
                MessageBox.Show("Такий лікарський засіб вже додано!", "Помилка!");
                return;
            }


            string drugName = "";
            string releaseForm = "";
            string dosageUnit = "";
            string quantityUnit = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT Drug_name, Release_form, Dosage_unit, Quantity_unit FROM Medicinal_product WHERE ID_drug = @ID", connection);
                cmd.Parameters.AddWithValue("@ID", drugId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        drugName = reader["Drug_name"].ToString();
                        releaseForm = reader["Release_form"].ToString();
                        dosageUnit = reader["Dosage_unit"].ToString();
                        quantityUnit = reader["Quantity_unit"].ToString();
                    }
                }
            }

            var item = new AdministeredDrugTemp
            {
                ID_drug = drugId,
                Name = $"{drugName} ({releaseForm})",
                Dosage = dosage,
                Quantity = quantity,
                DosageDisplay = $"{dosage} {dosageUnit}",
                QuantityDisplay = $"{quantity} {quantityUnit}"
            };

            administeredDrugs.Add(item);
            lstMedicinal.Items.Refresh();

            txtMedicinalProduct.SelectedIndex = -1;
            txtDosage.Clear();
            txtQuantity.Clear();
        }



        private void DeleteMedicinal_Click(object sender, RoutedEventArgs e)
        {
            if (lstMedicinal.SelectedItem is AdministeredDrugTemp selected)
            {
                administeredDrugs.Remove(selected);
                lstMedicinal.Items.Refresh();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string consultationId = cmbNumberAppointment.SelectedValue?.ToString();
            string complaints = txtComplaints.Text.Trim();
            string diagnosis = txtDiagnosis.Text.Trim();
            string conclusion = txtConclusion.Text.Trim();

            if (string.IsNullOrEmpty(consultationId) ||
                string.IsNullOrEmpty(complaints) || string.IsNullOrEmpty(diagnosis) || string.IsNullOrEmpty(conclusion))
            {
                MessageBox.Show("Будь ласка, заповніть усі обов'язкові поля (прийом, скарги, діагноз, заключення)!", "Помилка!");
                return;
            }


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand updateCmd = new SqlCommand(
                    @"UPDATE Consultation_report
              SET Complaints = @Complaints, Diagnosis = @Diagnosis, Conclusion = @Conclusion
              WHERE ID_consultation = @ID", connection);

                updateCmd.Parameters.AddWithValue("@Complaints", complaints);
                updateCmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                updateCmd.Parameters.AddWithValue("@Conclusion", conclusion);
                updateCmd.Parameters.AddWithValue("@ID", consultationId);
                updateCmd.ExecuteNonQuery();

                foreach (var drug in administeredDrugs)
                {
                    SqlCommand insertCmd = new SqlCommand(
                        @"INSERT INTO Administered_drugs (ID_consultation, ID_drug, Dosage, Quantity_used)
                  VALUES (@ID_consultation, @ID_drug, @Dosage, @Quantity)", connection);

                    insertCmd.Parameters.AddWithValue("@ID_consultation", consultationId);
                    insertCmd.Parameters.AddWithValue("@ID_drug", drug.ID_drug);
                    insertCmd.Parameters.AddWithValue("@Dosage", drug.Dosage);
                    insertCmd.Parameters.AddWithValue("@Quantity", drug.Quantity);
                    insertCmd.ExecuteNonQuery();
                }
            }
            cmbDoctor.SelectedIndex = -1;
            cmbNumberAppointment.SelectedIndex = -1;
            cmbNumberAppointment.ItemsSource = null;
            txtComplaints.Clear();
            txtDiagnosis.Clear();
            txtConclusion.Clear();
            txtMedicinalProduct.SelectedIndex = -1;
            txtDosage.Clear();
            txtQuantity.Clear();
            administeredDrugs.Clear();

            MessageBox.Show("Дані успішно збережено!", "Інформація!");
        }
    }
}
