using System.Collections.Generic;
using System.Linq;
using taskmaker_wpf.Model.Data;
using System.Collections.ObjectModel;
using taskmaker_wpf.Models;
using Prism.Mvvm;
using System.Collections.Specialized;

namespace taskmaker_wpf.Services {
    public class SystemService : BindableBase {
        //public ObservableCollection<NLinearMap> Maps { get; set; } = new ObservableCollection<NLinearMap>();
        //public ObservableCollection<ComplexM> Complexes { get; set; } = new ObservableCollection<ComplexM>();


        private ObservableCollection<Motor> _motors = new ObservableCollection<Motor>();
        public ObservableCollection<Motor> Motors {
            get => _motors;
            set => SetProperty(ref _motors, value);
        }

        private ObservableCollection<ControlUI> _UIs = new ObservableCollection<ControlUI>();
        public ObservableCollection<ControlUI> UIs {
            get => _UIs;
            set => SetProperty(ref _UIs, value);
        }

        private ISelectableTarget[] _targets;
        public ISelectableTarget[] Targets {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }


        public SystemService() {
            Enumerable.Range(0, 3)
                .ToList()
                .ForEach(
                    e => {
                        var motor = new Motor() {
                            Max = 10000,
                            Min = -10000,
                            Value = 0,
                            Name = $"S{e}"
                        };

                        Motors.Add(motor);
                    });


            UIs.CollectionChanged += OnCollectionChanged;
            Motors.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            var merged = new List<ISelectableTarget>();

            merged.AddRange(Motors);
            merged.AddRange(UIs);

            Targets = merged.ToArray();
        }
    }
}
