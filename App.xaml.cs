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
//using CMessageBox = taskmaker_wpf.Views.MessageBox;
using AutoMapper;
using taskmaker_wpf.Entity;
using Prism.Events;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;
using taskmaker_wpf.Views.Widget;

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
                cfg.CreateMap<MotorEntity, MotorState>()
                    .ForMember(d => d.Value, o => o.Ignore())
                    .ReverseMap();
                //.ForMember(d => d.Value, o => o.MapFrom(s => new double[] { s.Value }));
                //cfg.CreateMap<MotorEntity, MotorTargetState>()
                //    .ForMember(d => d.IsSelected, o => o.Ignore())
                //    .ReverseMap();

                cfg.CreateMap<NLinearMapEntity, NLinearMapViewModel>()
                    .ReverseMap();
                cfg.CreateMap<MotorEntity, MotorState>()
                    .ReverseMap();
                cfg.CreateMap<ControlUiEntity, ControlUiViewModel>();
                
                cfg.CreateMap<NodeEntity, NodeState>();

                cfg.CreateMap<BaseRegionEntity, BaseRegionState>();
                cfg.CreateMap<SimplexRegionEntity, SimplexState>()
                    .IncludeBase<BaseRegionEntity, BaseRegionState>()
                    .ForMember(d => d.Vertices, o => o.MapFrom(s => s.Vertices));
                cfg.CreateMap<VoronoiRegionEntity, VoronoiState>()
                    .IncludeBase<BaseRegionEntity, BaseRegionState>()
                    .ForMember(d => d.Vertices, o => o.MapFrom(s => s.Vertices));
            });


            // Register services
            containerRegistry.RegisterSingleton<SerialService>();
            containerRegistry.RegisterSingleton<EvaluationService>();
            containerRegistry.RegisterSingleton<MotorService>();
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
            containerRegistry.Register<DialogController>();

            containerRegistry.Register<ControlUiViewModel>();
            containerRegistry.Register<NLinearMapViewModel>();

            // Register IUseCase
            //containerRegistry.RegisterSingleton<IUseCase, MotorUseCase>("1");
            //containerRegistry.RegisterSingleton<IUseCase, ControlUiUseCase>("2");
            //containerRegistry.RegisterSingleton<IUseCase, NLinearMapUseCase>("3");
            //containerRegistry.RegisterSingleton<IUseCase, ListTargetUseCase>("ListTargetUseCase");
            //containerRegistry.RegisterSingleton<IUseCase, BuildRegionUseCase>("BuildRegionUseCase");

            //containerRegistry.Register<IPresenter>(() => Container.Resolve<TargetsPanelViewModel>());
            //containerRegistry.Register<IPresenter>(() => Container.Resolve<RegionControlUIViewModel>());

            // Register messagebox
            //containerRegistry.RegisterDialog<CMessageBox, MessageBoxViewModel>("standard");

            // Register regions for navigation
            containerRegistry.RegisterForNavigation<RegionHome>();
            containerRegistry.RegisterForNavigation<RegionMotor>();
            containerRegistry.RegisterForNavigation<RegionSettings>();
            containerRegistry.RegisterForNavigation<RegionControlUI>();
            containerRegistry.RegisterForNavigation<RegionSlider>();
            containerRegistry.RegisterForNavigation<RegionControlUIViewModel>();

            //containerRegistry.RegisterSingleton<RegionMotorViewModel>();
        }

        protected override void Initialize() {
            base.Initialize();
        }
    }
}
