﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using taskmaker_wpf.Services;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Views;
using CMessageBox = taskmaker_wpf.Views.MessageBox;  

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
            containerRegistry.RegisterForNavigation<RegionMotor>();
            containerRegistry.RegisterForNavigation<RegionSettings>();

            // Register services
            containerRegistry.RegisterSingleton<MotorService>();
            containerRegistry.RegisterSingleton<SerialService>();

            // Register messagebox
            containerRegistry.RegisterDialog<CMessageBox, MessageBoxViewModel>("standard");
        }
    }
}
