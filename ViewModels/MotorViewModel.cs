using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using taskmaker_wpf.Entity;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {


    //public struct MotorState {
    //    private int id;
    //    private string name;

    //    private double value;

    //    private double max;
    //    private double min;
    //    private int nuiBoardId;
    //    private int nuiMotorId;

    //    public int Id { get => id; set => id = value; }
    //    public string Name { get => name; set => name = value; }
    //    public double Value { get => value; set => this.value = value; }
    //    public double Max { get => max; set => max = value; }
    //    public double Min { get => min; set => min = value; }
    //    public int NuiBoardId { get => nuiBoardId; set => nuiBoardId = value; }
    //    public int NuiMotorId { get => nuiMotorId; set => nuiMotorId = value; }
    //}

    public class MotorValueUpdatedMessage {
        public int NuiBoardId { get; init; }
        public int NuiMotorId { get; init; }
        public double Value { get; init; }
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


        partial void OnValueChanged(double oldValue, double newValue) {
            // TODO: Should be in a entity
            if (Math.Abs(newValue - oldValue) >= 1) {
                _entity.Value = newValue;

                WeakReferenceMessenger.Default.Send(new MotorValueUpdatedMessage {
                NuiBoardId = NuiBoardId,
                NuiMotorId = NuiMotorId,
                Value = Value
            });
        }
    }

        [ObservableProperty]
        private Brush _color;
        [ObservableProperty]
        private double _max;
        [ObservableProperty]
        private double _min;

        private readonly MotorEntity _entity;

        public MotorViewModel(MotorEntity entity) {
            _entity = entity;

            Fetch();
        }

        [RelayCommand]
        public void Fetch() {
            // update all properties from entity to this
            Id = _entity.Id;
            Name = _entity.Name;
            NuiBoardId = _entity.NuiBoardId;
            NuiMotorId = _entity.NuiMotorId;
            Value = _entity.Value;
            Max = _entity.Max;
            Min = _entity.Min;
            Color = _entity.Color;
        }

        [RelayCommand]
        public void Update() {
            _entity.Id = Id;
            _entity.Name = Name;
            _entity.NuiBoardId = NuiBoardId;
            _entity.NuiMotorId = NuiMotorId;
            _entity.Value = Value;
            _entity.Max = Max;
            _entity.Min = Min;
            _entity.Color = Color;

            Fetch();
        }
    }


    public partial class MotorCollectionViewModel : ObservableObject {
        public ObservableCollection<MotorViewModel> Motors { get; private set; } = new();

        protected readonly MotorService _motorSrv;

        public MotorCollectionViewModel(MotorService motorService) {
            _motorSrv = motorService;

            Fetch();
        }

        [RelayCommand]
        public virtual void Fetch() {
            Motors.Clear();

            _motorSrv.GetAll().ToList().ForEach(entity => {
                var motorVM = new MotorViewModel(entity);

                Motors.Add(motorVM);
            });
        }

        [RelayCommand]
        public void Add() {
            var entity = _motorSrv.Create(new MotorEntity() { Name = "New Motor" });
            var motorVM = new MotorViewModel(entity);
            Motors.Add(motorVM);
        }

        [RelayCommand]
        public void Delete(int id) {
            _motorSrv.Delete(id);

            var motorVM = Motors.FirstOrDefault(vm => vm.Id == id);

            if (motorVM != null)
                Motors.Remove(motorVM);
        }

        [RelayCommand]
        public void Update() {
            foreach (var motorVM in Motors) {
                motorVM.Update();
            }
        }
    }
}