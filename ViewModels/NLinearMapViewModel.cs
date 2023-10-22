using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public record TensorOperationRecord {
        public int Axis { get; init; }
        public int Index { get; init; }
    }

    public partial class NLinearMapViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private int[] _keys;
        [ObservableProperty]
        private int[] _shape;

        private readonly MapService _mapSrv;

        public NLinearMapViewModel(MapService mapSrv) {
            _mapSrv = mapSrv;
        }

        [RelayCommand]
        public void ExpandAt(TensorOperationRecord record) {
            var entity = _mapSrv.Read(Id);

            entity.ExpandAt(record.Axis, record.Index);

            Shape = entity.Shape;
        }

        [RelayCommand]
        public void RemoveAt(TensorOperationRecord record) {
            var entity = _mapSrv.Read(Id);

            entity.RemoveAt(record.Axis, record.Index);

            Shape = entity.Shape;
        }

        [RelayCommand]
        public void SetValue(MapEntry entry) {
            var entity = _mapSrv.Read(Id);

            entity.SetValue(new[] { -1 }.Concat(entry.Key).ToArray(), entry.Value);
        }

        public override string ToString() {
            return Name;
        }
    }

}