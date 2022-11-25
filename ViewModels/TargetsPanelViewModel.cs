using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using taskmaker_wpf.Services;
using AutoMapper;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using Prism.Events;
using SkiaSharp;

namespace taskmaker_wpf.ViewModels {
    public interface ISelectableState {
        int Id { get; }
        bool IsSelected { get; set; }
        string Name { get; }
        double[] Value { get; }
    }

    public class ControlUiTargetState : ControlUiState, IInputPort, IOutputPort {
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public override string ToString() {
            return Name;
        }

        public object Clone() {
            return (ControlUiTargetState)MemberwiseClone();
        }
    }

    public class MotorTargetState : MotorState, IOutputPort {
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public object Clone() {
            return (MotorTargetState)MemberwiseClone();
        }

        public override string ToString() {
            return Name;
        }
    }
    public class NLinearMapState : BindableBase {
        private int _id;
        private string _name;

        private IInputPort[] _inputs;
        private IOutputPort[] _outputs;

        public int Id {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public IInputPort[] Inputs {
            get => _inputs;
            set => SetProperty(ref _inputs, value);
        }
        public IOutputPort[] Outputs { get => _outputs; set => SetProperty(ref _outputs, value); }

        public override string ToString() => Name;
    }

    public interface IOutputPort {
        int Id { get; set; }
        string Name { get; set; }
    }
    public interface IInputPort {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class DisplayInputPort {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public DisplayInputPort(IInputPort input) {
            Name = input.Name;
        }
    }

    public class DisplayOutputPort {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public DisplayOutputPort(IOutputPort output) {
            Name = output.Name;
        }
    }


    public class OutputPort {
        public string Name { get; set; }
        public int Dimension { get; set; }
        public int Id { get; set; }

        public static OutputPort Create(MotorEntity entity) {
            return new OutputPort {
                Name = entity.Name,
                Dimension = 1,
                Id = entity.Id
            };
        }

        public static OutputPort Create(ControlUiEntity entity) {
            return new OutputPort {
                Name = entity.Name,
                Dimension = 2,
                Id = entity.Id
            };
        }

        public static OutputPort Create(IOutputPort entity) {
            if (entity is MotorEntity motor) {
                return Create(motor);
            }
            else if (entity is ControlUiEntity control) {
                return Create(control);
            }
            else {
                return default;
            }
        }
    }

    public class InputPort {
        public string Name { get; set; }
        public int BasisCount { get; set; }


        public static InputPort Create(IInputPort entity) {

            return new InputPort {
                Name = entity.Name,
                BasisCount = (entity as ControlUiEntity).Nodes.Length
            };
        }
    }

}
