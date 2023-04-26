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

    public interface IOutputPortState {
        int Id { get; set; }
        string Name { get; set; }
    }
    public interface IInputPortState {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class DisplayInputPort {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public DisplayInputPort(IInputPortState input) {
            Name = input.Name;
        }
    }

    public class DisplayOutputPort {
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;

        public DisplayOutputPort(IOutputPortState output) {
            Name = output.Name;
        }
    }


    public struct OutPlug {
        public string Name { get; set; }
        public int Dimension { get; set; }
        public int Id { get; set; }

        public static OutPlug Create(MotorEntity entity) {
            return new OutPlug {
                Name = entity.Name,
                Dimension = 1,
                Id = entity.Id
            };
        }

        public static OutPlug Create(ControlUiEntity entity) {
            return new OutPlug {
                Name = entity.Name,
                Dimension = 2,
                Id = entity.Id
            };
        }

        public static OutPlug Create(IOutputPort entity) {
            if (entity is MotorEntity motor) {
                return Create(motor);
            }
            else if (entity is ControlUiEntity ui) {
                return Create(ui);
            }
            else return default;
        }
    }

    public struct InPlug {
        public string Name { get; set; }
        public int BasisCount { get; set; }
        public int Id { get; set; }

        public static InPlug Create(ControlUiEntity entity) {
            return new InPlug {
                Id = entity.Id,
                Name = entity.Name,
                BasisCount = entity.Nodes.Length
            };
        }
    }

}
