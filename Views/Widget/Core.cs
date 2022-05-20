using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SkiaSharp;
using taskmaker_wpf.Utilities;


namespace taskmaker_wpf.Views.Widgets {
    public interface IProps {
        SKRect Bound { get; set; }
    }

    public class RenderObject<T> : IRenderObject, IDisposable where T: IProps {
        protected SKPicture _picture;
        protected T _props;
        protected bool disposedValue;

        public SKPicture Picture { get => _picture; set => _picture = value; }

        public RenderObject(T props) {
            _props = props;

            Render();
        }

        public virtual bool HitTest(SKPoint pt) {
            throw new NotImplementedException();
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

    public class RootWidget : RenderWidget {
        public RootWidget(string name) : base(name) { }

        public override bool HitTest(SKPoint point) {
            return true;
        }

        public override void RenderAsync() {
        }
    }

    public interface IWidget {
        IRenderObject RenderObject { get; set; }

        bool HitTest(SKPoint point);
        void RaiseSKEvent(SKEventHandlerArgs args);
        void Paint(SKCanvas canvas);
        void RenderAsync();

        List<T> GetAll<T>();
    }

    public interface IRenderObject {
        SKPicture Picture { get; set; }

        bool HitTest(SKPoint pt);
    }

    public class RenderWidget : TreeElement, IWidget {
        public object DataContext { get; set; }

        protected Type _TRenderObj = null;
        protected Type _TProps = null;
        public IRenderObject RenderObject { get => _renderObject; set => _renderObject = value; }

        private IRenderObject _renderObject;

        public RenderWidget(string name) {
            Name = name;
            _TProps = Type.GetType(GetType().FullName + "Props");
            _TRenderObj = Type.GetType(GetType().FullName + "RenderObject");
        }

        public virtual void Paint(SKCanvas canvas) {
            if (_renderObject != null)
                canvas.DrawPicture(RenderObject.Picture);
        }

        protected virtual IProps GetProps() {
            throw new NotImplementedException();
        }

        public async virtual void RenderAsync() {
            var props = GetProps();

            await Task.Run(() => {
                // Renderer worker
                var instanceType = GetType();

                var ctor = _TRenderObj.GetConstructor(new[] { _TProps });
                dynamic obj = ctor.Invoke(new[] { props });

                // Invoke to UI thread
                Dispatcher.BeginInvoke((Action)(() => {
                    RenderObject = obj;
                }));
            });
        }

        public static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            (d as RenderWidget).RenderAsync();
        }

        public List<TreeElement> GetAllChildren() => GetAllChild();

        public List<T> GetAll<T>() => GetAllChild().Cast<T>().ToList();

        public virtual bool HitTest(SKPoint point) {
            throw new NotImplementedException();
        }

        public void RaiseSKEvent(SKEventHandlerArgs args) {
            var eventName = args.Event.Name;
            var ownerType = args.Event.OwnerType;

            ownerType.GetEvent(eventName).GetRaiseMethod().Invoke(this, new object[] { args });
        }

        public void AddHandler(SKEvent skEvt, Delegate handler) {
            skEvt
        }

        public void RemoveHandler(SKEvent skEvt, Delegate handler) {
        }
    }
}
