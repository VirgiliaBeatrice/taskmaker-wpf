using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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

        public ObservableCollection<EvaluationViewModel> Evaluations { get; private set; } = new ObservableCollection<EvaluationViewModel>();

        //public ControlUiCollectionViewModel UiCollection { get; private set; }

        //public ObservableCollection<ControlUiViewModel> Uis => UiCollection.Uis;

        [ObservableProperty]
        private EvaluationViewModel _selectedEvaluation;
        [ObservableProperty]
        private bool _isEvaluationStarted = false;
        [ObservableProperty]
        private bool _isPracticeStarted = false;
        [ObservableProperty]
        private bool _isCreationStarted = false;
        [ObservableProperty]
        private bool _isPerformStarted = false;

        //partial void OnSelectedSessionChanged(SessionViewModel value) {
        //    if (value != null)
        //        WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage {
        //            Button = MessageBoxButton.OK,
        //            Caption = "Session Selected",
        //            Icon = MessageBoxImage.Information,
        //            Message = $"Session {value.Name} selected.",
        //        });
        //}

        public RegionControlUIViewModel(EvaluationService evaluationService,MotorService motorService, UIService uIService, MapService mapService) {
            _mapSrv = mapService;
            _evaluationSrv = evaluationService;
            _motorSrv = motorService;
            _uiSrv = uIService;

            //UiCollection = new ControlUiCollectionViewModel(_uiSrv);

            //WeakReferenceMessenger.Default.Register<ValueChangedMessage<SessionViewModel>>(this, (r, m) => {
            //    SelectedSession = m.Value;
            //});
        }

        [RelayCommand]
        public void CreateEvaluation() {
            var evaEntity = _evaluationSrv.Create(new EvaluationEntity());
            var evaVM = new EvaluationViewModel(evaEntity);

            Evaluations.Add(evaVM);
        }


        [RelayCommand]
        public void Fetch() {
            //UiCollection.Fetch();
        }

        [RelayCommand]
        public void Evaluation() {
            if (IsEvaluationStarted)
                EventDispatcher.Record(new EvaluationStoppedEvent());
            else
                EventDispatcher.Record(new EvaluationStartedEvent());

            IsEvaluationStarted = !IsEvaluationStarted;
        }

        [RelayCommand]
        public void Practice() {
            if (IsPracticeStarted)
                EventDispatcher.Record(new PracticeStoppedEvent());
            else
                EventDispatcher.Record(new PracticeStartedEvent());

            IsPracticeStarted = !IsPracticeStarted;
        }

        [RelayCommand]
        public void Creation() {
            if (IsCreationStarted)
                EventDispatcher.Record(new CreationStoppedEvent());
            else
                EventDispatcher.Record(new CreationStartedEvent());
            IsCreationStarted = !IsCreationStarted;
        }

        [RelayCommand]
        public void Perform() {
            if (IsPerformStarted)
                EventDispatcher.Record(new PerformStoppedEvent());
            else
                EventDispatcher.Record(new PerformStartedEvent());

            IsPerformStarted = !IsPerformStarted;
        }


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