using System;
using System.Collections.Generic;
using System.Linq;
using taskmaker_wpf.Utilities;
using SkiaSharp;
using taskmaker_wpf.Views.Widgets;

namespace taskmaker_wpf.Views {
    public delegate void SKEventHandler(object sender, SKEventHandlerArgs args);

    public class HitTestResult {
        public List<IWidget> HitWidgets = new List<IWidget>();
    }

    public class SKEventManager {
        static public List<SKEvent> SKEventCollection = new List<SKEvent>();

        static public IWidget WidgetTree;

        static public SKEvent RegisterEvent(string name, SKEventRoutingStrategy strategy, Type handlerType, Type ownerType) {
            var skEvent = new SKEvent() {
                Name = name,
                Strategy = strategy,
                HandlerType = handlerType,
                OwnerType = ownerType
            };

            SKEventCollection.Add(skEvent);

            return skEvent;
        }

        static public void RaiseSKEvent(string name, IEnumerable<IWidget> path) {
            var events = SKEventCollection.Where(e => e.Name == name);

            var directEvts = events.Where(e => e.Strategy == SKEventRoutingStrategy.Direct);
            var tunnelEvts = events.Where(e => e.Strategy == SKEventRoutingStrategy.Tunnel);
            var bubbleEvts = events.Where(e => e.Strategy == SKEventRoutingStrategy.Bubble);

            var args = new SKEventHandlerArgs();

            if (!events.Any()) return;

            // Raise direct event
            var directTarget = path.Last();
            var isContain = directEvts.Where(e => directTarget.GetType().IsAssignableFrom(e.OwnerType)).Any();

            if (isContain) {
                var skEvt = directEvts.Where(e => directTarget.GetType().IsAssignableFrom(e.OwnerType)).First();
                args.Event = skEvt;

                directTarget.RaiseSKEvent(args);

                if (args.Handled) return;
            }


            // Raise tunnel events
            var tunnelTargets = path;

            foreach (var target in tunnelTargets) {
                isContain = tunnelEvts.Where(e => tunnelTargets.GetType().IsAssignableFrom(e.OwnerType)).Any();

                if (isContain) {
                    var skEvt = tunnelEvts.Where(e => directTarget.GetType().IsAssignableFrom(e.OwnerType)).First();
                    args.Event = skEvt;

                    target.RaiseSKEvent(args);
                    
                    if (args.Handled) return;
                }
            }

            // Raise bubble events
            var bubbleTarget = path.Reverse();

            foreach (var target in bubbleTarget) {
                isContain = bubbleEvts.Where(e => IsSubClassOrClassOf(target.GetType(), e.OwnerType)).Any();

                if (isContain) {
                    var skEvt = bubbleEvts.Where(e => IsSubClassOrClassOf(target.GetType(), e.OwnerType)).First();
                    args.Event = skEvt;

                    target.RaiseSKEvent(args);
                    
                    if (args.Handled) return;
                }
            }

            bool IsSubClassOrClassOf(Type derived, Type based) {
                return derived.IsSubclassOf(based) || derived.Equals(based);
            }
        }


        static public HitTestResult HitTest(IWidget widget, SKPoint pt) {
            var result = new HitTestResult();

            var isHit = widget.RenderObject is null ? true : widget.RenderObject.HitTest(pt);

            if (isHit) {
                result.HitWidgets.Add(widget);

                foreach(var child in widget.GetAll<IWidget>()) {
                    result.HitWidgets.AddRange(HitTest(child, pt).HitWidgets);
                }
            }
            return result;
        }
    } 

    public class SKEvent {
        public string Name { get; set; }
        public SKEventRoutingStrategy Strategy { get; set; }
        public Type HandlerType { get; set; }
        public Type OwnerType { get; set; }

        private List<Delegate> handlers = new List<Delegate>();

        public void AddHandler(Delegate handler) { 
            handlers.Add(handler);
        }

        public void RemoveHandler(Delegate handler) {
            handlers.Remove(handler);
        }
        
        public void Invoke(object sender, SKEventHandlerArgs args) {
            handlers.ForEach(e => ((SKEventHandler)e)(sender, args));
        }

    }

    public enum SKEventRoutingStrategy {
        Bubble,
        Tunnel,
        Direct
    }

    public class SKEventHandlerArgs {
        public bool Handled { get; set; }
        public IWidget Source { get; set; }
        public SKEvent Event { get; set; }
    }

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

        static public void Build(IWidget element) {
            element.RenderAsync();

            foreach(var child in element.GetAll<IWidget>()) {
                Build(child);
            }
        }

        static public void Paint(IWidget widget, SKCanvas canvas) {
            widget.Paint(canvas);

            foreach (var child in widget.GetAll<IWidget>()) {
                Paint(child, canvas);
            }
        }
    }
}
