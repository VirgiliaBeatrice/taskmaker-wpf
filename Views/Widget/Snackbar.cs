using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace taskmaker_wpf.Views.Widget {
    public class Snackbar : UserControl {
        private string icon = "";
        private string supportingText = "";
        private Label _text;
        private Label _icon;

        public string Icon {
            get => icon;
            set {
                icon = value;

                _icon.Content = icon;
            }
        }
        public string SupportingText { get => supportingText; set {
                supportingText = value;

                _text.Content = supportingText;
            } }

        public Snackbar() {
            var invSurfaceColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5EFF7"));
            var invPrimary = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#322F35"));

            invSurfaceColor.Freeze();
            invPrimary.Freeze();

            Effect = Evelations.Lv3;

            var container = new StackPanel {
                Orientation = Orientation.Horizontal,
                Height = 48
            };
            var shape = new Border {
                CornerRadius = new CornerRadius(4),
                Background = invPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _text = new Label {
                Content = "Supporting text",
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _icon = new Label {
                Content = "\ue7c9",
                Margin = new Thickness(16, 0, 0, 0),
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 24,
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var close = new TextBlock {
                Text = "Hide",
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = invSurfaceColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            close.MouseLeftButtonUp += (_, e) => {
                Visibility = Visibility.Hidden;
            };

            close.MouseEnter += (_, e) => {
                close.TextDecorations = TextDecorations.Underline;
            };

            close.MouseLeave += (_, e) => {
                close.TextDecorations = null;
            };

            Content = shape;
            shape.Child = container;
            container.Children.Add(_icon);
            container.Children.Add(_text);
            container.Children.Add(close);
        }
    }
}