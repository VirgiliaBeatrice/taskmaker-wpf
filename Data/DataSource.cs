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
        private List<ControlUiDTO> ControlUis { get; set; } = new List<ControlUiDTO>();
        private List<MotorDTO> Motors { get; set; } = new List<MotorDTO>();
        private List<NLinearMapDTO> Maps { get; set; } = new List<NLinearMapDTO>();

        private object DB { get; set; }
        private void Initialize() {

        }

        private void Load() {
            var docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.json");

            using (var fs = File.OpenRead(docPath)) {
                using (var r = new StreamReader(fs, System.Text.Encoding.UTF8)) {
                    while(!r.EndOfStream) {
                        var db = JsonSerializer.Deserialize<LocalDataSource>(r.ReadLine());

                        ControlUis = db.ControlUis;
                        Motors = db.Motors;
                        Maps = db.Maps;
                    }
                }
            }
        }

        private async void Save() {
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

        public T[] Find<T>(string name) {
            if (name is null) {
                if (default(T) is ControlUi) {
                    return ControlUis.Where(e => e.Name == name).Cast<T>().ToArray();
                }
                else if (default(T) is Motor) {
                    return Motors.Where(e => e.Name == name).Cast<T>().ToArray(); ;
                }
                else {
                    return new T[0];
                }
            }
            else {
                return new T[0];
            }

        }

        public void Add<T>(T item) {
            if (default(T) is MotorDTO) {
                Motors.Add(item as MotorDTO);
            }

            Save();
        }

        public void Remove<T>(T item) {
            if (default(T) is MotorDTO) {
                Motors.Remove(item as MotorDTO);
            }
            Save();
        }

        public void Update<T>(T item) {
            if (default(T) is ControlUi ui) {
                // ui
            }
            Save();
        }
    }
}
