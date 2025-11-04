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
using System.Windows.Shapes;

namespace DistrictPolyclinic.Pages
{
    /// <summary>
    /// Interaction logic for WindowMedicines.xaml
    /// </summary>
    public partial class WindowMedicines : Window
    {
        public WindowMedicines()
        {
            InitializeComponent();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Оберіть період!", "Помилка!");
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

            var reportWindow = new ReportMedicinesUsed(startDate, endDate);
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
