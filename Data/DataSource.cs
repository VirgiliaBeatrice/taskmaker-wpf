using System;
using System.Collections.Generic;
using System.Linq;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;

namespace taskmaker_wpf.Data {
    public interface IDataSource {
        T[] Find<T>(string name);

    }

    //public class MotorDataSource : IDataSource {
    
    //}

    public class FileSystemDataSource : IDataSource {
        private List<ControlUi> ControlUis { get; set; } = new List<ControlUi>();
        private List<Motor> Motors { get; set; } = new List<Motor>();
        private List<NLinearMap> Maps { get; set; } = new List<NLinearMap>();

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

    }
}
