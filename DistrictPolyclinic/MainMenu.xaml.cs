using System;
using System.Collections.Generic;
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
using DistrictPolyclinic.Themes;
using DistrictPolyclinic.Pages;
using DistrictPolyclinic.Services;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using DistrictPolyclinic.Chat;
using System.Windows.Controls.Primitives;
using System.Configuration;

namespace DistrictPolyclinic
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    /// 
    public partial class MainMenu : Window
    {
        private DateTime _lastClickTime = DateTime.MinValue;
        private Point _dragStartPoint;
        private ResizeDirection _resizeDirection;
        private enum ResizeDirection
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private string userRole;
        private AppointmentReminderService reminderService;

        public MainMenu(string role)
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            // Load the saved theme
            string savedTheme = Properties.Settings.Default.UserTheme;

            if (savedTheme == "Light")
            {
                Themes.IsChecked = true;
                ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
            }
            else
            {
                Themes.IsChecked = false;
                ThemesController.SetTheme(ThemesController.ThemeTypes.Dark);
            }

            frameContent.Navigate(new News());
            msNews.IsChecked = true;
            SessionManager.ClearSession();

            // User
            userRole = role;
            SessionData.UserRole = userRole;
            ConfigureByRole();

            // Email
            string connectionString = ConfigurationManager.ConnectionStrings["DistrictPolyclinic.Properties.Settings.DistrictPolyclinicConnectionString"].ConnectionString;
            reminderService = new AppointmentReminderService(connectionString);
            reminderService.Start();

            // Session
            this.DataContext = new CurrentUserInfo
            {
                FullName = SessionData.FullName,
                Role = SessionData.UserRole
            };
        }

        public class CurrentUserInfo
        {
            public string FullName { get; set; }
            public string Role { get; set; }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            reminderService.Stop();
        }

        // DISTRIBUTION OF RIGHTS
        private void ConfigureByRole()
        {
            if (userRole == "Адміністратор")
            {
                // Employees
                EnableMenu(msEmployees, true);
                EnableMenu(employeesSubmenu, true);
                EnableMenu(AddEmployee, true);
                EnableMenu(EditEmployee, true);
                EnableMenu(ListEmployee, true);

                // Patients
                EnableMenu(msPatients, true);
                EnableMenu(patientsSubmenu, true);
                EnableMenu(AddPatient, false);
                EnableMenu(EditPatient, false);
                EnableMenu(ListPatient, false);

                // Appointments
                EnableMenu(msAppointments, true);

                // Consultations
                EnableMenu(msConsultations, true);
                EnableMenu(consultationsSubmenu, true);
                EnableMenu(ResultAppointment, false);
                EnableMenu(ShowCard, false);

                // Reports
                EnableMenu(msReports, true);
                EnableMenu(reportsSubmenu, true);
                EnableMenu(MedicalCard, true);
                EnableMenu(AppointmentPeriod, true);
                EnableMenu(MedicinesUsed, true);

                // SINGLE MENUS
                EnableMenu(msNews, true);
                EnableMenu(msAssistant, true);

            }
            else if (userRole == "Працівник реєстратури")
            {
                // Employees
                EnableMenu(msEmployees, true);
                EnableMenu(employeesSubmenu, true);
                EnableMenu(AddEmployee, false);
                EnableMenu(EditEmployee, false);
                EnableMenu(ListEmployee, true);

                // Patients
                EnableMenu(msPatients, true);
                EnableMenu(patientsSubmenu, true);
                EnableMenu(AddPatient, true);
                EnableMenu(EditPatient, true);
                EnableMenu(ListPatient, true);

                // Appointments
                EnableMenu(msAppointments, true);

                // Consultations
                EnableMenu(msConsultations, true);
                EnableMenu(consultationsSubmenu, true);
                EnableMenu(ResultAppointment, false);
                EnableMenu(ShowCard, true);

                // Reports
                EnableMenu(msReports, true);
                EnableMenu(reportsSubmenu, true);
                EnableMenu(MedicalCard, false);
                EnableMenu(AppointmentPeriod, false);
                EnableMenu(MedicinesUsed, false);

                // SINGLE MENUS
                EnableMenu(msNews, true);
                EnableMenu(msAssistant, true);
            }
            else if (userRole == "Лікар")
            {
                // Employees
                EnableMenu(msEmployees, true);
                EnableMenu(employeesSubmenu, true);
                EnableMenu(AddEmployee, false);
                EnableMenu(EditEmployee, false);
                EnableMenu(ListEmployee, true);

                // Patients
                EnableMenu(msPatients, true);
                EnableMenu(patientsSubmenu, true);
                EnableMenu(AddPatient, false);
                EnableMenu(EditPatient, true);
                EnableMenu(ListPatient, true);

                // Appointments
                EnableMenu(msAppointments, true);

                // Consultations
                EnableMenu(msConsultations, true);
                EnableMenu(consultationsSubmenu, true);
                EnableMenu(ResultAppointment, true);
                EnableMenu(ShowCard, true);

                // Reports
                EnableMenu(msReports, true);
                EnableMenu(reportsSubmenu, true);
                EnableMenu(MedicalCard, true);
                EnableMenu(AppointmentPeriod, true);
                EnableMenu(MedicinesUsed, true);

                // SINGLE MENUS
                EnableMenu(msNews, true);
                EnableMenu(msAssistant, true);
            }
        }

        private void EnableMenu(UIElement menuElement, bool isEnabled)
        {
            menuElement.IsEnabled = isEnabled;
            menuElement.Opacity = isEnabled ? 1.0 : 0.5;  // Grayscale for inactive
        }

        // PASSWORD AND LOGIN
        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UserPopup.IsOpen = !UserPopup.IsOpen;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            UserPopup.IsOpen = false;
            ChangePasswordForm form = new ChangePasswordForm();
            form.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserPopup.IsOpen = false;
            Close();
        }

        private CustomPopupPlacement[] UserPopup_CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            return new CustomPopupPlacement[]
            {
        new CustomPopupPlacement(new Point(targetSize.Width + 5, - popupSize.Height + 35), PopupPrimaryAxis.None)
            };
        }

        // THEME AND BUTTONS
        private void Themes_Click(object sender, RoutedEventArgs e)
        {
            if (Themes.IsChecked == true)
            {
                ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
                Properties.Settings.Default.UserTheme = "Light";
            }
            else
            {
                ThemesController.SetTheme(ThemesController.ThemeTypes.Dark);
                Properties.Settings.Default.UserTheme = "Dark";
            }

            Properties.Settings.Default.Save();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        // MOVE AND DOUBLE CLICK
        private void HeaderGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 500)
                {
                    ToggleWindowState();
                }
                else
                {
                    _lastClickTime = DateTime.Now;
                    // If not double click, then allow moving the form
                    this.DragMove();
                }
            }
        }

        private void ToggleWindowState()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                WindowBorder.CornerRadius = new CornerRadius(0);
                WindowBorderLeft.CornerRadius = new CornerRadius(0,0,0,0);
            }
            else
            {
                WindowBorder.CornerRadius = new CornerRadius(10);
                WindowBorderLeft.CornerRadius = new CornerRadius(10,0,0,10);

            }
        }


        // SCALING
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (pos.X >= this.ActualWidth - 5)
                {
                    if (pos.Y <= 5)
                        _resizeDirection = ResizeDirection.TopRight; // Top right corner
                    else if (pos.Y >= this.ActualHeight - 5)
                        _resizeDirection = ResizeDirection.BottomRight; // Bottom right corner
                    else
                        _resizeDirection = ResizeDirection.Right; // Right edge
                }
                else if (pos.Y >= this.ActualHeight - 5)
                    _resizeDirection = ResizeDirection.Bottom; // Bottom edge

                _dragStartPoint = pos;
                this.CaptureMouse();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            // Change the cursor appearance
            if (_resizeDirection == ResizeDirection.None)
            {
                if (pos.X >= this.ActualWidth - 5)
                {
                    if (pos.Y <= 5)
                        this.Cursor = Cursors.SizeNESW;
                    else if (pos.Y >= this.ActualHeight - 5)
                        this.Cursor = Cursors.SizeNWSE;
                    else
                        this.Cursor = Cursors.SizeWE;
                }
                else if (pos.Y >= this.ActualHeight - 5)
                    this.Cursor = Cursors.SizeNS;
                else
                    this.Cursor = Cursors.Arrow;
            }

            // If the mouse is captured, resize the window
            if (_resizeDirection != ResizeDirection.None && e.LeftButton == MouseButtonState.Pressed)
            {
                var delta = e.GetPosition(this) - _dragStartPoint;

                switch (_resizeDirection)
                {
                    case ResizeDirection.Right:
                        this.Width += delta.X;
                        break;
                    case ResizeDirection.Bottom:
                        this.Height += delta.Y;
                        break;
                    case ResizeDirection.TopRight:
                        this.Width += delta.X;
                        this.Height -= delta.Y;
                        break;
                    case ResizeDirection.BottomRight:
                        this.Width += delta.X;
                        this.Height += delta.Y;
                        break;
                }

                _dragStartPoint = e.GetPosition(this);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            _resizeDirection = ResizeDirection.None;
        }


        // ANIMATION
        private bool isPatientsSubmenuOpen = false;
        private bool isEmployeesSubmenuOpen = false;
        private bool isConsultationsSubmenuOpen = false;
        private bool isReportsSubmenuOpen = false;

        private void AnimateHeight(Border container, double toHeight)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(300));
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                To = toHeight,
                Duration = duration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            container.BeginAnimation(HeightProperty, heightAnimation);
        }

        private void CloseAllSubmenus()
        {
            if (isPatientsSubmenuOpen)
            {
                AnimateHeight(patientsSubmenuContainer, 0);
                isPatientsSubmenuOpen = false;
            }

            if (isEmployeesSubmenuOpen)
            {
                AnimateHeight(employeesSubmenuContainer, 0);
                isEmployeesSubmenuOpen = false;
            }

            if (isConsultationsSubmenuOpen)
            {
                AnimateHeight(consultationsSubmenuContainer, 0);
                isConsultationsSubmenuOpen = false;
            }

            if (isReportsSubmenuOpen)
            {
                AnimateHeight(reportsSubmenuContainer, 0);
                isReportsSubmenuOpen = false;
            }
        }


        // NAVIGATE THE MENU
        private RadioButton _lastSelectedRadioButton = null;

        private void SubButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton currentButton)
            {
                if (_lastSelectedRadioButton != null && _lastSelectedRadioButton != currentButton)
                {
                    _lastSelectedRadioButton.IsChecked = false;
                }

                _lastSelectedRadioButton = currentButton;
            }
        }

        private void msPatients_Click(object sender, RoutedEventArgs e)
        {
            if (isPatientsSubmenuOpen)
            {
                AnimateHeight(patientsSubmenuContainer, 0);
                isPatientsSubmenuOpen = false;
            }
            else
            {
                CloseAllSubmenus();
                AnimateHeight(patientsSubmenuContainer, patientsSubmenu.ActualHeight);
                isPatientsSubmenuOpen = true;
            }
        }

        private void msEmployees_Click(object sender, RoutedEventArgs e)
        {
            if (isEmployeesSubmenuOpen)
            {
                AnimateHeight(employeesSubmenuContainer, 0);
                isEmployeesSubmenuOpen = false;
            }
            else
            {
                CloseAllSubmenus();
                AnimateHeight(employeesSubmenuContainer, patientsSubmenu.ActualHeight);
                isEmployeesSubmenuOpen = true;
            }
        }

        private void msAppointments_Click(object sender, RoutedEventArgs e)
        {
            ResetSelectedSubButton();
            CollapseAllSubmenus();
            frameContent.Navigate(new Appointment());
        }

        private void msConsultations_Click(object sender, RoutedEventArgs e)
        {
            if (isConsultationsSubmenuOpen)
            {
                AnimateHeight(consultationsSubmenuContainer, 0);
                isConsultationsSubmenuOpen = false;
            }
            else
            {
                CloseAllSubmenus();
                AnimateHeight(consultationsSubmenuContainer, consultationsSubmenu.ActualHeight);
                isConsultationsSubmenuOpen = true;
            }
        }

        private void msReports_Click(object sender, RoutedEventArgs e)
        {
            if (isReportsSubmenuOpen)
            {
                AnimateHeight(reportsSubmenuContainer, 0);
                isReportsSubmenuOpen = false;
            }
            else
            {
                CloseAllSubmenus();
                AnimateHeight(reportsSubmenuContainer, reportsSubmenu.ActualHeight);
                isReportsSubmenuOpen = true;
            }
        }

        // SINGLE FORMS
        public void ResetSelectedSubButton()
        {
            if (_lastSelectedRadioButton != null)
            {
                _lastSelectedRadioButton.IsChecked = false;
                _lastSelectedRadioButton = null;
            }
        }

        public void CollapseAllSubmenus()
        {
            if (isPatientsSubmenuOpen)
            {
                AnimateHeight(patientsSubmenuContainer, 0);
                isPatientsSubmenuOpen = false;
            }

            if (isEmployeesSubmenuOpen)
            {
                AnimateHeight(employeesSubmenuContainer, 0);
                isEmployeesSubmenuOpen = false;
            }

            if (isConsultationsSubmenuOpen)
            {
                AnimateHeight(consultationsSubmenuContainer, 0);
                isConsultationsSubmenuOpen = false;
            }

            if (isReportsSubmenuOpen)
            {
                AnimateHeight(reportsSubmenuContainer, 0);
                isReportsSubmenuOpen = false;
            }
        }

        private void msNews_Click(object sender, RoutedEventArgs e)
        {
            CloseAllSubmenus();
            ResetSelectedSubButton();
            frameContent.Navigate(new News());
        }

        private void msAssistant_Click(object sender, RoutedEventArgs e)
        {
            CloseAllSubmenus();
            ResetSelectedSubButton();
            frameContent.Navigate(new Assistant());
        }

        // TRANSITIONS
        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new AddEmployee());
        }
        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new EditEmployee());
        }

        private void EmployeeList_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new EmployeeList());
        }

        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e); 
            frameContent.Navigate(new AddPatient()); 
        }

        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new EditPatient());
        }
        private void PatientList_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new PatientList());
        }

        private void ResultAppointment_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new ResultAppointment());
        }

        private void MedicalCard_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            frameContent.Navigate(new MedicalCard());
        }

        private void ReportMedicalCard_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            WindowPatient form = new WindowPatient();
            form.ShowDialog();
        }

        private void ReportAppointmentPeriod_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            WindowAppointment form = new WindowAppointment();
            form.ShowDialog();
        }

        private void ReportMedicinesUsed_Click(object sender, RoutedEventArgs e)
        {
            SubButton_Click(sender, e);
            WindowMedicines form = new WindowMedicines();
            form.ShowDialog();
        }

    }
}
