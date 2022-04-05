using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Numpy;
using System.Reactive;
using System.Reactive.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using taskmaker_wpf.View;
using System.Windows.Threading;

namespace taskmaker_wpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public IObservable<MouseEventArgs> OMouseClick;
        public IObservable<MouseEventArgs> OMouseDrag;
        public IObservable<KeyEventArgs> OKeyDown;
        public IObservable<KeyEventArgs> OKeyUp;
        public IObservable<KeyEventArgs> OKeyPress;
        public IObservable<TouchEventArgs> OTouchDrag;

        private DispatcherTimer _timer;
        private ViewModel.ComplexViewModel _viewModel;

        public MainWindow() {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += _timer_Tick; ;

            Console.WriteLine(np.pi);
            PrepareObservableEvents();

            _viewModel = new ViewModel.ComplexViewModel(this);

            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e) {
            skElement.InvalidateVisual();
        }

        private void SKElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
            var canvas = e.Surface.Canvas;

            canvas.Clear(SKColors.AntiqueWhite);

            if (Engine.RenderQueue.Count != 0) {
                do {
                    var (widget, state) = Engine.RenderQueue.Dequeue();

                    widget.SetState(state);
                    widget.Build();
                } while (Engine.RenderQueue.Count != 0);
            }

            Engine.Paint(_viewModel.Page.Root, canvas);
        }

        private void PrepareObservableEvents() {
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(this, nameof(MouseDown)).Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(this, nameof(MouseMove)).Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(this, nameof(MouseUp)).Select(e => e.EventArgs);
            var keyDown = Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyDown)).Select(e => e.EventArgs);
            var keyUp = Observable.FromEventPattern<KeyEventArgs>(this, nameof(KeyUp)).Select(e => e.EventArgs);

            var touchDown = Observable.FromEventPattern<TouchEventArgs>(this, nameof(TouchDown)).Select(e => e.EventArgs);
            var touchMove = Observable.FromEventPattern<TouchEventArgs>(this, nameof(TouchMove)).Select(e => e.EventArgs);
            var touchUp = Observable.FromEventPattern<TouchEventArgs>(this, nameof(TouchUp)).Select(e => e.EventArgs);

            OMouseClick = mouseDown
                .Take(1)
                .Sample(mouseUp.Take(1))
                .Do(e => Console.WriteLine("mouse click"));

            OKeyDown = keyDown
                .Do(e => Console.WriteLine("key down"));
            OKeyUp = keyUp
                .Do(e => Console.WriteLine("key up"));
            OKeyPress = keyDown
                .Take(1)
                .Sample(keyUp.Take(1))
                .Do(e => Console.WriteLine("key press"));

            OMouseDrag = mouseDown
                .SelectMany(
                    e =>
                       mouseDown
                           .Take(1)
                           .Timeout(
                                TimeSpan.FromMilliseconds(400),
                                Observable.Empty<MouseEventArgs>()))
               .SelectMany(e =>
                    mouseMove
                        .TakeUntil(mouseUp));

            var OTouchDown = touchDown
                .Do(e => Console.WriteLine(e.GetIntermediateTouchPoints(this)));
            var OTouchUp = touchUp
                .Do(e => Console.WriteLine(e.GetIntermediateTouchPoints(this)));

            OTouchDrag = touchDown
                .Take(1)
                .SelectMany(e => touchMove.TakeUntil(touchUp));


            //OKeyDown.Subscribe(e => Console.WriteLine("down"));
        }
    }
}
