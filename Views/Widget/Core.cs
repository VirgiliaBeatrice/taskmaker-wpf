using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using taskmaker_wpf.Utilities;

namespace taskmaker_wpf.Views.Widgets {
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
}
