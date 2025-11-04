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
    internal class TextBoxPlaceholderServiceBig
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxPlaceholderServiceBig),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderProperty, value);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded += (s, _) => ShowPlaceholder(textBox);
                textBox.TextChanged += (s, _) => ShowPlaceholder(textBox);
            }
        }

        private static void ShowPlaceholder(TextBox textBox)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBox);
            if (layer == null) return;

            var adorners = layer.GetAdorners(textBox);
            if (!string.IsNullOrEmpty(textBox.Text))
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
                    layer.Add(new PlaceholderAdorner(textBox, GetPlaceholder(textBox)));
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
                var textBox = (TextBox)AdornedElement;
                var text = new FormattedText(
                    _placeholder,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
                    textBox.FontSize,
                    Brushes.Gray,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                double yOffset = 4;
                drawingContext.DrawText(text, new Point(4, yOffset));
            }
        }
    }
}
