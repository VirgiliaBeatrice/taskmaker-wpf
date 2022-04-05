using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using taskmaker_wpf.Utilities;
using taskmaker_wpf.View;

namespace taskmaker_wpf.View.Widgets.Units {
    //public class TestWidget : RenderWidget {
    //    public TestWidgetState State { get; set; }
    //    protected TestWidgetInternalState _internalState;
    //    protected bool _isClicked = false; 

    //    public TestWidget(string name, TestWidgetState initState) : base(name) {
    //        State = initState;

    //        _internalState = TestWidgetInternalState.Default;
    //    }

    //    public override bool Contains(SKPoint p) {
    //        return State.bound.Contains(p);
    //    }

    //    public override void OnClick() {
    //        _isClicked = !_isClicked;
    //        (RenderObject as TestRenderObject).Render(State, _isClicked);
    //        Console.WriteLine($"{this} has been clicked.");
    //    }

    //    public override void Build() {
    //        RenderObject = new TestRenderObject(State);

    //        base.Build();
    //    }
    //}

    //public class TestRenderObject : RenderObject {
    //    public TestRenderObject(TestWidgetState initState) {
    //        Render(initState);
    //    }

    //    public void Render(TestWidgetState state = new TestWidgetState(), bool arg=false) {
    //        var recorder = new SKPictureRecorder();
    //        var canvas = recorder.BeginRecording(state.bound);

    //        var shrinkedRect = state.bound;
    //        //shrinkedRect.Inflate(-10.0f, -10.0f);

    //        var roundSize = new SKSize(5, 5);
    //        var stroke = new SKPaint {
    //            IsAntialias = true,
    //            StrokeWidth = 4,
    //            IsStroke = true,
    //            Color = SKColors.Black,
    //        };
    //        var fill = new SKPaint {
    //            IsAntialias = true,
    //            Color = SKColors.YellowGreen
    //        };

    //        if (arg) {
    //            fill.Color = SKColors.Beige;
    //        }

    //        canvas.DrawRoundRect(shrinkedRect, roundSize, stroke);
    //        canvas.DrawRoundRect(shrinkedRect, roundSize, fill);

    //        _cachedPicture = recorder.EndRecording();

    //        stroke.Dispose();
    //        fill.Dispose();
    //        canvas.Dispose();
    //    }
    //}
    public struct UnitWidgetState : IWidgetState {
        public SKRect Bound { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class UnitWidget : RenderWidget<UnitWidgetState> {
        public UnitWidget(string name) : base(name) { }

        //public override void CreateRenderObject() {
        //    RenderObject = new UnitRenderObject(null);
        //}
    }

    public class UnitRenderObject : RenderObject {
        public SKRect Bound { get; set; }

        public UnitRenderObject(UnitWidgetState initState) : base(initState) {
            Bound = new SKRect {
                Size = new SKSize {
                    Width = 100,
                    Height = 24
                }
            };

            // Initialize _cachedPicure
            Render();
        }

        public void Render() {
            var recorder = new SKPictureRecorder();
            var canvas = recorder.BeginRecording(Bound);
            var roundSize = new SKSize(5, 5);
            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                Color = SKColors.Black
            };
            var fill = new SKPaint {
                IsAntialias = true,
                Color = SKColors.Aquamarine
            };

            canvas.DrawRoundRect(Bound, roundSize, stroke);
            canvas.DrawRoundRect(Bound, roundSize, fill);

            _cachedPicture = recorder.EndRecording();

            stroke.Dispose();
            fill.Dispose();
            canvas.Dispose();
        }
    }
}
