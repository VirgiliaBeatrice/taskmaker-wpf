using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public static class Helper {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>
        (this IEnumerable<IEnumerable<T>> sequences) {
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            IEnumerable<IEnumerable<T>> result = emptyProduct;
            foreach (IEnumerable<T> sequence in sequences) {
                result = from accseq in result from item in sequence select accseq.Concat(new[] { item });
            }
            return result;
        }

    }
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

        public override string ToString() {
            return $"[{string.Join(",", Indices)}]";
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
        [ObservableProperty]
        private MapEntry[] _entries;

        private readonly MapService _mapSrv;

        public NLinearMapViewModel(MapService mapSrv) {
            _mapSrv = mapSrv;
        }

        [RelayCommand]
        public void ExpandAt(TensorOperationRecord record) {
            var entity = _mapSrv.Read(Id);

            entity.ExpandAt(record.Axis, record.Index);

            Shape = entity.Shape;

            Fetch();
        }

        [RelayCommand]
        public void RemoveAt(TensorOperationRecord record) {
            var entity = _mapSrv.Read(Id);

            entity.RemoveAt(record.Axis, record.Index);

            Shape = entity.Shape;
            Fetch();
        }


        // TODO: key for entry
        [RelayCommand]
        public void SetValue(MapEntry entry) {
            var entity = _mapSrv.Read(Id);

            entity.SetValue(new[] { -1 }.Concat(entry.Indices).ToArray(), entry.Value);
            Fetch();
        }

        [RelayCommand]
        public void ClearValue(MapEntry entry) {
            var entity = _mapSrv.Read(Id);

            entity.SetValue(
                new[] { -1 }.Concat(entry.Indices).ToArray(), 
                Enumerable.Repeat(double.NaN, 6).ToArray());
            
            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            var entity = _mapSrv.Read(Id);

            var dim = entity.Shape[0];
            
            var shape = entity.Shape.Skip(1).ToArray();

            // make all index combination of shape
            var indices = Enumerable.Range(0, shape.Length)
                .Select(i => Enumerable.Range(0, shape[i]))
                .CartesianProduct()
                .ToArray();
            var values = indices.Select(e => entity.GetValue(new[] { -1 }.Concat(e).ToArray()))
                .ToArray();

            var entries = new List<MapEntry>();

            for (int i = 0; i < indices.Length; i++) {
                var entry = new MapEntry() {
                    Indices = indices.ElementAt(i).ToArray(),
                    Value = values[i]
                };
                entries.Add(entry);
            }

            Entries = entries.ToArray();
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