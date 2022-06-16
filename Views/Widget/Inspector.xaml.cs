using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Views.Widget {
    /// <summary>
    /// Interaction logic for Inspector.xaml
    /// </summary>
    public partial class Inspector : UserControl {
        public FrameworkElement InspectedWidget {
            get { return (FrameworkElement)GetValue(InspectedWidgetProperty); }
            set { SetValue(InspectedWidgetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InspectedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InspectedWidgetProperty =
            DependencyProperty.Register(
                "InspectedWidget",
                typeof(FrameworkElement),
                typeof(Inspector),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));



        public Inspector() {
            InitializeComponent();
        }
    }

    public interface IInspectorTarget { }

    public class InspectorTargetTemplateSelector : DataTemplateSelector {
        public DataTemplate NodeDataTemplate { get; set; }
        public DataTemplate MotorDataTemplate { get; set; }
        public DataTemplate ComplexWidgetDataTemplate { get; set; }
        public DataTemplate SimplexWidgetDataTemplate { get; set; }
        public DataTemplate VoronoiWidgetDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var target = (FrameworkElement)item;

            if (target is NodeWidget) {
                return NodeDataTemplate;
            }
            else if (target is ComplexWidget) {
                return ComplexWidgetDataTemplate;
            }

            else if (target is SimplexWidget) {
                return SimplexWidgetDataTemplate;
            }
            else if (target is VoronoiWidget) {
                return VoronoiWidgetDataTemplate;
            }
            //else if (target is Motor) {
            //    return MotorDataTemplate;
            //}
            else
                return base.SelectTemplate(item, container);
        }
    }

    public class SystemTypeTemplateSelector : DataTemplateSelector {
        public DataTemplate BooleanTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is bool)
                return BooleanTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
