using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

//namespace taskmaker_wpf.Views.Widgets {
//    public struct GridWidgetState {
//        public int row;
//        public int column;
//        public float width;
//        public float height;
//    }

//    public class GridWidget : ContainerWidget {
//        public GridWidgetState State { get; set; }
//        public SKRect ClipBound { get; set; }

//        public GridWidget(string name, GridWidgetState initState) : base(name) {
//            State = initState;

//            ClipBound = new SKRect {
//                Right = State.width,
//                Bottom = State.height,
//            };
//        }

//        public override bool Contains(SKPoint p) {
//            return ClipBound.Contains(p);
//        }

//        public void Layout() { }

//        public GridItemWidgetState GetItemState(int row, int column) {
//            var width = State.width / (row + 1);
//            var height = State.height / (column + 1);
//            var x = width * row;
//            var y = height * column;

//            return new GridItemWidgetState {
//                rowIdx = row,
//                columnIdx = column,
//                location = new SKPoint(x, y),
//                width = width,
//                height = height,
//            };
//        }
//    }

//    public struct GridItemWidgetState {
//        public int rowIdx;
//        public int columnIdx;
//        public SKPoint location;
//        public float width;
//        public float height;
//    }

//    public class GridItemWidget : ContainerWidget {
//        public GridItemWidgetState State { get; set; }
//        public SKRect ClipBound { get; set; }

//        private int _paintLayer;

//        public GridItemWidget(string name, GridItemWidgetState initState) : base(name) {
//            State = initState;

//            ClipBound = new SKRect {
//                Right = State.width,
//                Bottom = State.height,
//            };
//        }

//        public override bool Contains(SKPoint p) {
//            return ClipBound.Contains(p);
//        }

//        public override void OnPainting(SKCanvas canvas) {
//            _paintLayer = canvas.Save();

//            canvas.SetMatrix(_transform);
//            canvas.ClipRect(ClipBound);

//            // if it has any render object
//            if (RenderObject != null)
//                canvas.DrawPicture(RenderObject.Picture);
//        }

//        public override void OnPainted(SKCanvas canvas) {
//            canvas.RestoreToCount(_paintLayer);
//        }
//    }
//}
