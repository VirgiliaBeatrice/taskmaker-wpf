using System;
using System.Windows.Media;

namespace taskmaker_wpf.Views.Widget {
    public class ColorManager {
        // https://abierre.com/article/5c18da1189599b92b890ecc1
        static public double TintFactor = 0.25;

        static public Color[] Palette = {
            Colors.Red,
            Colors.Pink,
            Colors.Purple,
            Colors.Indigo,
            Colors.Blue,
            Colors.LightBlue,
            Colors.Cyan,
            Colors.Teal,
            Colors.Green,
            Colors.LightGreen,
            Colors.Lime,
            Colors.Yellow,
            Colors.Orange,
            Colors.Brown,
            Colors.Gray,
        };

        static public Color GetTintedColor(Color color, int level) {
            return new Color {
                R = (byte)(color.R + (255 - color.R) * (TintFactor * level)),
                G = (byte)(color.G + (255 - color.G) * (TintFactor * level)),
                B = (byte)(color.B + (255 - color.B) * (TintFactor * level)),
                A = 255
            };
        }

        public static double GetRelativeLuminance(Color color) {
            var r = (double)color.R / 255.0;
            var g = (double)color.G / 255.0;
            var b = (double)color.B / 255.0;

            double R, G, B;

            if (r <= 0.03928)
                R = r / 12.92;
            else
                R = Math.Pow(((r + 0.055) / 1.055), 2.4);

            if (g <= 0.03928)
                G = g / 12.92;
            else
                G = Math.Pow(((g + 0.055) / 1.055), 2.4);

            if (b <= 0.03928)
                B = b / 12.92;
            else
                B = Math.Pow(((b + 0.055) / 1.055), 2.4);

            return 0.2126 * R + 0.7152 * G + 0.0722 * B;
        }

        public static double GetConstrastRatio(Color color0, Color color1) {
            var l0 = GetRelativeLuminance(color0);
            var l1 = GetRelativeLuminance(color1);

            var luminH = l0 > l1 ? l0 : l1;
            var luminL = l0 > l1 ? l1 : l0;

            return (luminH + 0.05) / (luminL + 0.05);
        }

        public static Color GetComplemetaryColor(Color color) {
            return new Color() {
                R = (byte)(255 - color.R),
                G = (byte)(255 - color.G),
                B = (byte)(255 - color.B),
                A = 255
            };
        }
    }
}
