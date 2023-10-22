using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public struct MotorState {
        private int id;
        private string name;

        private double value;

        private double max;
        private double min;
        private int nuiBoardId;
        private int nuiMotorId;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public double Value { get => value; set => this.value = value; }
        public double Max { get => max; set => max = value; }
        public double Min { get => min; set => min = value; }
        public int NuiBoardId { get => nuiBoardId; set => nuiBoardId = value; }
        public int NuiMotorId { get => nuiMotorId; set => nuiMotorId = value; }
    }

    public partial class MotorViewModel : ObservableObject {
        [ObservableProperty]
        private int _id;
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private int _nuiBoardId;
        [ObservableProperty]
        private int _nuiMotorId;

        [ObservableProperty]
        private double _value;

        partial void OnValueChanged(double value) {
            var state = ToState();

            motorService.ToSerial(state);
        }

        [ObservableProperty]
        private double _max;
        [ObservableProperty]
        private double _min;

        private readonly MotorService motorService;

        public MotorViewModel(MotorService motorService) {
            this.motorService = motorService;
        }

        [RelayCommand]
        public void Commit() {
            var state = ToState();
            var entity = motorService.Read(state.Id);

            entity.Id = state.Id;
            entity.Name = state.Name;
            entity.NuiBoardId = state.NuiBoardId;
            entity.NuiMotorId = state.NuiMotorId;
            entity.Value = state.Value;
            entity.Max = state.Max;
            entity.Min = state.Min;

            motorService.Update(entity);
        }

        public MotorState ToState() {
            return new MotorState {
                Id = Id,
                Name = Name,
                NuiBoardId = NuiBoardId,
                NuiMotorId = NuiMotorId,
                Value = Value,
                Max = Max,
                Min = Min,
            };
        }
    }


    public partial class MotorCollectionViewModel : ObservableObject {
        public ObservableCollection<MotorViewModel> MotorVMs { get; private set; } = new();

        protected readonly MotorService motorService;

        public MotorCollectionViewModel(MotorService motorService) {
            this.motorService = motorService;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                MotorVMs = new ObservableCollection<MotorViewModel>
                {
                    new MotorViewModel(motorService),
                    new MotorViewModel(motorService),
                    new MotorViewModel(motorService),
                    new MotorViewModel(motorService),
                    new MotorViewModel(motorService),
                    new MotorViewModel(motorService),
                };
            }
            else {
                // Your runtime initialization
                Fetch();
            }
        }

        protected void Fetch() {
            motorService.GetAll().ToList().ForEach(entity => {
                var motorVM = new MotorViewModel(motorService) {
                    Id = entity.Id,
                    Name = entity.Name,
                    NuiBoardId = entity.NuiBoardId,
                    NuiMotorId = entity.NuiMotorId,
                    Value = entity.Value,
                    Max = entity.Max,
                    Min = entity.Min
                };

                MotorVMs.Add(motorVM);
            });
        }

        [RelayCommand]
        public void Add() {
            var entity = motorService.Create(new MotorEntity() { Name = "New Motor" });

            var motorVM = new MotorViewModel(motorService) {
                Id = entity.Id,
                Name = entity.Name,
                NuiBoardId = entity.NuiBoardId,
                NuiMotorId = entity.NuiMotorId,
                Value = entity.Value,
                Max = entity.Max,
                Min = entity.Min
            };

            MotorVMs.Add(motorVM);
        }

        [RelayCommand]
        public void Delete(MotorState state) {
            motorService.Delete(state.Id);

            var motorVM = MotorVMs.FirstOrDefault(vm => vm.Id == state.Id);

            if (motorVM != null)
                MotorVMs.Remove(motorVM);
        }

        [RelayCommand]
        public void Commit() {
            foreach (var motorVM in MotorVMs) {
                motorVM.Commit();
            }
        }
    }
}