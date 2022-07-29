using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;

namespace taskmaker_wpf.Data {
    public interface IDataSource {
        //T[] Find<T>(string name);

    }

    public class LocalDataSource : IDataSource {
        public List<ControlUiDTO> ControlUis { get; set; } = new List<ControlUiDTO>();
        public List<MotorDTO> Motors { get; set; } = new List<MotorDTO>();
        public List<NLinearMapDTO> Maps { get; set; } = new List<NLinearMapDTO>();

        public LocalDataSource() {
            //Motors.Add(new MotorDTO {
            //    Id = 1,
            //    Name = "S1"
            //});
            //Load();
        }

        private int _counter = 0;

        private void Count() {
            if (_counter <= 10) {
                _counter++;
            }
            else {
                _counter = 0;

                Save();
            }
        }

        public static LocalDataSource Load() {
            var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.json");

            if (!File.Exists(docPath)) {
                return new LocalDataSource();
            }

            using (var fs = File.OpenRead(docPath)) {
                using (var r = new StreamReader(fs, System.Text.Encoding.UTF8)) {
                    var jsonStr = r.ReadToEnd();

                    return JsonSerializer.Deserialize<LocalDataSource>(jsonStr);
                }
            }
        }

        private async void Save() {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker"));

            var options = new JsonSerializerOptions {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };
            var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.json");

            using (var fs = File.Create(docPath)) {
                using (var w = new StreamWriter(fs, System.Text.Encoding.UTF8)) {
                    var text = JsonSerializer.Serialize(this, options);

                    await w.WriteAsync(text);
                }
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
