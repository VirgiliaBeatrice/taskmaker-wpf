using SkiaSharp;
using System;

namespace taskmaker_wpf.View.Widgets {
    public interface IWidgetState {
        SKRect Bound { get; set; }
    }

    public struct NodeState : IWidgetState, ICloneable {
        public SKRect Bound { get; set; }
        public SKPoint location;
        public float radius;

        public bool isClicked;

        public object Clone() {
            return new NodeState {
                Bound = Bound,
                location = location,
                radius = radius,
                isClicked = isClicked
            };
        }

        public static NodeState Default =>
            new NodeState {
                Bound = new SKRect(0, 0, 10, 10),
                location = new SKPoint(0, 0),
                isClicked = false,
                radius = 10
            };
    }

    public class NodeWidget : RenderWidget<NodeState> {
        public NodeWidget(string name, NodeState initState) : base(name, initState) { }

        public override bool Contains(SKPoint p) {
            return (p - State.location).LengthSquared <= Math.Pow(State.radius, 2);
        }

        public override void OnClick() {
            var state = State;
            state.isClicked = !state.isClicked;
            State = state;
            (RenderObject as NodeRenderObject).Render(State);
        }

        public override void Build() {
            RenderObject = new NodeRenderObject(State);

            base.Build();
        }
    }

    public class NodeRenderObject : RenderObject<NodeState> {
        public NodeRenderObject(NodeState initState) : base(initState) { }

        protected override void OnRender(SKCanvas canvas, NodeState state) {
            var stroke = new SKPaint {
                IsAntialias = true,
                StrokeWidth = 2,
                IsStroke = true,
                Color = SKColors.Black,
            };
            var fill = new SKPaint {
                IsAntialias = true,
                Color = SKColors.YellowGreen
            };

            if (state.isClicked)
                fill.Color = SKColors.AliceBlue;

            canvas.DrawCircle(state.location, state.radius, stroke);
            canvas.DrawCircle(state.location, state.radius, fill);

            stroke.Dispose();
            fill.Dispose();
        }
    }
}
