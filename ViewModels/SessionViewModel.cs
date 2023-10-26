using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views.Widget;

namespace taskmaker_wpf.ViewModels
{
    public partial class SessionViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private NodeState[] _selectedNodeStates;
        [ObservableProperty]
        private int[] _selectedNodeIndices;
        [ObservableProperty]
        private int[] _selectedNodeIds;
        [ObservableProperty]
        private Point[] _inputCollection;
        [ObservableProperty]
        private NLinearMapViewModel _map;
        [ObservableProperty]
        private ControlUiViewModel[] _uis;
        [ObservableProperty]
        private MapEntryWidetViewModel _mapEntryWidget;
        [ObservableProperty]
        private UiMode _mode = UiMode.Default;
        [ObservableProperty]
        private bool _showWidget = false;

        partial void OnShowWidgetChanged(bool value) {
            if (value)
                FetchWidget();
        }

        partial void OnModeChanged(UiMode value) {
            if (value == UiMode.Control) {
                EventDispatcher.Record(new CreationTryControlEvent());
            }
        }


        public NodeState[][] NodeCollections => Uis.Select(e => e.NodeStates).ToArray();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SessionEntity _entity;

        public SessionViewModel(SessionEntity entity) {
            _entity = entity;

            WeakReferenceMessenger.Default.Register<UiViewModelNodeAddedMessage>(this, (r, m) => {
                var uiVM = m.Ui;

                var axis = Array.IndexOf(Uis, uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = uiVM.NodeStates.Length - 1
                };

                Map.ExpandAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeDeletedMessage>(this, (r, m) => {
                var uiVM = m.Ui;

                var axis = Array.IndexOf(Uis, uiVM);
                var op = new TensorOperationRecord {
                    Axis = axis + 1,
                    Index = m.NodeIndex
                };

                Map.RemoveAt(op);
            });

            WeakReferenceMessenger.Default.Register<UiViewModelNodeUpdatedMessage>(this, (r, m) => {
                var uiVM = m.Ui;


            });

            FetchThis();
            FetchUis();
            FetchMap();
            FetchWidget();
        }

        private void FetchWidget() {
            MapEntryWidget = new MapEntryWidetViewModel(this);

            if (SelectedNodeStates != null) {
                MapEntryWidget.Update();
            }

        }

        partial void OnSelectedNodeStatesChanged(NodeState[] value) {
            SelectedNodeIds = SelectedNodeStates.Select(e => e.Id).ToArray();
            SelectedNodeIndices = SelectedNodeStates.Select(e => Array.FindIndex(NodeCollections[Array.IndexOf(SelectedNodeStates, e)], n => n.Id == e.Id)).ToArray();
        }


        [RelayCommand]
        public void SetValue(double[] values) {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
                Value = values
            };

            Map.SetValue(mapEntry);
            //Fetch();
            FetchMap();
        }

        [RelayCommand]
        public void GetValue() {
            var mapEntry = new MapEntry {
                IDs = SelectedNodeIds,
                Indices = SelectedNodeIndices,
            };

            Map.GetValue(ref mapEntry);
            //Fetch();
            FetchMap();
        }

        [RelayCommand]
        public void FetchThis() {
            // Update all properties from entity
            // Update basic properties
            Id = _entity.Id;
            Name = _entity.Name;
        }

        private void FetchMap() {
            Map = new NLinearMapViewModel(_entity.Map);
        }

        private void FetchUis() {
            // Update Uis
            var uis = new List<ControlUiViewModel>();
            foreach (var entity in _entity.Uis) {
                var uiVM = new ControlUiViewModel(entity) {
                    NodeStates = entity.Nodes.Select(e => new NodeState(e.Id, e.Value)).ToArray()
                };

                uis.Add(uiVM);
            }

            Uis = uis.ToArray();
        }

        [RelayCommand]
        public void Interpolate() {
            var lambdasCollection = new List<double[]>();
            for (int i = 0; i < Uis.Length; i++) {
                _logger.Info($"{Uis[i].GetHashCode()}");
                var nodes = Uis[i].NodeStates;
                var input = Uis[i].Input;
                var region = Uis[i].HitRegion;

                if (region == null) return;

                var indices = region.Vertices.Select(e => Array.FindIndex(nodes, n => n.Id == e.Id)).ToArray();
                var lambdas = BaseRegionState.GetLambdas(region.Vertices.Select(e => e.Value).ToArray(), input, indices, nodes.Length);

                lambdasCollection.Add(lambdas);
            }

            Map.Interpolate(lambdasCollection.ToArray());
        }

        public override string ToString() {
            return Name;
        }
    }
}