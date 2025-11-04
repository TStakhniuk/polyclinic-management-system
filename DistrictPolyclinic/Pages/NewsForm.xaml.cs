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
using System.Windows.Shapes;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for NewsForm.xaml
    /// </summary>
    public partial class NewsForm : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public NewsForm()
        {
            InitializeComponent();
            dpDate.SelectedDate = DateTime.Today;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate == null || string.IsNullOrWhiteSpace(txtHeader.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля!", "Помилка!");
                return;
            }

            if (dpDate.SelectedDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Неможливо додати оголошення на минулу дату!", "Помилка!");
                return;
            }

            // Ask the user before adding
            var result = MessageBox.Show("Ви дійсно хочете додати це оголошення?", "Підтвердження!", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO NewsNote (NoteDate, Title, Description)
                    VALUES (@NoteDate, @Title, @Description)", conn);

                        cmd.Parameters.AddWithValue("@NoteDate", dpDate.SelectedDate.Value.Date);
                        cmd.Parameters.AddWithValue("@Title", txtHeader.Text.Trim());
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Оголошення додано успішно!", "Інформація!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при збереженні: " + ex.Message, "Помилка!");
                }
            }
            else
            {
                MessageBox.Show("Оголошення не додано!", "Інформація!");
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
