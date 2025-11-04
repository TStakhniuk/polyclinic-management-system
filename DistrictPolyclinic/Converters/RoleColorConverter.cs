using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DistrictPolyclinic.Converters
{
    public class RoleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;

            string input = value.ToString();

            // For gender
            if (input == "Чоловік")
                return new SolidColorBrush(Color.FromRgb(84, 90, 255)); // #545AFF
            else if (input == "Жінка")
                return new SolidColorBrush(Color.FromRgb(192, 18, 255)); // #C012FF

            // For roles
            if (input == "Лікар")
                return new SolidColorBrush(Color.FromRgb(84, 90, 255)); // #545AFF
            else if (input == "Працівник реєстратури")
                return new SolidColorBrush(Color.FromRgb(192, 18, 255)); // #C012FF

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}