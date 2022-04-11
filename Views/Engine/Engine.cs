using System;
using System.Collections.Generic;
using System.Linq;
using taskmaker_wpf.Utilities;
using SkiaSharp;
using taskmaker_wpf.Views.Widgets;

namespace taskmaker_wpf.Views {

    public class Engine {
        static private Stack<object> _ctx = new Stack<object>();
        
        static public void DFSUtil(TreeElement curr, Func<TreeElement, object> preAction, Func<TreeElement, object, object> action, Func<TreeElement, object, object> postAction) {
            // Do something
            object ctx = preAction(curr);

            // Recursively do
            foreach (var node in curr.GetAllChild()) {
                // Push context into _ctx stack before going to next level recursively
                _ctx.Push(ctx);
                DFSUtil(node, preAction, action, postAction);
                ctx = _ctx.Pop();

                ctx = action(node, ctx);
            }

            // Do something
            ctx = postAction(curr, ctx);
            //_ctx.Pop();
        }

        static public void Build(IWidget element, bool forceAll = false) {
            if (element.IsDirty | forceAll)
                element.Build();
            else
                return;

            foreach (var child in element.GetAllChild()) {
                Build(child, forceAll);
            }
        }

        static public void _Layout(IWidget widget) {

            if (widget is ContainerWidget) {
                (widget as ContainerWidget).Layout();
            }

            foreach (var child in widget.GetAllChild()) {
                _Layout(child);
            }
        }

        static public void Layout(Widget root) {
            _Layout(root);
        }

        static public void _Paint(IWidget widget, SKCanvas canvas) {
            if (widget is ContainerWidget container0)
                container0.OnPainting(canvas);

            widget.Paint(canvas);

            foreach (var child in widget.GetAllChild()) {
                _Paint(child, canvas);
            }

            if (widget is ContainerWidget container1)
                container1.OnPainted(canvas);
        }

        static public void Paint(Widget root, SKCanvas canvas) {
            _Paint(root, canvas);
        }

        static public Queue<(IStatefulWidget, IWidgetState)> RenderQueue { get; set; } = new Queue<(IStatefulWidget, IWidgetState)>();

        static public void SetState(IStatefulWidget widget, IWidgetState state) {
            RenderQueue.Enqueue((widget, state));
        }
    }
}
