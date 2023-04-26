using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Views;

namespace taskmaker_wpf.Data {
    public interface IDataSource {
        //T[] Find<T>(string name);

    }

    [Serializable]
    public class XmlObject {
        public List<ControlUiDTO> ControlUis { get; set; } = new List<ControlUiDTO>();
        public List<MotorDTO> Motors { get; set; } = new List<MotorDTO>();
        public List<NLinearMapDTO> Maps { get; set; } = new List<NLinearMapDTO>();

        public XmlObject() { }

        public XmlObject(IEnumerable<ControlUiDTO> controlUis, IEnumerable<MotorDTO> motors, IEnumerable<NLinearMapDTO> maps) {
            ControlUis = controlUis.ToList();
            Motors = motors.ToList();
            Maps = maps.ToList();
        }
    }

    public class LocalDataSource : IDataSource {
        public List<ControlUiDTO> ControlUis { get; set; } = new List<ControlUiDTO>();
        public List<MotorDTO> Motors { get; set; } = new List<MotorDTO>();
        public List<NLinearMapDTO> Maps { get; set; } = new List<NLinearMapDTO>();

        //public RegionDTO[][] Regions => ControlUis.Select(e => e.Regions).ToArray();


        private XmlObject _xmlObject;
        private IEventAggregator _ea;

        public LocalDataSource() { }

        public void BindEventAggregator(IEventAggregator ea) {
            _ea = ea;
            _ea.GetEvent<SystemSaveEvent>().Subscribe(
                () => Save()
                );
        }

        private int _counter = 0;

        private void Count() {
            if (_counter <= 10) {
                _counter++;
            }
            else {
                _counter = 0;

                Task.Run(Save);
                //Save();
            }
        }

        // TODO: better loading method by DI
        public static LocalDataSource Load(IEventAggregator ea) {
            var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.json");
            var xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");

            //if (!File.Exists(docPath)) {
            //    var dataSource = new LocalDataSource();
            //    dataSource.BindEventAggregator(ea);
            //    return dataSource;
            //}

            //using (var fs = File.OpenRead(docPath)) {
            //    using (var r = new StreamReader(fs, System.Text.Encoding.UTF8)) {
            //        var jsonStr = r.ReadToEnd();
            //        var local = JsonSerializer.Deserialize<LocalDataSource>(jsonStr);

            //        local.BindEventAggregator(ea);

            //        return local;
            //    }

            //}
            var xml = new System.Xml.Serialization.XmlSerializer(typeof(XmlObject));

            if (!File.Exists(xmlPath)) {
                var dataSource = new LocalDataSource();
                dataSource.BindEventAggregator(ea);
                return dataSource;
            }

            using (var fs = File.OpenRead(xmlPath)) {
                var xmlObject = (XmlObject)xml.Deserialize(fs);
                var local = new LocalDataSource();

                local.BindEventAggregator(ea);
                local.ControlUis.AddRange(xmlObject.ControlUis);
                local.Motors.AddRange(xmlObject.Motors);
                local.Maps.AddRange(xmlObject.Maps);

                return local;
            }
        }

        private async void Save() {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker"));

            var options = new JsonSerializerOptions {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                WriteIndented = true,
            };
            var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.json");
            var xmlFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");

            var xml = new System.Xml.Serialization.XmlSerializer(typeof(XmlObject));

            //using (var fs = File.Create(docPath)) {
            //    using (var w = new StreamWriter(fs, System.Text.Encoding.UTF8)) {
            //        var text = JsonSerializer.Serialize(this, GetType(), options);
            //        await w.WriteAsync(text);
            //    }
            //}

            using (var xmlfs = File.Create(xmlFilePath)) {
                xml.Serialize(xmlfs, new XmlObject(ControlUis, Motors, Maps));
            }
        }

        public T Find<T>(int id) {
            if (typeof(T) == typeof(ControlUiDTO)) {
                return (T)(object)ControlUis.Find(e => e.Id == id);
            }
            else if (typeof(T) == typeof(MotorDTO)) {
                return (T)(object)Motors.Find(e => e.Id == id);
            }
            else if (typeof(T) == typeof(NLinearMapDTO)) {
                return (T)(object)Maps.Find(e => e.Id == id);
            }
            else {
                return default(T);
            }
        }

        public T[] FindAllOfType<T>() {
            if (typeof(T) == typeof(ControlUiDTO)) {
                return ControlUis.Cast<T>().ToArray();
            }
            else if (typeof(T) == typeof(MotorDTO)) {
                return Motors.Cast<T>().ToArray(); ;
            }
            else if (typeof(T) == typeof(NLinearMapDTO)) {
                return Maps.Cast<T>().ToArray();
            }
            else {
                return new T[0];
            }
        }

        public T Add<T>(T item) {
            if (typeof(T) == typeof(MotorDTO)) {
                var motor = item as MotorDTO;
                motor.Id = Motors.Count != 0? Motors.Last().Id + 1: 0;

                Motors.Add(motor);

                Count();
                return (T)(object)motor;
            }
            else if (typeof(T) == typeof(ControlUiDTO)) {
                var ui = item as ControlUiDTO;
                ui.Id = ControlUis.Count != 0? ControlUis.Last().Id + 1: 0;

                ControlUis.Add(ui);

                Count();

                return (T)(object)ui;
            }
            else if (typeof(T) == typeof(NLinearMapDTO)) {
                var map = item as NLinearMapDTO;
                map.Id = Maps.Count != 0? Maps.Last().Id + 1: 0;

                Maps.Add(map);

                Count();

                return (T)(object)map;
            }


            return default;
        }

        public void Remove<T>(T item) {
            if (typeof(T) == typeof(MotorDTO)) {
                var motor = (item as MotorDTO);
                var target = Motors.Where(e => e.Id == motor.Id).First();

                Motors.Remove(target);
            }

            Count();
        }

        public void Update<T>(T item) {
            if (typeof(T) == typeof(MotorDTO)) {
                var idx = Motors.FindIndex(e => e.Id == (item as MotorDTO).Id);

                Motors[idx] = item as MotorDTO;
            }
            else if (typeof(T) == typeof(ControlUiDTO)) {
                var idx = ControlUis.FindIndex(e => e.Id == (item as ControlUiDTO).Id);

                ControlUis[idx] = item as ControlUiDTO;
            }
            else if (typeof(T) == typeof(NLinearMapDTO)) {
                var idx = Maps.FindIndex(e => e.Id == (item as NLinearMapDTO).Id);

                Maps[idx] = item as NLinearMapDTO;
            }

            Count();
        }
    }
}
