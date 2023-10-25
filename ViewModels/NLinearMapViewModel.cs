using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
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

    public class MapOutputMessage {
        public int Id { get; init; }
        public double[] Output { get; init; }
    }

    public partial class NLinearMapViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private int[] _shape;
        [ObservableProperty]
        private int _dimension;

        [ObservableProperty]
        private double[] _output;
        private readonly NLinearMapEntity _entity;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        partial void OnOutputChanged(double[] value) {
            _logger.Debug($"Output changed: {string.Join(",", value)}");
            WeakReferenceMessenger.Default.Send(new MapOutputMessage() {
                Id = Id,
                Output = value
            });
        }

        public ObservableCollection<MapEntry> MapEntries { get; set; } = new ObservableCollection<MapEntry>();


        public NLinearMapViewModel(NLinearMapEntity entity) {
            _entity = entity;

            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            // fetch all properties into this from entity
            Id = _entity.Id;
            Name = _entity.Name;
            Shape = _entity.Shape;
            Dimension = _entity.Dimension??-1;

            MapEntries.Clear();

            if (Shape.Skip(1).Any(e => e == 0)) return;

            var indices = Enumerable.Range(1, Dimension)
                .Select(i => Enumerable.Range(0, Shape[i]).ToArray())
                .CartesianProduct()
                .Select(e => e.ToArray())
                .ToArray();

            var values = indices.Select(e => _entity.GetValue(new[] { -1 }.Concat(e).ToArray())).ToArray();

            var entries = new List<MapEntry>();
            for (int i = 0; i < indices.Length; i++) {
                entries.Add(new MapEntry {
                    Indices = indices[i],
                    Value = values[i]
                });
            }

            MapEntries = new ObservableCollection<MapEntry>(entries);
        }

        [RelayCommand]
        public void ExpandAt(TensorOperationRecord record) {
            var entity = _entity;

            entity.ExpandAt(record.Axis, record.Index);

            Shape = entity.Shape;

            //Fetch();
        }

        [RelayCommand]
        public void RemoveAt(TensorOperationRecord record) {
            var entity = _entity;

            entity.RemoveAt(record.Axis, record.Index);

            Shape = entity.Shape;
        }


        // TODO: key for entry
        [RelayCommand]
        public void SetValue(MapEntry entry) {
            var entity = _entity;

            entity.SetValue(new[] { -1 }.Concat(entry.Indices).ToArray(), entry.Value);

            MapEntries.Remove(entry);
            MapEntries.Add(entry);
        }

        [RelayCommand]
        public void ClearValue(MapEntry entry) {
            var entity = _entity;

            entity.SetValue(
                new[] { -1 }.Concat(entry.Indices).ToArray(), 
                Enumerable.Repeat(double.NaN, 6).ToArray());
            
            MapEntries.Remove(entry);
        }

        public void GetValue(ref MapEntry entry) {
            var entity = _entity;

            var value = entity.GetValue(
                new[] { -1 }.Concat(entry.Indices).ToArray());

            entry.Value = value;
        }

        [RelayCommand]
        public void Interpolate(double[][] lambdas) {
            Output = _entity.MapTo(lambdas);
        }

        //public MapEntry[] Fetch() {
        //    var entity = _mapSrv.Read(Id);


        //    var indices = Enumerable.Range(1, Dimension)
        //        .Select(i => Enumerable.Range(0, Shape[i]).ToArray())
        //        .CartesianProduct()
        //        .Select(e => e.ToArray())
        //        .ToArray();

        //    var values = indices.Select(e => entity.GetValue(new[] { -1 }.Concat(e).ToArray())).ToArray();

        //    var entries = new List<MapEntry>();
        //    for (int i = 0; i < indices.Length; i++) {
        //        entries.Add(new MapEntry {
        //            Indices = indices[i],
        //            Value = values[i]
        //        });
        //    }

        //    return entries.ToArray();
        //}

        public NLinearMapEntity ToEntity() {
            return new NLinearMapEntity(Shape) {
                Id = Id,
                Name = Name,
            };
        }

        public void FromEntity(NLinearMapEntity entity) {
            Id = entity.Id;
            Name = entity.Name;
            Shape = entity.Shape;
            Dimension = entity.Dimension??0;
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
            var vm = new NLinearMapViewModel(entity);

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