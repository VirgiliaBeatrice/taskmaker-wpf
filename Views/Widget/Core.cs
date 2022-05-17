using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SkiaSharp;
using taskmaker_wpf.Utilities;


namespace taskmaker_wpf.Views.Widgets {
    public interface IProps {
        SKRect Bound { get; set; }
    }

    public class RenderObject_Wpf<T> : IRenderObject, IDisposable where T: IProps {
        protected SKPicture _picture;
        protected T _props;
        protected bool disposedValue;

        public SKPicture Picture { get => _picture; set => _picture = value; }

        public RenderObject_Wpf(T props) {
            _props = props;

            Render();
        }

        protected virtual void OnRender(SKCanvas canvas) => throw new NotImplementedException();

        public virtual void Render() {
            var recorder = new SKPictureRecorder();
            var canvas = recorder.BeginRecording(_props.Bound);

            var shrinkedRect = _props.Bound;
            //shrinkedRect.Inflate(-10.0f, -10.0f);

            OnRender(canvas);

            _picture = recorder.EndRecording();

            canvas.Dispose();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _picture?.Dispose();
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RenderObject_Wpf() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class RenderObject : RenderObject<IWidgetState> {
        public RenderObject(IWidgetState initState) : base(initState) { }
    }

    public class RenderObject<T> : IDisposable where T : IWidgetState {
        public MetaInfo MetaInfo { get; set; } = new MetaInfo();
        public SKPicture Picture => _cachedPicture;

        protected SKPicture _cachedPicture;
        private bool disposedValue;

        public RenderObject(T initState) {
            Render(initState);
        }

        protected virtual void OnRender(SKCanvas canvas, T state) => throw new NotImplementedException();

        public virtual void Render(T state) {
            var recorder = new SKPictureRecorder();
            var canvas = recorder.BeginRecording(state.Bound);

            var shrinkedRect = state.Bound;
            //shrinkedRect.Inflate(-10.0f, -10.0f);

            OnRender(canvas, state);

            _cachedPicture = recorder.EndRecording();

            canvas.Dispose();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _cachedPicture?.Dispose();

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RenderObject() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public struct MetaInfo {
        public string Name { get; set; }
    }

    public interface IWidget {
        int ModelHash { get; set; }
        bool IsDirty { get; set; }

        void Build();
        void Paint(SKCanvas canvas);
        bool Contains(SKPoint point);
        List<IWidget> GetAllChild();

        IWidget FindTarget(SKPoint point);
    }


    public interface IStatefulWidget : IWidget {
        void SetState(IWidgetState state);
    }

    /// <summary>
    /// Widget - code description of UI widget for engine
    /// </summary>
    public class Widget : TreeElement, IWidget {
        public int ModelHash { get; set; }
        public bool IsDirty { get; set; } = true;
        public RenderObject RenderObject { get; set; }

        //public virtual void CreateRenderObject() { }

        public virtual void Build() {
            IsDirty = false;
        }

        public virtual void Paint(SKCanvas canvas) => throw new NotImplementedException();

        public virtual bool Contains(SKPoint p) {
            return false;
        }

        public virtual void OnClick() { }

        public Widget(string name) {
            Name = name;
        }

        public new List<IWidget> GetAllChild() => base.GetAllChild().Cast<IWidget>().ToList();

        public virtual IWidget FindTarget(SKPoint point) {
            throw new NotImplementedException();
        }

        //public virtual void SetState(IWidgetState newState) { }
    }

    public class StatefulWidget<T> : TreeElement, IDisposable, IStatefulWidget where T : IWidgetState {
        private bool disposedValue;

        public T State { get; set; }
        public int ModelHash { get; set; }
        public bool IsDirty { get; set; } = true;
        public RenderObject<T>  RenderObject { get; set; }

        public StatefulWidget(string name) {
            Name = name;
        }

        public virtual void Build() {
            IsDirty = false;
        }

        public virtual bool Contains(SKPoint p) {
            return false;
        }

        public virtual void OnClick() { }

        public new List<IWidget> GetAllChild() => base.GetAllChild().Cast<IWidget>().ToList();

        public virtual void SetState(IWidgetState newState) {
            throw new NotImplementedException();
        }

        public virtual void Paint(SKCanvas canvas) {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    RenderObject?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~StatefulWidget()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public virtual IWidget FindTarget(SKPoint point) {
            throw new NotImplementedException();
        }
    }

    public class RenderWidget<T> : StatefulWidget<T> where T : IWidgetState {
        public RenderWidget(string name) : base(name) { }

        public RenderWidget(string name, T initState) : base(name) {
            State = initState;
        }

        public override void Paint(SKCanvas canvas) {
            canvas.DrawPicture(RenderObject.Picture);
        }
    }

    public class RenderWidget : Widget {
        public RenderWidget(string name) : base(name) { }

        public override void Paint(SKCanvas canvas) {
            canvas.DrawPicture(RenderObject.Picture);
        }
    }

    public struct MarginSize {
        public float Top;
        public float Left;
        public float Right;
        public float Bottom;

        static public MarginSize Zero() {
            return new MarginSize {
                Top = 0,
                Left = 0,
                Right = 0,
                Bottom = 0
            };
        }
    }

    public struct PaddingSize {
        public float Top;
        public float Left;
        public float Right;
        public float Bottom;

        static public PaddingSize Zero() {
            return new PaddingSize {
                Top = 0,
                Left = 0,
                Right = 0,
                Bottom = 0
            };
        }
    }

    public interface IContainerWidget {
        void Layout();
        void OnPainting(SKCanvas canvas);
        void OnPainted(SKCanvas canvas);
    }

    public abstract class ContainerWidget : Widget, IContainerWidget {
        // Constraints
        public MarginSize Margin { get; set; }
        public PaddingSize Padding { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }
        public SKMatrix T => _transform;

        protected SKMatrix _transform = SKMatrix.Identity;

        public ContainerWidget(string name) : base(name) { }
        //public abstract void SetTransform(float x, float y);

        public virtual void Layout() { }
        public virtual void OnPainting(SKCanvas canvas) { }
        public virtual void OnPainted(SKCanvas canvas) { }
    }

    public class RootContainerWidget : ContainerWidget {
        public RootContainerWidget(string name) : base(name) { }

        public override void Paint(SKCanvas canvas) { }

        //public override bool Contains(SKPoint p) {
        //    return true;
        //}
    }

    public class RootWidget_Wpf : TreeElement, IWidget_Wpf {
        public RootWidget_Wpf(string name) {
            Name = name;
        }

        public IRenderObject RenderObject { get; set; }

        public List<TreeElement> GetAllChildren() => GetAllChild();

        public void Paint(SKCanvas canvas) { }

        public void RenderAsync() { }
    }

    public interface IWidget_Wpf {
        IRenderObject RenderObject { get; set; }

        void Paint(SKCanvas canvas);
        void RenderAsync();

        List<TreeElement> GetAllChildren();
    }

    public interface IRenderObject {
        SKPicture Picture { get; set; }
    }

    public class RenderWidget_Wpf : TreeElement, IWidget_Wpf {
        public IRenderObject RenderObject { get => _renderObject; set => _renderObject = value; }

        private IRenderObject _renderObject;

        protected virtual RenderObject_Wpf<IProps> Build() {
            throw new NotImplementedException();
        }

        public virtual void Paint(SKCanvas canvas) {
            if (_renderObject != null)
                canvas.DrawPicture(RenderObject.Picture);
        }

        public virtual void RenderAsync() {
            throw new NotImplementedException();
        }

        public List<TreeElement> GetAllChildren() => GetAllChild();
    }

    public class DebugInfoWidgetProps : IProps {
        public SKRect Bound { get; set; } = new SKRect { Right = 100, Bottom = 40 };
        public string Message { get; set; }
    }

    public class DebugInfoRenderObject_Wpf : RenderObject_Wpf<DebugInfoWidgetProps> {
        public DebugInfoRenderObject_Wpf(DebugInfoWidgetProps props) : base(props) {
        }

        protected override void OnRender(SKCanvas canvas) {
            var paint = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                Color = SKColors.Black,
                TextSize = 14,
            };

            canvas.DrawText(_props.Message, new SKPoint { X = 0, Y = 20 }, paint);

            paint.Dispose();
        }
    }

    public class DebugInfoWidget : RenderWidget_Wpf {
        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(DebugInfoWidget),
                new PropertyMetadata("Debug", OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as DebugInfoWidget).RenderAsync();
        }

        private DebugInfoWidgetProps GetProps() {
            return new DebugInfoWidgetProps { Message = Message };
        }

        public async override void RenderAsync() {
            var props = GetProps();

            await Task.Run(() => {
                // Renderer worker
                var obj = new DebugInfoRenderObject_Wpf(props);

                // Invoke to UI thread
                Dispatcher.BeginInvoke((Action)(() => {
                    RenderObject = obj;
                }));
            });
        }

        protected override RenderObject_Wpf<IProps> Build() {
            var renderObj = new RenderObject_Wpf<IProps>(GetProps());

            return renderObj;
        }



        public DebugInfoWidget(string name) { }

    }
}
