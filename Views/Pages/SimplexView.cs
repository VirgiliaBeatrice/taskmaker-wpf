using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Views.Widgets;
using taskmaker_wpf.Views.Widgets.Container;
using SkiaSharp;
using System.Windows;
using System.Windows.Data;

namespace taskmaker_wpf.Views.Pages {
    public class SimplexView {
        public RootWidget_Wpf Root { get; set; }

        private ComplexWidget _complex;
        private ViewModels.RegionControlUIViewModel _viewModel;

        public Dictionary<string, object> Elements { get; set; } = new Dictionary<string, object>();


        public SimplexView(ViewModels.RegionControlUIViewModel vm) {
            _viewModel = vm;

            InitializeWidgets();
            HandleDataBinding();
        }

        private void HandleDataBinding() {
            var bind = new Binding() {
                Source = _viewModel
            };

            bind.Path = new PropertyPath("Count");
            BindingOperations.SetBinding(
                Elements["Debug"] as DependencyObject,
                DebugInfoWidget.MessageProperty,
                bind);
        }

        public void InitializeWidgets() {
            Root = new RootWidget_Wpf("Root");

            var debug = new DebugInfoWidget("Debug");

            Root.AddChild(debug);

            Elements.Add("Debug", debug);
            //var nodes = _viewModel.FetchNodes();

            //_complex = new ComplexWidget(
            //    "Complex",
            //    new ComplexWidgetState {
            //        width = 800,
            //        height = 600,
            //        location = new SKPoint(),
            //        nodes = nodes
            //    });

            //Root.AddChild(_complex);

            //var debugView = new Debug.DataMonitorWidget(
            //    "Debug",
            //    new Debug.DataMonitorState {
            //        Bound = new SKRect(0, 0, 200, 40),
            //        Seqs = new float[0]
            //    });

            //Root.AddChild(debugView);
        }

        //public T FindByName<T>(string name) where T: class {
        //    return Root.FindByName<T>(name);
        //}

        //public NodeWidget DeleteNode(SKPoint p) {
        //    foreach (var node in _complex.GetAllChild()) {
        //        if (node is NodeWidget widget) {
        //            if (widget.Contains(p))
        //                return widget;
        //        }
        //    }

        //    return default;
        //}

        //public IWidget FindTargetWidget(SKPoint p) {
        //    return FindTarget(Root, p);
        //}

        //private IWidget FindTarget(IWidget widget, SKPoint location) {
        //    var ret = widget.Contains(location);

        //    if (ret)
        //        return widget;
        //    else
        //        foreach (var item in widget.GetAllChild()) {
        //            var childRet = FindTarget(item, location);

        //            if (childRet != null)
        //                return childRet;
        //        }

        //    return null;
        //}
    }
}
