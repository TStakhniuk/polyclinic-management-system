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
using DistrictPolyclinic.Services;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for News.xaml
    /// </summary>
    public partial class News : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;

        public News()
        {
            InitializeComponent();
            string todayDate = DateTime.Now.ToString("dd.MM.yyyy");
            Block2Date.Text = todayDate;
            Block3Date.Text = todayDate;

            LoadDashboardData();
            LoadAndDisplayNews();

            this.Loaded += News_Loaded;
        }

        private void News_Loaded(object sender, RoutedEventArgs e)
        {
            if (SessionData.UserRole == "Адміністратор")
            {
                NotificationIconPath.Data = Geometry.Parse("M17 14V17H14V19H17V22H19V19H22V17H19V14M12 2A2 2 0 0 0 10 4A2 2 0 0 0 10 4.29C7.12 5.14 5 7.82 5 11V17L3 19V20H12.35A6 6 0 0 1 12 18A6 6 0 0 1 18 12A6 6 0 0 1 19 12.09V11C19 7.82 16.88 5.14 14 4.29A2 2 0 0 0 14 4A2 2 0 0 0 12 2M10 21A2 2 0 0 0 12 23A2 2 0 0 0 13.65 22.13A6 6 0 0 1 12.81 21Z");
                NotificationIconPath.ToolTip = "Додати оголошення";
            }
            else
            {
                NotificationIconPath.Data = Geometry.Parse("M21,19V20H3V19L5,17V11C5,7.9 7.03,5.17 10,4.29C10,4.19 10,4.1 10,4A2,2 0 0,1 12,2A2,2 0 0,1 14,4C14,4.1 14,4.19 14,4.29C16.97,5.17 19,7.9 19,11V17L21,19M14,21A2,2 0 0,1 12,23A2,2 0 0,1 10,21");
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Expected receptions (not completed) for today
                    SqlCommand cmd3 = new SqlCommand(@"
                        SELECT COUNT(*) FROM Appointment_record ar
                        WHERE CAST(ar.Date_time AS DATE) = CAST(GETDATE() AS DATE)
                        AND (
                            ar.Date_time > GETDATE() OR
                            NOT EXISTS (
                                SELECT 1 FROM Consultation_report cr
                                WHERE cr.ID_employee = ar.ID_employee
                                AND CAST(cr.Start_date_time AS DATE) = CAST(GETDATE() AS DATE)
                                AND cr.Start_date_time = ar.Date_time
                                AND cr.End_date_time <= GETDATE()
                            )
                        )", connection);
                    int upcomingAppointments = (int)cmd3.ExecuteScalar();
                    UpcomingAppointmentsText.Text = upcomingAppointments.ToString();


                    // 2. Completed receptions for today
                    SqlCommand cmd4 = new SqlCommand(
                        @"SELECT COUNT(*) FROM Consultation_report
                  WHERE CAST(Start_date_time AS DATE) = CAST(GETDATE() AS DATE)
                  AND End_date_time <= GETDATE()", connection);
                    int finishedAppointments = (int)cmd4.ExecuteScalar();
                    FinishedAppointmentsText.Text = finishedAppointments.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні даних для панелі: " + ex.Message, "Помилка!");
            }
        }

        private void LoadAndDisplayNews()
        {
            try
            {
                StringBuilder newsContent = new StringBuilder();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Delete outdated news
                    string deleteQuery = "DELETE FROM NewsNote WHERE NoteDate < CAST(GETDATE() AS DATE)";
                    SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                    deleteCmd.ExecuteNonQuery();

                    // Loading all news
                    string selectQuery = "SELECT NoteDate, Title, Description FROM NewsNote ORDER BY NoteDate";
                    SqlCommand cmd = new SqlCommand(selectQuery, conn);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasNews = false;

                        while (reader.Read())
                        {
                            hasNews = true;

                            DateTime date = reader.GetDateTime(0);
                            string title = reader.GetString(1);
                            string description = reader.IsDBNull(2) ? "" : reader.GetString(2);

                            string formattedTitle = $"{date:dd.MM} - {title}";
                            newsContent.AppendLine(formattedTitle);
                            newsContent.AppendLine(description);
                        }

                        if (!hasNews)
                        {
                            newsContent.AppendLine("Оголошень на даний час немає!");
                        }
                    }
                }

                NewsBox.Inlines.Clear();
                string[] lines = newsContent.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // If the message about no news
                    if (line == "Оголошень на даний час немає!")
                    {
                        NewsBox.Inlines.Add(new Run(line) { FontSize = 23 });
                        break;
                    }

                    var bold = new Bold(new Run(line)) { FontSize = 25 };
                    NewsBox.Inlines.Add(bold);
                    NewsBox.Inlines.Add(new LineBreak());

                    if (i + 1 < lines.Length)
                    {
                        string description = lines[i + 1].Trim();
                        NewsBox.Inlines.Add(new Run(description) { FontSize = 23 });

                        if (i + 2 < lines.Length)
                        {
                            NewsBox.Inlines.Add(new LineBreak());
                            NewsBox.Inlines.Add(new LineBreak());
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні новин: " + ex.Message, "Помилка!");
            }
        }




        private void BellIcon_Click(object sender, RoutedEventArgs e)
        {
            if (SessionData.UserRole == "Адміністратор")
            {
                NewsForm form = new NewsForm();
                form.ShowDialog();
                LoadAndDisplayNews();
            }
        }

    }
}
