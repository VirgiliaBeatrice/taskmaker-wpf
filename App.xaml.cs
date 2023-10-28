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
            // Register services
            containerRegistry.RegisterSingleton<SerialService>();
            containerRegistry.RegisterSingleton<EvaluationService>();
            containerRegistry.RegisterSingleton<MotorService>();
            containerRegistry.RegisterSingleton<SurveyService>();
            containerRegistry.RegisterSingleton<UIService>();


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
            containerRegistry.Register<SurveyPageViewModel>();

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
            containerRegistry.RegisterForNavigation<SurveyPage>();

            //containerRegistry.RegisterSingleton<RegionMotorViewModel>();
        }

        protected override void Initialize() {
            base.Initialize();
        }
    }
}
