using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views;
using CMessageBox = taskmaker_wpf.Views.MessageBox;
using AutoMapper;
using taskmaker_wpf.Data;
using taskmaker_wpf.Domain;
using Prism.Events;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;

namespace taskmaker_wpf {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication {
        protected override Window CreateShell() {
            var w = Container.Resolve<TestWindow>();

            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {
            var config = new MapperConfiguration(
            cfg => {
                cfg.CreateMap<NLinearMapEntity, NLinearMapDTO>()
                    .ReverseMap();

                cfg.CreateMap<NodeEntity, NodeDTO>(MemberList.Destination)
                    .ReverseMap();

                cfg.CreateMap<BaseRegionEntity, RegionDTO>()
                    .IncludeAllDerived()
                    .ReverseMap();

                cfg.CreateMap<SimplexRegionEntity, SimplexRegionDTO>()
                    .ForMember(d => d.Vertices, o => o.MapFrom(s => s.Vertices))
                    .IncludeBase<BaseRegionEntity, RegionDTO>()
                    .ReverseMap();

                cfg.CreateMap<VoronoiRegionEntity, VoronoiRegionDTO>()
                    .ForMember(d => d.Vertices, o => o.MapFrom(s => s.Vertices))
                    .IncludeBase<BaseRegionEntity, RegionDTO>()
                    .ReverseMap();

                cfg.CreateMap<ControlUiEntity, ControlUiDTO>()
                    .ForMember(d => d.Nodes, o => o.MapFrom(s => s.Nodes))
                    .ForMember(d => d.Regions, o => o.MapFrom(s => s.Regions))
                    .ReverseMap();

                cfg.CreateMap<MotorEntity, MotorDTO>()
                    .ReverseMap();

                cfg.CreateMap<MotorEntity, MotorState>()
                    .ForMember(d => d.Value, o => o.Ignore())
                    .ReverseMap();
                //.ForMember(d => d.Value, o => o.MapFrom(s => new double[] { s.Value }));
                //cfg.CreateMap<MotorEntity, MotorTargetState>()
                //    .ForMember(d => d.IsSelected, o => o.Ignore())
                //    .ReverseMap();

                cfg.CreateMap<NLinearMapEntity, NLinearMapState>()
                    .ForMember(d => d.Value, o => o.MapFrom(s => s.Tensor.GetData<double>()));
                cfg.CreateMap<MotorEntity, MotorState>()
                    .ReverseMap();
                cfg.CreateMap<ControlUiEntity, ControlUiState>();
                
                cfg.CreateMap<NodeEntity, NodeState_v1>();

                cfg.CreateMap<BaseRegionEntity, BaseRegionState>();
                cfg.CreateMap<SimplexRegionEntity, SimplexState>()
                    .IncludeBase<BaseRegionEntity, BaseRegionState>()
                    .ForMember(d => d.Points, o => o.MapFrom(s => s.Vertices));
                cfg.CreateMap<VoronoiRegionEntity, VoronoiState>()
                    .IncludeBase<BaseRegionEntity, BaseRegionState>()
                    .ForMember(d => d.Points, o => o.MapFrom(s => s.Vertices));
            });


            // Register services
            containerRegistry.RegisterSingleton<SerialService>();
            //containerRegistry.RegisterSingleton<SystemService>();

            // Register model agent
            containerRegistry.RegisterSingleton<MapperConfiguration>(
                () => {
                    //config.AssertConfigurationIsValid();
                    return config;
            });

            // Register IDataSource
            //containerRegistry.RegisterSingleton<IDataSource>(() => {
            //    return LocalDataSource.Load(Container.Resolve<IEventAggregator>());
            //});
            //containerRegistry.RegisterSingleton<IEventAggregator>();

            // Register IRepository
            //containerRegistry.RegisterSingleton<IRepository, MotorRepository>("1");
            //containerRegistry.Register<IRepository, ControlUiRepository>("2");
            //containerRegistry.Register<IRepository, NLinearMapRepository>("3");
            containerRegistry.RegisterSingleton<IRepository, ProjectRepository>();

            // Register IUseCase
            //containerRegistry.RegisterSingleton<IUseCase, MotorUseCase>("1");
            //containerRegistry.RegisterSingleton<IUseCase, ControlUiUseCase>("2");
            //containerRegistry.RegisterSingleton<IUseCase, NLinearMapUseCase>("3");
            //containerRegistry.RegisterSingleton<IUseCase, ListTargetUseCase>("ListTargetUseCase");
            //containerRegistry.RegisterSingleton<IUseCase, BuildRegionUseCase>("BuildRegionUseCase");


            containerRegistry.Register<SystemInteractorBus>();

            // Register Interactor Bus
            containerRegistry.RegisterSingleton<MotorInteractorBus>();
            containerRegistry.RegisterSingleton<ControlUiInteractorBus>();
            containerRegistry.RegisterSingleton<NLinearMapInteractorBus>();

            //containerRegistry.Register<IPresenter>(() => Container.Resolve<TargetsPanelViewModel>());
            containerRegistry.Register<IPresenter>(() => Container.Resolve<RegionControlUIViewModel>());

            // Register messagebox
            containerRegistry.RegisterDialog<CMessageBox, MessageBoxViewModel>("standard");

            // Register regions for navigation
            containerRegistry.RegisterForNavigation<RegionHome>();
            containerRegistry.RegisterForNavigation<RegionMotor>();
            containerRegistry.RegisterForNavigation<RegionSettings>();
            containerRegistry.RegisterForNavigation<RegionControlUI>();
            containerRegistry.RegisterForNavigation<RegionSlider>();
            //containerRegistry.RegisterForNavigation<RegionSliderPanda>();

            ViewModelLocationProvider.Register<RegionSlider>(() => Container.Resolve<RegionMotorViewModel>());
            //ViewModelLocationProvider.Register<RegionSliderPanda>(() => Container.Resolve<RegionMotorViewModel>());
        }

        protected override void Initialize() {
            base.Initialize();
        }
    }
}
