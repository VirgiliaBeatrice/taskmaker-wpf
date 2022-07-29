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

            // Register services
            containerRegistry.RegisterSingleton<SerialService>();
            containerRegistry.RegisterSingleton<SystemService>();

            // Register model agent
            //containerRegistry.Register<MotorAgent>();



            containerRegistry.RegisterSingleton<MapperConfiguration>(
                () => {
                    var config = new MapperConfiguration(
                        cfg => {
                            cfg.CreateMap<NodeEntity, NodeState>().ReverseMap();
                            cfg.CreateMap<NodeEntity, NodeDTO>(MemberList.Destination)
                            .ReverseMap();

                            cfg.CreateMap<SimplexRegionEntity, SimplexState>().ReverseMap();
                            cfg.CreateMap<SimplexRegionEntity, SimplexRegionDTO>().ReverseMap();

                            cfg.CreateMap<VoronoiRegionEntity, VoronoiState>().ReverseMap();
                            cfg.CreateMap<VoronoiRegionEntity, VoronoiRegionDTO>().ReverseMap();

                            cfg.CreateMap<ControlUiEntity, ControlUiDTO>()
                            .ForMember(d => d.Nodes, o => o.MapFrom(s => s.Nodes))
                            .ReverseMap();
                            cfg.CreateMap<MotorEntity, MotorDTO>().ReverseMap();
                            cfg.CreateMap<NLinearMapEntity, NLinearMapDTO>().ReverseMap();

                            cfg.CreateMap<MotorEntity, MotorState >().ReverseMap();
                        });

                    //config.AssertConfigurationIsValid();
                    return config;
            });

            // Register IDataSource
            containerRegistry.RegisterSingleton<IDataSource>(() => {
                return LocalDataSource.Load();
            });

            // Register IRepository
            containerRegistry.RegisterSingleton<IRepository, MotorRepository>("1");
            containerRegistry.Register<IRepository, ControlUiRepository>("2");
            containerRegistry.Register<IRepository, NLinearMapRepository>("3");

            // Register IUseCase
            containerRegistry.RegisterSingleton<IUseCase, MotorUseCase>("1");
            containerRegistry.RegisterSingleton<IUseCase, ControlUiUseCase>("2");
            containerRegistry.RegisterSingleton<IUseCase, NLinearMapUseCase>("3");
            containerRegistry.RegisterSingleton<IUseCase, ListTargetUseCase>("ListTargetUseCase");

            // Register messagebox
            containerRegistry.RegisterDialog<CMessageBox, MessageBoxViewModel>("standard");

            // Register regions for navigation
            containerRegistry.RegisterForNavigation<RegionHome>();
            containerRegistry.RegisterForNavigation<RegionMotor>();
            containerRegistry.RegisterForNavigation<RegionSettings>();
            containerRegistry.RegisterForNavigation<RegionControlUI>();
            containerRegistry.RegisterForNavigation<RegionControlUISelection>();

        }

        protected override void Initialize() {
            base.Initialize();
        }
    }
}
