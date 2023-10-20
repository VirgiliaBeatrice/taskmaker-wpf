using AutoMapper;
using Numpy;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SkiaSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Views;
using CommunityToolkit.Mvvm;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using taskmaker_wpf.Views.Widget;
using System.Collections.Generic;
using System.Xml.Linq;
using PCController;
using System.Windows.Controls;
using System.Xml.XPath;
using taskmaker_wpf.Model.Data;
using SharpVectors.Dom.Svg;
using System.Windows.Media;
using System.Text;
using NLog;
using System.Diagnostics;
using System.Collections.Immutable;
using taskmaker_wpf.Services;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using System.DirectoryServices;

namespace taskmaker_wpf.ViewModels {

    public static class Helper {

        public static IObservable<TSource> Dump<TSource>(this IObservable<TSource> observable) {
            observable.Subscribe(
                (e) => { Debug.WriteLine($"[{DateTime.Now}] OnNext({e})"); },
                (e) => { Debug.WriteLine($"[{DateTime.Now}] OnError({e})"); },
                () => { Debug.WriteLine($"[{DateTime.Now}] OnCompleted()"); });

            return observable;
        }

        public static void Dump<T>(this IObservable<T> source, string name) {
            _ = source.Subscribe(
                i => Debug.WriteLine("{0}-->{1}", name, i),
                ex => Debug.WriteLine("{0} failed-->{1}", name, ex.Message),
                () => Debug.WriteLine("{0} completed", name));
        }

        public static NDarray<float> ToNDarray(this Point pt) {
            return np.array((float)pt.X, (float)pt.Y);
        }

        public static NDarray<float> ToNDarray(this SKPoint pt) {
            return np.array(pt.X, pt.Y);
        }

        public static Point ToPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new Point { X = values[0], Y = values[1] };
        }

        public static SKPoint ToSKPoint(this NDarray pt) {
            if (pt.ndim > 1)
                throw new Exception("Invalid ndarray");

            var castValues = pt.astype(np.float32);
            var values = castValues.GetData<float>();

            castValues.Dispose();

            return new SKPoint(values[0], values[1]);
        }
    }

    public partial class NLinearMapState : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private int[] _keys;
        [ObservableProperty]
        private MapEntry<double[], double[]>[] _entries = new MapEntry<double[], double[]>[0];
        [ObservableProperty]
        private double[][] _values;

        public override string ToString() {
            return Name;
        }
    }

    public partial class ControlUiState : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private NodeState[] _nodes = new NodeState[0];
        [ObservableProperty]
        private BaseRegionState[] _regions;

        private readonly RegionControlUIViewModel _vm;
        public ControlUiState(RegionControlUIViewModel vm) {
            _vm = vm;
        }

        partial void OnNameChanged(string value) {
            if (value != _name)
                _vm.UpdateUi(this);
        }

        partial void OnNodesChanged(NodeState[] value) {
            if (value != _nodes)
                _vm.UpdateUi(this);
        }

        public override string ToString() {
            return Name;
        }
    }

    public struct NodeState {
        private int _id;
        private Point _value = new Point();

        public int Id { get => _id; set => _id = value; }
        public Point Value { get => _value; set => _value = value; }

        public NodeState(int id, Point value) {
            Id = id;
            Value = value;
        }
    }

    public partial class RegionControlUIViewModel : ObservableObject, INavigationAware {
        private readonly EvaluationService _evaluationSrv;
        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;
        private readonly MapService _mapSrv;

        public ObservableCollection<ControlUiState> UiStates { get; private set; } = new();
        public ObservableCollection<NLinearMapEntity> MapStates { get; private set; } = new();

        public RegionControlUIViewModel(EvaluationService evaluationService,MotorService motorService, UIService uIService, MapService mapService) {
            _mapSrv = mapService;
            _evaluationSrv = evaluationService;
            _motorSrv = motorService;
            _uiSrv = uIService;

            UiStates.CollectionChanged += UiStates_CollectionChanged;
            MapStates.CollectionChanged += MapStates_CollectionChanged;

            //WeakReferenceMessenger.Default.Register<UiUpdatedMessage>(this, (r, m) => {
            //    // TODO: unify the index start point
            //    var target = UiStates[m.Entity.Id - 1];

            //    target.Name = m.Entity.Name;
            //    target.Nodes = m.Entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray();
            //});
        }

        private void MapStates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (INotifyPropertyChanged item in e.NewItems) {
                    item.PropertyChanged += UiState_PropertyChanged;
                }
            }

            if (e.OldItems != null) {
                foreach (INotifyPropertyChanged item in e.OldItems) {
                    item.PropertyChanged -= UiState_PropertyChanged;
                }
            }
        }

        private void UiStates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (INotifyPropertyChanged item in e.NewItems) {
                    item.PropertyChanged += UiState_PropertyChanged;
                }
            }

            if (e.OldItems != null) {
                foreach (INotifyPropertyChanged item in e.OldItems) {
                    item.PropertyChanged -= UiState_PropertyChanged;
                }
            }
        }

        private void UiState_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            // Here, you can inform your model about the specific change
            // For example:
            var state = sender as ControlUiState;
            var entity = new ControlUiEntity {
                Id = state.Id,
                Name = state.Name,
                Nodes = state.Nodes.Select(e => new NodeEntity { Id = e.Id, Value = e.Value }).ToList(),
            };

            _uiSrv.Update(entity);
        }

        private void MapState_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            // Here, you can inform your model about the specific change
            // For example:
            var state = sender as NLinearMapState;
            var entity = _mapSrv.Read(state.Id);

            //entity.

            //_mapSrv.Update(entity);
        }

        [RelayCommand]
        private void CreateUi() {
            var entity = _uiSrv.Create(new ControlUiEntity());
            var state = new ControlUiState(this) {
                Id = entity.Id,
                Name = entity.Name,
                Nodes = entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray(),
                Regions = MapFromEntity(entity.Regions.ToArray()),
            };

            UiStates.Add(state);
        }

        [RelayCommand]
        private void TryBuild(ControlUiState state) {
            var entity = _uiSrv.Read(state.Id);

            if (entity.Nodes.Count >= 3) {
                entity.Build();

                var newRegions = entity.Regions.Select(entity =>
                {
                    if (entity is SimplexRegionEntity) {
                        return BaseRegionState.Create<SimplexRegionEntity, SimplexState>(entity as SimplexRegionEntity) as BaseRegionState;
                    }
                    else if (entity is VoronoiRegionEntity) {
                        return BaseRegionState.Create<VoronoiRegionEntity, VoronoiState>(entity as VoronoiRegionEntity);
                    }
                    else
                        throw new InvalidOperationException($"Unsupported entity type: {entity.GetType()}");
                }).ToArray();

                state.Regions = newRegions;
            }
        }


        [RelayCommand]
        private void CreateMap() {
            var entity = _mapSrv.Create(new NLinearMapEntity(new[] { 6, 0 }));

            var state = new NLinearMapState() {
                Id = entity.Id,
                Name = entity.Name,
                Keys = entity.Keys,
            };

            
        }

        [RelayCommand]
        public void UpdateUi(ControlUiState state) {
            var entity = _uiSrv.Read(state.Id);

            entity.Id = state.Id;
            entity.Name = state.Name;
            entity.Nodes = state.Nodes.Select(e => new NodeEntity { Id = e.Id, Value = e.Value }).ToList();

            _uiSrv.Update(entity);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            //throw new NotImplementedException();
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            //throw new NotImplementedException();
        }

        private BaseRegionState[] MapFromEntity(BaseRegionEntity[] entities) {
            return entities.Select(entity => {
                if (entity is SimplexRegionEntity) {
                    return BaseRegionState.Create<SimplexRegionEntity, SimplexState>(entity as SimplexRegionEntity) as BaseRegionState;
                }
                else if (entity is VoronoiRegionEntity) {
                    return BaseRegionState.Create<VoronoiRegionEntity, VoronoiState>(entity as VoronoiRegionEntity);
                }
                else
                    throw new InvalidOperationException($"Unsupported entity type: {entity.GetType()}");
            }).ToArray();
        }
    }

    public interface IRegionState {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class BaseRegionState : IRegionState {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point[] Vertices { get; set; } = new Point[0];

        public override string ToString() {
            return Name;
        }

        public static TOut Create<TIn, TOut>(TIn entity)
            where TIn : BaseRegionEntity, new()
            where TOut : BaseRegionState, new() {
            // Checking if the given entity is of type SimplexRegionEntity
            if (entity is SimplexRegionEntity simplexEntity) {
                return SimplexState.Create(simplexEntity) as TOut;
            }
            // Add more type checks as necessary for other region entity types.
            // Example:
            else if (entity is VoronoiRegionEntity) {
                return VoronoiState.Create(entity as VoronoiRegionEntity) as TOut;
            }
            else
            // Handle the case where the entity type doesn't match any known state creation logic.
                throw new InvalidOperationException($"No corresponding state type found for entity type {typeof(TIn)}");
        }
    }

    public class SimplexState : BaseRegionState {
        public static SimplexState Create(SimplexRegionEntity entity) {
            return new SimplexState() {
                Id = entity.Id,
                Name = entity.Name,
                Vertices = entity.Vertices
            };
        }
    }

    public class VoronoiState : BaseRegionState {
        // add a create method for VoronoiState
        public static VoronoiState Create(VoronoiRegionEntity entity) {
            return new VoronoiState() {
                Id = entity.Id,
                Name = entity.Name,
                Vertices = entity.Vertices
            };
        }
    }

}