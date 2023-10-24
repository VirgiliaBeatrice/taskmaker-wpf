using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels {
    public partial class SessionViewModel : ObservableObject {
        public ObservableCollection<ControlUiViewModel> UiViewModels { get; init; }
        public NLinearMapViewModel MapViewModel { get; init; }


        public NodeState[][] UiVMNodeStates => UiViewModels.Select(e => e.NodeStates).ToArray();

        [ObservableProperty]
        private NodeState[] _selectedNodeStates;
        [ObservableProperty]
        private int[] _selectedNodeIndices;
        [ObservableProperty]
        private int[] _selectedNodeIds;
        [ObservableProperty]
        private MapEntry[] _entries;
        [ObservableProperty]
        private Point[] _inputs;
        [ObservableProperty]
        private BaseRegionEntity[] _regions;

        private readonly EvaluationService _evaluationSrv;
        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;
        private readonly MapService _mapSrv;

        public SessionViewModel(EvaluationService evaluationSrv, MotorService motorSrv, UIService uiSrv, MapService mapSrv) {
            _evaluationSrv = evaluationSrv;
            _motorSrv = motorSrv;
            _uiSrv = uiSrv;
            _mapSrv = mapSrv;

            WeakReferenceMessenger.Default.Register<UiViewModelNodeAddedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;

                var axis = UiViewModels.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = uiVM.NodeStates.Length - 1
                };

                MapViewModel.ExpandAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeDeletedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;

                var axis = UiViewModels.IndexOf(uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = m.NodeIndex
                };

                MapViewModel.RemoveAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeUpdatedMessage>(this, (r, m) => {
                var uiVM = m.ViewModel;


            });
        }

        partial void OnSelectedNodeStatesChanged(NodeState[] value) {
            SelectedNodeIds = SelectedNodeStates.Select(e => e.Id).ToArray();
            SelectedNodeIndices = SelectedNodeStates.Select(e => Array.FindIndex(UiVMNodeStates[Array.IndexOf(SelectedNodeStates, e)], n => n.Id == e.Id)).ToArray();
        }


        [RelayCommand]
        public void SetValue(double[] values) {
            //for (int i = 0; i < UiViewModels.Count; i++) {
            //    var selectedNode = SelectedNodeStates[i];
            //    var selectedNodeIndex = Array.FindIndex(UiVMNodeStates[i], e => e.Id == selectedNode.Id);

            //    SelectedNodeIndices[i] = selectedNodeIndex;
            //    SelectedNodeIds[i] = selectedNode.Id;
            //}

            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
                Value = values
            };

            MapViewModel.SetValue(mapEntry);
            Fetch();
            //SetEntry(mapEntry);
        }

        [RelayCommand]
        public void GetValue() {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
            };

            MapViewModel.GetValue(ref mapEntry);
            Fetch();
            //SetEntry(mapEntry);
        }

        [RelayCommand]
        public void Fetch() {
            //var dim = MapViewModel.Dimension;
            //if (dim == 0) return;

            //var shape = MapViewModel.Shape;
            //var entity = MapViewModel.Entity;

            //var indices = Enumerable.Range(1, dim - 1)
            //    .Select(i => Enumerable.Range(0, shape[i]).ToArray())
            //    .CartesianProduct()
            //    .Select(e => e.ToArray())
            //    .ToArray();

            //var values = indices.Select(e => entity.GetValue(new[] { -1 }.Concat(e).ToArray())).ToArray();

            //var entries = new List<MapEntry>();
            //for (int i = 0; i < indices.Length; i++) {
            //    entries.Add(new MapEntry {
            //        IDs = SelectedNodeIds,
            //        Indices = indices[i],
            //        Value = values[i]
            //    });
            //}

            //Entries = entries;
            Entries = MapViewModel.MapEntries.ToArray();
        }

        [RelayCommand]
        public void Interpolate() {
            var lambdasCollection = new List<double[]>();
            for (int i = 0; i < UiViewModels.Count; i++) {
                var nodes = UiViewModels[i].NodeStates;
                var input = UiViewModels[i].Input;
                var region = UiViewModels[i].HitRegion;

                if (region == null) return;

                var indices = region.Vertices.Select(e => Array.FindIndex(nodes, n => n.Id == e.Id)).ToArray();
                var lambdas = BaseRegionState.GetLambdas(region.Vertices.Select(e => e.Value).ToArray(), input, indices, nodes.Length);

                lambdasCollection.Add(lambdas);
            }

            MapViewModel.Interpolate(lambdasCollection.ToArray());
        }

        //public void SetEntry(MapEntry entry) {
        //    var idx = Entries.FindIndex(e => e.Indices == entry.Indices);

        //    if (idx != -1) {
        //        Entries[idx] = entry;
        //    }
        //    else {
        //        Entries.Add(entry);
        //    }

        //}
    }


}