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
    public static class ComboBoxPlaceholderService
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(ComboBoxPlaceholderService),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderProperty, value);

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                comboBox.Loaded += (s, _) => ShowPlaceholder(comboBox);
                comboBox.SelectionChanged += (s, _) => ShowPlaceholder(comboBox);
                comboBox.GotFocus += (s, _) => ShowPlaceholder(comboBox);
                comboBox.LostFocus += (s, _) => ShowPlaceholder(comboBox);

                // Add a handler for text input
                comboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                                    new TextChangedEventHandler((s, _) => ShowPlaceholder(comboBox)));
            }
        }

        private static void ShowPlaceholder(ComboBox comboBox)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(comboBox);
            if (layer == null) return;

            var adorners = layer.GetAdorners(comboBox);

            bool hasText = false;

            if (comboBox.IsEditable)
            {
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                hasText = textBox != null && !string.IsNullOrEmpty(textBox.Text);
            }
            else
            {
                hasText = comboBox.SelectedItem != null;
            }

            if (hasText)
            {
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
                if (adorners == null || !adorners.Any(a => a is PlaceholderAdorner))
                {
                    layer.Add(new PlaceholderAdorner(comboBox, GetPlaceholder(comboBox)));
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
                var comboBox = (ComboBox)AdornedElement;
                var text = new FormattedText(
                    _placeholder,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(comboBox.FontFamily, comboBox.FontStyle, comboBox.FontWeight, comboBox.FontStretch),
                    comboBox.FontSize,
                    Brushes.Gray,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // Center vertically
                double yOffset = (comboBox.ActualHeight - text.Height) / 2;
                if (yOffset < 0) yOffset = 0;

                drawingContext.DrawText(text, new Point(4, yOffset));
            }
        }
    }
}
