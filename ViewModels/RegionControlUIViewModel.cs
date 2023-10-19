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

    public class NLinearMapState {
        public int Id { get; set; }
        public string Name { get; set; }

        public int[] Shape { get; set; }
        public double[] Value { get; set; }

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

        public ObservableCollection<ControlUiState> UiStates { get; private set; } = new();

        public RegionControlUIViewModel(EvaluationService evaluationService,MotorService motorService, UIService uIService) {
            _evaluationSrv = evaluationService;
            _motorSrv = motorService;
            _uiSrv = uIService;

            UiStates.CollectionChanged += UiStates_CollectionChanged;

            //WeakReferenceMessenger.Default.Register<UiUpdatedMessage>(this, (r, m) => {
            //    // TODO: unify the index start point
            //    var target = UiStates[m.Entity.Id - 1];

            //    target.Name = m.Entity.Name;
            //    target.Nodes = m.Entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray();
            //});
        }

        private void UiStates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (INotifyPropertyChanged item in e.NewItems) {
                    item.PropertyChanged += State_PropertyChanged;
                }
            }

            if (e.OldItems != null) {
                foreach (INotifyPropertyChanged item in e.OldItems) {
                    item.PropertyChanged -= State_PropertyChanged;
                }
            }
        }

        private void State_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            // Here, you can inform your model about the specific change
            // For example:
            var state = sender as ControlUiState;
            var entity = new ControlUiEntity {
                Id = state.Id,
                Name = state.Name,
                Nodes = state.Nodes.Select(e => new NodeEntity { Id = e.Id, Value = e.Value }).ToArray(),
            };

            _uiSrv.Update(entity);
        }

        [RelayCommand]
        private void CreateUi() {
            var entity = _uiSrv.Create(new ControlUiEntity());
            var state = new ControlUiState(this) {
                Id = entity.Id,
                Name = entity.Name,
                Nodes = entity.Nodes?.Select(e => new NodeState(e.Id, e.Value)).ToArray(),
            };

            UiStates.Add(state);
        }

        [RelayCommand]
        private void CreateMap() {

        }

        [RelayCommand]
        public void UpdateUi(ControlUiState state) {
            var entity = new ControlUiEntity {
                Id = state.Id,
                Name = state.Name,
                Nodes = state.Nodes.Select(e => new NodeEntity { Id = e.Id, Value = e.Value }).ToArray(),
            };

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
    }

    public interface IRegionState {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class BaseRegionState : IRegionState {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimplexState : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];

        public override string ToString() {
            return Name;
        }
    }


    public class VoronoiState : BaseRegionState {
        public Point[] Points { get; set; } = new Point[0];
        public override string ToString() {
            return Name;
        }
    }
}