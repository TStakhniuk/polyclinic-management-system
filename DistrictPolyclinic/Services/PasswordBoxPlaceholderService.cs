using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace DistrictPolyclinic.Services
{
    public static class PasswordBoxPlaceholderService
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PasswordBoxPlaceholderService),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderProperty, value);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                passwordBox.Loaded += (s, _) => ShowPlaceholder(passwordBox);
                passwordBox.PasswordChanged += (s, _) => ShowPlaceholder(passwordBox);
            }
        }

        private static void ShowPlaceholder(PasswordBox passwordBox)
        {
            var layer = AdornerLayer.GetAdornerLayer(passwordBox);
            if (layer == null) return;

            var adorners = layer.GetAdorners(passwordBox);

            if (!string.IsNullOrEmpty(passwordBox.Password))
            {
                // remove existing
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        if (adorner is PlaceholderAdorner)
                            layer.Remove(adorner);
                    }
                }
            }
            else
            {
                // add if not exists
                if (adorners == null || !adorners.Any(a => a is PlaceholderAdorner))
                {
                    layer.Add(new PlaceholderAdorner(passwordBox, GetPlaceholder(passwordBox)));
                }
            }
        }

        private class PlaceholderAdorner : Adorner
        {
            private readonly string _placeholder;

            public PlaceholderAdorner(UIElement element, string placeholder) : base(element)
            {
                IsHitTestVisible = false;
                _placeholder = placeholder;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                var passwordBox = (PasswordBox)AdornedElement;
                var text = new FormattedText(
                    _placeholder,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(passwordBox.FontFamily, passwordBox.FontStyle, passwordBox.FontWeight, passwordBox.FontStretch),
                    passwordBox.FontSize,
                    Brushes.Gray,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                double yOffset = (passwordBox.ActualHeight - text.Height) / 2;
                if (yOffset < 0) yOffset = 0;

                drawingContext.DrawText(text, new Point(4, yOffset));
            }
        }
    }
}
