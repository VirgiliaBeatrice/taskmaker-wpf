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
            // Register regions for navigation
            containerRegistry.RegisterForNavigation<RegionHome>();
            containerRegistry.RegisterForNavigation<RegionMotor>();
            containerRegistry.RegisterForNavigation<RegionSettings>();
            containerRegistry.RegisterForNavigation<RegionControlUI>();
            containerRegistry.RegisterForNavigation<RegionControlUISelection>();


            // Register services
            containerRegistry.RegisterSingleton<SerialService>();
            containerRegistry.RegisterSingleton<SystemService>();

            // Register model agent
            //containerRegistry.Register<MotorAgent>();

            containerRegistry.RegisterSingleton<MapperConfiguration>(() => {
                var config = new MapperConfiguration(
                    cfg => {
                        cfg.CreateMap<ControlUiDTO, ControlUiEnity>();
                        cfg.CreateMap<MotorDTO, MotorEntity>();
                        cfg.CreateMap<NLinearMapDTO, NLinearMapEntity>();

                        cfg.CreateMap<StatefulMotor, MotorEntity>();
                    });

                return config;
            });

            // Register IRepository
            containerRegistry.RegisterSingleton<IRepository, MotorRepository>("Motor");
            //containerRegistry.Register<IRepository, ControlUiRepository>("ControlUi");

            // Register messagebox
            containerRegistry.RegisterDialog<CMessageBox, MessageBoxViewModel>("standard");
        }

        protected override void Initialize() {
            base.Initialize();
        }
    }
}
