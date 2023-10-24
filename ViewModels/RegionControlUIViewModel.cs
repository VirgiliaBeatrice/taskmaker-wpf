using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Numpy;
using Prism.Ioc;
using Prism.Regions;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views;
using taskmaker_wpf.Views.Widget;


namespace taskmaker_wpf.ViewModels {

    public partial class RegionControlUIViewModel : ObservableObject, INavigationAware {
        private readonly EvaluationService _evaluationSrv;
        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;
        private readonly MapService _mapSrv;
        private static IContainerProvider Container => (App.Current as Prism.PrismApplicationBase).Container;

        public ControlUiCollectionViewModel UiCollectionViewModel { get; private set; }

        public ObservableCollection<ControlUiViewModel> UiVMs => UiCollectionViewModel.VMs;

        [ObservableProperty]
        private SessionViewModel _sessionViewModel;

        public RegionControlUIViewModel(EvaluationService evaluationService,MotorService motorService, UIService uIService, MapService mapService) {
            _mapSrv = mapService;
            _evaluationSrv = evaluationService;
            _motorSrv = motorService;
            _uiSrv = uIService;

            UiCollectionViewModel = new ControlUiCollectionViewModel(_uiSrv);
        }

        [RelayCommand]
        public void CreateSession() {
            SessionViewModel = new SessionViewModel(_evaluationSrv, _motorSrv, _uiSrv, _mapSrv) {
                UiViewModels = new ObservableCollection<ControlUiViewModel>(),
                MapViewModel = new NLinearMapViewModel(_mapSrv)
            };

            // Init UIs
            var entity = new ControlUiEntity();
            var uiVM = new ControlUiViewModel(_uiSrv);

            _uiSrv.Create(entity);
            uiVM.FromEntity(entity);

            SessionViewModel.UiViewModels.Add(uiVM);

            // Init Map
            var mapEntity = new NLinearMapEntity(2, 6);
            var mapVM = SessionViewModel.MapViewModel;

            _mapSrv.Create(mapEntity);
            mapVM.FromEntity(mapEntity);

            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            UiCollectionViewModel.Fetch();
        }

        //[RelayCommand]
        //private void CreateUi() {
        //    var entity = _uiSrv.Create(new ControlUiEntity());

        //    var uiVM = Container.Resolve<ControlUiViewModel>();

        //    uiVM.Id = entity.Id;
        //    uiVM.Name = entity.Name;

        //    UiVMs.Add(uiVM);
        //}

        //[RelayCommand]
        //private void CreateMap() {
        //    // TODO: hard coding
        //    if (SelectedMapVM == null) {
        //        // send message, no ui has been selected
        //        var msg = new ShowMessageBoxMessage() {
        //            Message = "No ui has been selected.",
        //            Icon = MessageBoxImage.Error,
        //            Button = MessageBoxButton.OK,
        //            Caption = "Error"
        //        };

        //        WeakReferenceMessenger.Default.Send(msg);

        //        return;
        //    }

        //    var entity = _mapSrv.Create(new NLinearMapEntity(new[] { 6, 0 }) {
        //        Keys = new[] { SelectedUiVM.Id },
        //    });

        //    var mapVM = Container.Resolve<NLinearMapViewModel>();

        //    mapVM.Id = entity.Id;
        //    mapVM.Name = entity.Name;
        //    mapVM.Keys = entity.Keys;
        //    mapVM.Shape = entity.Shape;

        //    MapVMs.Add(mapVM);
        //}

        //[RelayCommand]
        //private void SetMapEntry(MapEntry entry) {
        //    var mapState = SelectedMapVM;
        //    var entity = _mapSrv.Read(mapState.Id);
        //    var indices = new[] { -1 }.Concat(entry.Key).ToArray();

        //    entity.SetValue(indices, entry.Value);

        //    //UpdateMapState(entity, ref mapState);
        //}


        [RelayCommand]
        private async Task RequestMotorDialog() {
            // Send dialog message
            var result = await WeakReferenceMessenger.Default.Send(new DialogRequestMessage());
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
        public NodeState[] Vertices { get; set; } = new NodeState[0];

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

        public static double[] GetLambdas(Point[] pts, Point pt, int[] indices, int length) {
            var lambdas = Bary.GetLambdas(pts, pt);
            var expandedLambdas = Enumerable.Repeat(0.0, length).ToArray();

            for (var idx = 0; idx < lambdas.Length; idx++) {
                expandedLambdas[indices[idx]] = lambdas[idx];
            }

            return expandedLambdas;
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