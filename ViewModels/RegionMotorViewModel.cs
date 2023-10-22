using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Services;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.ViewModels {
    public partial class RegionMotorViewModel : MotorCollectionViewModel, INavigationAware {
        private readonly EvaluationService evaluationService;

        public RegionMotorViewModel(EvaluationService evaluationService, MotorService motorService): base(motorService) {
            this.evaluationService = evaluationService;
        }


        [RelayCommand]
        public async Task Initialize() {
            evaluationService.Initialize();

            var result = await WeakReferenceMessenger.Default.Send(new ShowMessageBoxMessage {
                Message = "Initialized all motors for experiments!",
                Caption = "Initialize",
                Button = MessageBoxButton.OK,
                Icon = MessageBoxImage.Information,
            });

            if (result == MessageBoxResult.OK) {
                //InvalidateMotors();
            }

            Fetch();
        }

        [ObservableProperty]
        private Brush[] _colors = new Brush[] {
            Brushes.Black,
            Brushes.Red,
            Brushes.Green,
            Brushes.Blue
        };

        public RegionMotorViewModel(MotorService motorService) : base(motorService) { }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
