using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;

namespace taskmaker_wpf.Data {
    internal interface IRepository {
        IDataSource DataSource { get; }
        void SaveChanges();

    }

    public class MotorRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }

        public Motor[] GetMotors() {
            return DataSource.Find<Motor>(null);
        }

        public async void SaveChanges() {

        }
    }

    public class ControlUiRepository {
        public IDataSource DataSource {
            get;
            private set;
        }

        public ControlUi[] GetControlUis() {
            return DataSource.Find<ControlUi>(null); 
        }

        public void AddNode(NodeM node) {

        }

        public void Build(IEnumerable<IRegion> regions) {

        }

        public ControlUiEnity FindByName() {
            return new ControlUiEnity();
        }

    }

    public class NLinearMapRepository {
        public IDataSource DataSource {
            get;
            private set;
        }

        public NLinearMap[] GetMaps() {
            return DataSource.Find<NLinearMap>(null);
        }
    }
}
