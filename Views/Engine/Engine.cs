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

        static public void Build(IWidget_Wpf element) {
            element.RenderAsync();

            foreach(var child in element.GetAllChildren()) {
                Build(child as IWidget_Wpf);
            }
        }

        static public void Paint(IWidget_Wpf widget, SKCanvas canvas) {
            widget.Paint(canvas);

            foreach (var child in widget.GetAllChildren()) {
                Paint(child as IWidget_Wpf, canvas);
            }
        }
    }
}
