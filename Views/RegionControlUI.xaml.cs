using SkiaSharp;
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
using System.Windows.Threading;
using taskmaker_wpf.ViewModels;
using Numpy;

namespace taskmaker_wpf.Views {
    /// <summary>
    /// Interaction logic for RegionControlUI.xaml
    /// </summary>
    public partial class RegionControlUI : UserControl {
        private DispatcherTimer _timer;
        private RegionControlUIViewModel _viewModel => DataContext as RegionControlUIViewModel;

        public RegionControlUI() {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += _timer_Tick;

            //_viewModel = DataContext as RegionControlUIViewModel;

            Console.WriteLine(np.pi);

            _timer.Start();
        }


        private void _timer_Tick(object sender, EventArgs e) {
            skElement.InvalidateVisual();
        }

        private void SKElement_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e) {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.AntiqueWhite);

            //if (Engine.RenderQueue.Count != 0) {
            //    do {
            //        var (widget, state) = Engine.RenderQueue.Dequeue();

            //        widget.SetState(state);
            //        widget.Build();
            //    } while(Engine.RenderQueue.Count != 0);
            //}

            Engine.Paint(_viewModel.Page.Root, canvas);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            Console.WriteLine(skElement.Focusable);
            Keyboard.Focus(skElement);
            Console.WriteLine(Keyboard.FocusedElement);
        }

        private void skElement_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            //Console.WriteLine("PDown: " + e.ChangedButton.ToString()) ;
        }

        private void skElement_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            //Console.WriteLine("PUp: " + e.ChangedButton.ToString());

        }

        private void skElement_MouseDown(object sender, MouseButtonEventArgs e) {
            //Console.WriteLine("Down: " + e.ChangedButton.ToString());

        }

        private void skElement_MouseUp(object sender, MouseButtonEventArgs e) {
            //Console.WriteLine("Up: " + e.ChangedButton.ToString());

        }
    }
}
