using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Views.Widgets;
using taskmaker_wpf.Views.Widgets.Container;
using SkiaSharp;

namespace taskmaker_wpf.Views.Pages {
    public class SimplexView {
        public Widget Root { get; set; }

        private ComplexWidget _complex;
        private ViewModels.ComplexViewModel _viewModel;

        public SimplexView(ViewModels.ComplexViewModel vm) {
            _viewModel = vm;

            InitializeWidgets();
        }

        public void InitializeWidgets() {
            Root = new RootContainerWidget("Root");

            var nodes = _viewModel.FetchNodes();

            _complex = new ComplexWidget(
                "Complex",
                new ComplexWidgetState {
                    width = 800,
                    height = 600,
                    location = new SKPoint(),
                    nodes = nodes
                });

            Root.AddChild(_complex);

            var debugView = new Debug.DataMonitorWidget(
                "Debug",
                new Debug.DataMonitorState {
                    Bound = new SKRect(0,0,200,40),
                    Seqs = new float[0]
                });

            Root.AddChild(debugView);
        }

        public NodeWidget DeleteNode(SKPoint p) {
            foreach(var node in _complex.GetAllChild()) {
                if (node is NodeWidget widget) {
                    if (widget.Contains(p))
                        return widget;
                }
            }

            return default;
        }
    }
}
