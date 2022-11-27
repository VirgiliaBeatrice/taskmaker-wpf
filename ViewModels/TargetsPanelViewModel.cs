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


    public struct OutputPort {
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

    public struct InputPort {
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
