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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DistrictPolyclinic.Services;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for Appointment.xaml
    /// </summary>

    public class SpecializationItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    public class DoctorItem
    {
        public string ID { get; set; }
        public string FullName { get; set; }
    }


    public partial class Appointment : Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
        private DateTime currentDate;
        private string selectedDoctorId;
        private DateTime currentWeekStartDate;

        public Appointment()
        {
            InitializeComponent();
            currentDate = DateTime.Now; 
            DrawGrid();
            UpdateWeekDates();
            LoadSpecializations();
            currentWeekStartDate = GetStartOfWeek(DateTime.Today);

        }

        private void LoadSpecializations()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID_specialization, Name_specialization FROM Specialization";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    List<SpecializationItem> specializations = new List<SpecializationItem>();
                    while (reader.Read())
                    {
                        specializations.Add(new SpecializationItem
                        {
                            ID = reader["ID_specialization"].ToString(),
                            Name = reader["Name_specialization"].ToString()
                        });
                    }

                    cmbSpecializations.ItemsSource = specializations;
                    cmbSpecializations.DisplayMemberPath = "Name"; 
                    cmbSpecializations.SelectedValuePath = "ID";  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні спеціалізацій: " + ex.Message);
            }
        }

        private void LoadDoctorsBySpecialization(string specializationId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT d.ID_employee, e.Last_name, e.First_name, e.Patronymic
                    FROM Doctor d
                    INNER JOIN Employee e ON d.ID_employee = e.ID_employee
                    WHERE d.ID_specialization = @specializationId
                    AND e.Status_employee = 'Активний'";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@specializationId", specializationId);
                    SqlDataReader reader = command.ExecuteReader();

                    List<DoctorItem> doctors = new List<DoctorItem>();
                    while (reader.Read())
                    {
                        string fullName = $"{reader["Last_name"]} {reader["First_name"]} {reader["Patronymic"]}";
                        doctors.Add(new DoctorItem
                        {
                            ID = reader["ID_employee"].ToString(),
                            FullName = fullName
                        });
                    }

                    cmbDoctors.ItemsSource = doctors;
                    cmbDoctors.DisplayMemberPath = "FullName";
                    cmbDoctors.SelectedValuePath = "ID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні лікарів: " + ex.Message, "Помилка!");
            }
        }

        // Specialization change handler
        private void cmbSpecializations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSpecializations.SelectedValue != null)
            {
                string selectedSpecializationId = cmbSpecializations.SelectedValue.ToString();
                LoadDoctorsBySpecialization(selectedSpecializationId);
            }
            else
            {
                cmbDoctors.ItemsSource = null;
            }
        }

        private void DrawGrid()
        {
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0.1)
                    };

                    // If this is the first column (for time)
                    if (col == 0 && row != 0)
                    {
                        border.BorderThickness = new Thickness(0, 0, 0, 0);
                    }

                    Rectangle rect = new Rectangle
                    {
                        Fill = Brushes.Transparent
                    };

                    //rect.MouseEnter += Rectangle_MouseEnter;
                    //rect.MouseLeave += Rectangle_MouseLeave
                    // border.Child = rect;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    gridContainer.Children.Add(border);
                }
            }
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect != null)
            {
                var parentBorder = rect.Parent as Border;
                int row = Grid.GetRow(parentBorder);
                int col = Grid.GetColumn(parentBorder);

                if (row == 0 || col == 0) return;

                rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0")); // Gray background on hover

            }
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect != null)
            {
                var parentBorder = rect.Parent as Border;
                int row = Grid.GetRow(parentBorder);
                int col = Grid.GetColumn(parentBorder);

                if (row == 0 || col == 0) return;

                rect.Fill = Brushes.Transparent; // Transparent background when relegated
            }
        }

        // DAYS OF THE WEEK
        // Update numbers for days of the week
        private void UpdateWeekDates()
        {
            DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (currentDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            ResetHighlight();

            MondayDate.Text = startOfWeek.Day.ToString();
            TuesdayDate.Text = startOfWeek.AddDays(1).Day.ToString();
            WednesdayDate.Text = startOfWeek.AddDays(2).Day.ToString();
            ThursdayDate.Text = startOfWeek.AddDays(3).Day.ToString();
            FridayDate.Text = startOfWeek.AddDays(4).Day.ToString();
            SaturdayDate.Text = startOfWeek.AddDays(5).Day.ToString();
            SundayDate.Text = startOfWeek.AddDays(6).Day.ToString();

            HighlightToday(startOfWeek);
            UpdateMonthYearText();
        }

        private void HighlightToday(DateTime startOfWeek)
        {
            DateTime today = DateTime.Now;

            if (today >= startOfWeek && today < startOfWeek.AddDays(7))
            {
                switch (today.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        HighlightDay(MondayBorder, MondayDate);
                        HighlightDayText(MondayText);
                        break;
                    case DayOfWeek.Tuesday:
                        HighlightDay(TuesdayBorder, TuesdayDate);
                        HighlightDayText(TuesdayText);
                        break;
                    case DayOfWeek.Wednesday:
                        HighlightDay(WednesdayBorder, WednesdayDate);
                        HighlightDayText(WednesdayText);
                        break;
                    case DayOfWeek.Thursday:
                        HighlightDay(ThursdayBorder, ThursdayDate);
                        HighlightDayText(ThursdayText);
                        break;
                    case DayOfWeek.Friday:
                        HighlightDay(FridayBorder, FridayDate);
                        HighlightDayText(FridayText);
                        break;
                    case DayOfWeek.Saturday:
                        HighlightDay(SaturdayBorder, SaturdayDate);
                        HighlightDayText(SaturdayText);
                        break;
                    case DayOfWeek.Sunday:
                        HighlightDay(SundayBorder, SundayDate);
                        HighlightDayText(SundayText);
                        break;
                }
            }
        }

        private void HighlightDayText(TextBlock dayTextBlock)
        {
            dayTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0c5fff"));
        }

        private void ResetDayTextHighlight(TextBlock dayTextBlock)
        {
            dayTextBlock.Foreground = Brushes.Black;
        }

        private void ResetHighlight()
        {
            // Reset the highlight for all days
            ResetDayHighlight(MondayBorder, MondayDate);
            ResetDayHighlight(TuesdayBorder, TuesdayDate);
            ResetDayHighlight(WednesdayBorder, WednesdayDate);
            ResetDayHighlight(ThursdayBorder, ThursdayDate);
            ResetDayHighlight(FridayBorder, FridayDate);
            ResetDayHighlight(SaturdayBorder, SaturdayDate);
            ResetDayHighlight(SundayBorder, SundayDate);

            // Reset the text highlighting for all days
            ResetDayTextHighlight(MondayText);
            ResetDayTextHighlight(TuesdayText);
            ResetDayTextHighlight(WednesdayText);
            ResetDayTextHighlight(ThursdayText);
            ResetDayTextHighlight(FridayText);
            ResetDayTextHighlight(SaturdayText);
            ResetDayTextHighlight(SundayText);
        }

        private void ResetDayHighlight(Border dayBorder, TextBlock dayTextBlock)
        {
            dayBorder.Background = Brushes.Transparent; 
            dayTextBlock.Foreground = Brushes.Black; 
        }

        private void HighlightDay(Border dayBorder, TextBlock dayTextBlock)
        {
            dayBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0c5fff")); 
            dayTextBlock.Foreground = Brushes.White;
        }


        // BUTTONS ON THE PANEL
        // Back button handler
        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Children.Clear();
            currentDate = currentDate.AddDays(-7);
            currentWeekStartDate = currentWeekStartDate.AddDays(-7);

            if (isCalendarOpen)
            {
                CalendarPopup.IsOpen = false;
                isCalendarOpen = false;

                var arrowPath = GetArrowPathFromButton(MonthYearButton);
                if (arrowPath != null)
                {
                    arrowPath.Data = Geometry.Parse("M7,10L12,15L17,10H7Z");
                }

                MonthYearButton.Background = Brushes.Transparent;
            }

            Storyboard sb = (Storyboard)FindResource("WeekChangeBackwardStoryboard");
            sb.Completed += async (s, ev) =>
            {
                UpdateWeekDates();
                Storyboard resetSb = (Storyboard)FindResource("ResetPositionBackwardStoryboard");
                resetSb.Begin();

                await LoadAndDisplayAppointmentsAsync();
            };
            sb.Begin();
        }


        // Forward button handler
        private async void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Children.Clear();
            currentDate = currentDate.AddDays(7);
            currentWeekStartDate = currentWeekStartDate.AddDays(7);

            if (isCalendarOpen)
            {
                CalendarPopup.IsOpen = false;
                isCalendarOpen = false;

                var arrowPath = GetArrowPathFromButton(MonthYearButton);
                if (arrowPath != null)
                {
                    arrowPath.Data = Geometry.Parse("M7,10L12,15L17,10H7Z");
                }

                MonthYearButton.Background = Brushes.Transparent;
            }

            Storyboard sb = (Storyboard)FindResource("WeekChangeForwardStoryboard");
            sb.Completed += async (s, ev) =>
            {
                UpdateWeekDates();
                Storyboard resetSb = (Storyboard)FindResource("ResetPositionForwardStoryboard");
                resetSb.Begin();

                await LoadAndDisplayAppointmentsAsync();
            };
            sb.Begin();
        }

        // Button background animation
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                button.Background = Brushes.Transparent;
            }
        }

        // CALENDAR
        private bool isCalendarOpen = false;
        private void MonthYearButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
        }

        private void MonthYearButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && !isCalendarOpen)
            {
                button.Background = Brushes.Transparent;
            }
        }

        private void MonthYearButton_Click(object sender, RoutedEventArgs e)
        {
            isCalendarOpen = !isCalendarOpen;
            CalendarPopup.IsOpen = isCalendarOpen;

            var arrowPath = GetArrowPathFromButton(sender as Button);
            if (isCalendarOpen)
            {
                arrowPath.Data = Geometry.Parse("M7,15L12,10L17,15H7Z"); // Down arrow
            }
            else
            {
                arrowPath.Data = Geometry.Parse("M7,10L12,15L17,10H7Z"); // Up arrow
            }

            CalendarControl.DisplayDate = currentDate;
        }

        private Path GetArrowPathFromButton(Button button)
        {
            StackPanel stackPanel = button.Content as StackPanel;
            if (stackPanel != null)
            {
                return stackPanel.Children.OfType<Path>().FirstOrDefault();
            }
            return null;
        }

        private async void CalendarControl_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarControl.SelectedDate.HasValue)
            {
                currentDate = CalendarControl.SelectedDate.Value; // Update the current date
                currentWeekStartDate = GetStartOfWeek(currentDate);
                UpdateWeekDates(); // Update the week
                UpdateMonthYearText(); // Update the button text
                CalendarPopup.IsOpen = false; // Close the Popup
                isCalendarOpen = false; // Update the flag

                MonthYearButton.Background = Brushes.Transparent;
                var arrowPath = GetArrowPathFromButton(MonthYearButton);
                if (arrowPath != null)
                {
                    arrowPath.Data = Geometry.Parse("M7,10L12,15L17,10H7Z"); // Up arrow
                }

                await LoadAndDisplayAppointmentsAsync();
            }
        }

        private void UpdateMonthYearText()
        {
            DateTime startOfWeek = currentDate.AddDays(DayOfWeek.Monday - currentDate.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            string startMonth = startOfWeek.ToString("MMMM");
            string endMonth = endOfWeek.ToString("MMMM");
            string year = currentDate.Year.ToString();

            MonthYearText.Text = (startMonth == endMonth) ? $"{startMonth} {year}" : $"{startMonth}-{endMonth} {year}";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCalendarOpen)
            {
                if (!CalendarPopup.IsMouseOver && !MonthYearButton.IsMouseOver)
                {
                    CalendarPopup.IsOpen = false;
                    isCalendarOpen = false;

                    var arrowPath = GetArrowPathFromButton(MonthYearButton);
                    if (arrowPath != null)
                    {
                        arrowPath.Data = Geometry.Parse("M7,10L12,15L17,10H7Z");
                    }

                    MonthYearButton.Background = Brushes.Transparent;
                }
            }
        }

        // RECORD IN FORM
        private void Cell_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.ClearValue(Button.BackgroundProperty);

                // Role check
                if (SessionData.UserRole == "Адміністратор" || SessionData.UserRole == "Лікар")
                {
                    MessageBox.Show("У вас немає доступу до запису на прийом!", "Доступ заборонено!");
                    return;
                }

                string[] parts = clickedButton.Name.Split('_');
                int row = int.Parse(parts[1]); // 1..9
                int column = int.Parse(parts[2]); // 1..7

                DateTime selectedDate = currentWeekStartDate.AddDays(column - 1);
                TimeSpan startTime = TimeSpan.FromHours(7 + row);

                AppointmentForm appointmentForm = new AppointmentForm(selectedDate, startTime, selectedDoctorId);

                // After saving — update the schedule
                appointmentForm.OnAppointmentSaved = async () =>
                {
                    await LoadAndDisplayAppointmentsAsync();
                };

                appointmentForm.ShowDialog();
            }
        }


        // DISPLAY RECORDS
        public class DoctorAppointment
        {
            public string PatientId { get; set; }
            public string PatientSurname { get; set; }
            public string PatientName { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }
            public string ConsultationId { get; set; }
            public string DoctorId { get; set; }
        }


        private async void cmbDoctors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbDoctors.SelectedItem is DoctorItem selectedDoctor)
                {
                    selectedDoctorId = selectedDoctor.ID;

                    DateTime startOfWeek = GetStartOfWeek(currentDate);
                    DateTime endOfWeek = startOfWeek.AddDays(6);

                    var appointments = await LoadDoctorAppointmentsAsync(selectedDoctorId, startOfWeek, endOfWeek);
                    DisplayAppointments(appointments);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private async Task LoadAndDisplayAppointmentsAsync()
        {
            if (string.IsNullOrEmpty(selectedDoctorId)) return;

            DateTime startOfWeek = GetStartOfWeek(currentDate);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            var appointments = await LoadDoctorAppointmentsAsync(selectedDoctorId, startOfWeek, endOfWeek);
            DisplayAppointments(appointments);
        }


        private async Task<List<DoctorAppointment>> LoadDoctorAppointmentsAsync(string doctorId, DateTime startOfWeek, DateTime endOfWeek)
        {
            var appointments = new List<DoctorAppointment>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT 
                        ar.ID_patient,
                        p.Last_name,
                        p.First_name,
                        ar.Date_time AS StartDateTime,
                        cr.End_date_time AS EndDateTime,
                        cr.ID_consultation
                    FROM Appointment_record ar
                    JOIN Patient p ON ar.ID_patient = p.ID_patient
                    LEFT JOIN Medical_card mc ON mc.ID_patient = ar.ID_patient
                    LEFT JOIN Consultation_report cr 
                        ON cr.ID_medical_card = mc.ID_medical_card
                        AND cr.Start_date_time = ar.Date_time
                        AND cr.ID_employee = ar.ID_employee
                    WHERE ar.ID_employee = @DoctorId
                    AND CAST(ar.Date_time AS DATE) BETWEEN @StartOfWeek AND @EndOfWeek
                    ORDER BY StartDateTime;";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoctorId", doctorId);
                    command.Parameters.AddWithValue("@StartOfWeek", startOfWeek.Date);
                    command.Parameters.AddWithValue("@EndOfWeek", endOfWeek.Date);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            appointments.Add(new DoctorAppointment
                            {
                                PatientId = reader["ID_patient"].ToString(),
                                PatientSurname = reader["Last_name"].ToString(),
                                PatientName = reader["First_name"].ToString(),
                                StartDateTime = (DateTime)reader["StartDateTime"],
                                EndDateTime = (DateTime)reader["EndDateTime"],
                                ConsultationId = reader["ID_consultation"]?.ToString(),
                                DoctorId = doctorId
                            });
                        }
                    }
                }
            }

            return appointments;
        }


        private void DisplayAppointments(List<DoctorAppointment> appointments)
        {
            OverlayGrid.Children.Clear();

            var timeLabel = new TextBlock
            {
                Text = "ㅤㅤㅤ",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(1, 30, 0, 0)
            };
            Grid.SetColumn(timeLabel, 0);
            OverlayGrid.Children.Add(timeLabel);

            foreach (var appointment in appointments)
            {
                int dayColumn = (int)appointment.StartDateTime.DayOfWeek;
                if (dayColumn == 0) dayColumn = 7;
                int gridColumnIndex = dayColumn;

                DateTime start = appointment.StartDateTime;
                DateTime end = appointment.EndDateTime;

                int startHour = start.Hour;
                int endHour = end.Hour;

                int startRow = (startHour - 8) + 1;
                int endRow = (endHour - 8) + 1;

                if (startRow < 1 || startRow > 9) continue;
                if (endRow < 1) endRow = 1;
                if (endRow > 9) endRow = 9;

                int rowSpan = endRow - startRow + 1;

                bool isPast = end < DateTime.Now;

                Brush color = (Brush)new BrushConverter().ConvertFromString(isPast ? "#C012FF" : "#0063F7");

                var container = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(8) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
                };

                var indicator = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = color,
                    BorderThickness = new Thickness(1, 1, 0, 1),
                    Margin = new Thickness(0)
                };
                Grid.SetColumn(indicator, 0);
                container.Children.Add(indicator);

                // Extract the first 3 characters from ID_consultation
                string consultationIdPrefix = appointment.ConsultationId?.Substring(0, 3);

                var button = new Button
                {
                    Content = $"{start:HH:mm} - {end:HH:mm} №{consultationIdPrefix}\n{appointment.PatientSurname} {appointment.PatientName}",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Padding = new Thickness(1),
                    Background = color,
                    BorderBrush = color,
                    BorderThickness = new Thickness(1, 0, 1, 0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontSize = 9.5
                };

                // Scaling text depending on the size of the form
                OverlayGrid.SizeChanged += (s, e) =>
                {
                    double scaleFactor = OverlayGrid.ActualHeight / 600; 
                    button.FontSize = 10 * scaleFactor; 
                };

                // Hover over button — highlight button and bar
                button.MouseEnter += (s, e) =>
                {
                    button.BorderThickness = new Thickness(1, 1, 1, 1);
                    Brush highlight = (Brush)new BrushConverter().ConvertFromString("#3c7fb1");
                    button.BorderBrush = highlight;
                    indicator.BorderBrush = highlight;
                };

                button.MouseLeave += (s, e) =>
                {
                    button.BorderThickness = new Thickness(1, 0, 1, 0);
                    button.BorderBrush = color;
                    indicator.BorderBrush = color;
                };

                Grid.SetColumn(button, 1);
                container.Children.Add(button);

                Grid.SetColumn(container, gridColumnIndex);
                Grid.SetRow(container, startRow);
                Grid.SetRowSpan(container, rowSpan);
                OverlayGrid.Children.Add(container);

                void UpdateButtonPosition()
                {
                    if (OverlayGrid.RowDefinitions.Count <= endRow) return;

                    double totalHeight = 0;
                    for (int i = startRow; i <= endRow && i < OverlayGrid.RowDefinitions.Count; i++)
                        totalHeight += OverlayGrid.RowDefinitions[i].ActualHeight;

                    double offsetY = (start.Minute / 60.0) * OverlayGrid.RowDefinitions[startRow].ActualHeight;
                    double durationMinutes = (end - start).TotalMinutes;
                    double pixelPerMinute = totalHeight / ((endRow - startRow + 1) * 60.0);
                    double height = durationMinutes * pixelPerMinute;

                    button.Height = height;
                    container.Margin = new Thickness(0, offsetY, 0, 0);
                }

                EventHandler layoutHandler = null;
                layoutHandler = (s, e) =>
                {
                    // Check: do all rows really have height > 0
                    bool allRowsReady = true;
                    for (int i = startRow; i <= endRow && i < OverlayGrid.RowDefinitions.Count; i++)
                    {
                        if (OverlayGrid.RowDefinitions[i].ActualHeight <= 0)
                        {
                            allRowsReady = false;
                            break;
                        }
                    }

                    if (allRowsReady)
                    {
                        OverlayGrid.LayoutUpdated -= layoutHandler;
                        UpdateButtonPosition();
                    }
                };

                OverlayGrid.LayoutUpdated += layoutHandler;
                OverlayGrid.SizeChanged += (s, e) => UpdateButtonPosition();


                button.MouseDoubleClick += (sender, e) =>
                {
                    // Role check
                    if (SessionData.UserRole == "Адміністратор" || SessionData.UserRole == "Лікар")
                    {
                        MessageBox.Show("У вас немає доступу до редагування інформації про прийом!", "Доступ заборонено!");
                        return;
                    }

                    if (isPast)
                    {
                        MessageBox.Show("Цей запис вже завершено і не може бути змінений!", "Інформація!");
                        return;
                    }

                    var editForm = new EditAppointmentForm(appointment);
                    editForm.OnAppointmentSaved = async () =>
                    {
                        await LoadAndDisplayAppointmentsAsync();
                    };
                    editForm.ShowDialog();
                };
            }
        }
    }
}
