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
        public RootWidget Root { get; set; }

        //private ComplexWidget _complex;
        private ViewModels.RegionControlUIViewModel _viewModel;

        public Dictionary<string, object> Elements { get; set; } = new Dictionary<string, object>();


        public SimplexView(ViewModels.RegionControlUIViewModel vm) {
            _viewModel = vm;

            InitializeWidgets();
            HandleDataBinding();
        }

        private Binding CreateBinding(string path) {
            return new Binding {
                Source = _viewModel,
                Path = new PropertyPath(path)
            };
        }

        private void HandleDataBinding() {
            BindingOperations.SetBinding(
                Elements["Debug"] as DependencyObject,
                DebugInfoWidget.MessageProperty,
                CreateBinding("Count"));
            //BindingOperations.SetBinding(
            //    Elements["Complex"] as DependencyObject,
            //    ComplexWidget.NodeSourceProperty,
            //    CreateBinding("Nodes_v1"));
            //BindingOperations.SetBinding(
            //    Elements["Complex"] as DependencyObject,
            //    ComplexWidget.CommandProperty,
            //    CreateBinding("ItemRemoveCmd"));
        }

        public void InitializeWidgets() {
            Root = new RootWidget("Root");

            var debug = new DebugInfoWidget("Debug") {
                DataContext = _viewModel
            };

            Root.AddChild(debug);

            Elements.Add("Debug", debug);

            //var complex = new ComplexWidget("Complex") {
            //    DataContext = _viewModel
            //};

            //Root.AddChild(complex);

            //Elements.Add("Complex", complex);
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
