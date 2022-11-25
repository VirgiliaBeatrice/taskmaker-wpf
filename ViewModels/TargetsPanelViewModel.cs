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

    public interface IOutputPort : ICloneable {
        int Id { get; set; }
        string Name { get; set; }
    }
    public interface IInputPort : ICloneable {
        string Name { get; set; }
    }

    public class OutputPort {
        public bool IsSelected { get; set; } = false;
        public string Name { get; set; }
        public int Dimension { get; set; }
        public Type PortType { get; set; }
        public int PortId { get; set; }

        public OutputPort(IOutputPort reference) {
            if (reference is MotorState_v1) {
                Dimension = 1;
            }
            else if (reference is ControlUiState_v1) {
                Dimension = 2;
            }

            Name = reference.Name;

            PortType = reference.GetType();
            PortId = reference.Id;
        }
    }

    public class InputPort {
        public object Reference { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public int BasisCount { get; set; }

        public InputPort(IInputPort reference) {
            Reference = reference;

            Name = reference.Name;
            BasisCount = (reference as ControlUiState_v1).Nodes.Length;
        }
    }

}
