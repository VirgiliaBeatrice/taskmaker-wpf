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
        private MapEntry[] _mapEntries;
        [ObservableProperty]
        private double[] _output;

        private readonly NLinearMapEntity _entity;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        partial void OnOutputChanged(double[] value) {
            //_logger.Debug($"Output changed: {string.Join(",", value)}");
            WeakReferenceMessenger.Default.Send(new MapOutputMessage() {
                Id = Id,
                Output = value
            });
        }



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

            if (Shape.Skip(1).Any(e => e == 0)) return;

            var indices = Enumerable.Range(1, Dimension - 1)
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

            MapEntries = entries.ToArray();
        }

        [RelayCommand]
        public void ExpandAt(TensorOperationRecord record) {
            _entity.ExpandAt(record.Axis, record.Index);

            Fetch();
        }

        [RelayCommand]
        public void RemoveAt(TensorOperationRecord record) {
            _entity.RemoveAt(record.Axis, record.Index);

            Fetch();
        }

        public void SetValue(int[] indices, double[] value) {
            _entity.SetValue((new[] { -1 }).Concat(indices).ToArray(), value);
            EventDispatcher.Record(new CreationAssignEvent());
        }

        // TODO: key for entry
        [RelayCommand]
        public void SetValue(MapEntry entry) {
            _entity.SetValue((new[] { -1 }).Concat(entry.Indices).ToArray(), entry.Value);
            _entity.MapEntries.Add(entry);

            Fetch();
        }

        public void ClearValue(int[] indices) {
            SetValue(indices, Enumerable.Repeat(double.NaN, 6).ToArray());
        }

        [RelayCommand]
        public void ClearValue(MapEntry entry) {
            _entity.SetValue(
                (new[] { -1 }).Concat(entry.Indices).ToArray(),
                Enumerable.Repeat(double.NaN, 6).ToArray());
            _entity.MapEntries.Remove(_entity.MapEntries.First(e => e == entry));

            Fetch();
        }

        public void GetValue(ref MapEntry entry) {
            var value = _entity.GetValue(
                (new[] { -1 }).Concat(entry.Indices).ToArray());

            entry.Value = value;
        }

        public double[] GetValue(int[] indices) {
            var value = _entity.GetValue(
                (new[] { -1 }).Concat(indices).ToArray());

            return value;
        }

        [RelayCommand]
        public void Interpolate(double[][] lambdas) {
            Output = _entity.MapTo(lambdas);
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

        public ObservableCollection<NLinearMapViewModel> Maps { get; set; } = new();


        [RelayCommand]
        public void Create(int dim) {
            var entity = _mapSrv.Create(new NLinearMapEntity(dim, 6));
            var vm = new NLinearMapViewModel(entity);

            Maps.Add(vm);
        }

        [RelayCommand]
        public void Delete(NLinearMapViewModel vm) {
            _mapSrv.Delete(vm.Id);
            Maps.Remove(vm);
        }
    }

}