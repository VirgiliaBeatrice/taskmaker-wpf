using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public record TensorOperationRecord {
        public int Axis { get; init; }
        public int Index { get; init; }
    }

    public class NDKey {
        public List<int> Indices { get; }

        public NDKey(params int[] indices) {
            Indices = new List<int>(indices);
        }

        public override bool Equals(object obj) {
            if (obj is NDKey other) {
                return Enumerable.SequenceEqual(Indices, other.Indices);
            }
            return false;
        }

        public override int GetHashCode() {
            return Indices.Aggregate(0, (acc, val) => acc ^ val.GetHashCode());
        }
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
        [ObservableProperty]
        private int _dimension;

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


        // TODO: key for entry
        [RelayCommand]
        public void SetValue(MapEntry entry) {
            var entity = _mapSrv.Read(Id);

            entity.SetValue(new[] { -1 }.Concat(entry.Keys).ToArray(), entry.Value);
        }

        [RelayCommand]
        public void ClearValue(MapEntry entry) {
            var entity = _mapSrv.Read(Id);

            entity.SetValue(
                new[] { -1 }.Concat(entry.Keys).ToArray(), 
                Enumerable.Repeat(double.NaN, 6).ToArray());
        }

        public NLinearMapEntity ToEntity() {
            return new NLinearMapEntity(Shape) {
                Id = Id,
                Name = Name,
                Keys = Keys
            };
        }

        public void FromEntity(NLinearMapEntity entity) {
            Id = entity.Id;
            Name = entity.Name;
            Keys = entity.Keys;
            Shape = entity.Shape;        
        }

        public override string ToString() {
            return Name;
        }
    }

    public partial class NLinearMapCollectionViewModel : ObservableObject {
        private readonly MapService _mapSrv;


        public NLinearMapCollectionViewModel(MapService mapSrv) {
            _mapSrv = mapSrv;
        }

        public ObservableCollection<NLinearMapViewModel> MapViewModels { get; set; } = new();


        [RelayCommand]
        public void Create(int dim) {
            var entity = _mapSrv.Create(new NLinearMapEntity(dim, 6));
            var vm = new NLinearMapViewModel(_mapSrv);

            vm.FromEntity(entity);

            MapViewModels.Add(vm);
        }

        [RelayCommand]
        public void Delete(NLinearMapViewModel vm) {
            _mapSrv.Delete(vm.Id);
            MapViewModels.Remove(vm);
        }
    }

}