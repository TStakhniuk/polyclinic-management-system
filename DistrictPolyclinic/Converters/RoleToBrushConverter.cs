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
    public class RoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string role = value as string;
            if (role == null)
                return Brushes.Transparent;

            if (role == "Працівник реєстратури")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C012FF"));
            else if (role == "Лікар")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#545AFF"));
            else if (role == "Адміністратор")
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA023"));
            else
                return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
