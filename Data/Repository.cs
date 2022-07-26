using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;

namespace taskmaker_wpf.Data {
    internal interface IRepository<T> {
        IDataSource DataSource { get; }
        void Add(T item);
        void Update(T item);
        void Delete(T item);
        T Find(string name);
        IEnumerable<T> FindAll();

    }

    public class MotorRepository : IRepository<MotorEntity> {
        public IDataSource DataSource {
            get;
            private set;
        }

        public void Add(MotorEntity item) {
            throw new NotImplementedException();
        }

        public void Delete(MotorEntity item) {
            throw new NotImplementedException();
        }

        public MotorEntity Find(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<MotorEntity> FindAll() {
            throw new NotImplementedException();
        }

        public void Update(MotorEntity item) {
            throw new NotImplementedException();
        }
    }

    public class ControlUiRepository : IRepository<ControlUiEnity> {
        public IDataSource DataSource {
            get;
            private set;
        }

        public void AddNode(NodeEntity node) {

        }

        public void AddSimplices(SimplexRegionEntity[] simplices) { }

        public void AddVoronois(VoronoiRegionEntity[] voronois) { }

        public void Add(ControlUiEnity item) {
            throw new NotImplementedException();
        }

        public void Update(ControlUiEnity item) {
            throw new NotImplementedException();
        }

        public void Delete(ControlUiEnity item) {
            throw new NotImplementedException();
        }

        public ControlUiEnity Find(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<ControlUiEnity> FindAll() {
            throw new NotImplementedException();
        }
    }

    public class NLinearMapRepository : IRepository<NLinearMapEntity> {
        public IDataSource DataSource {
            get;
            private set;
        }

        public void Add(NLinearMapEntity item) {
            throw new NotImplementedException();
        }

        public void Delete(NLinearMapEntity item) {
            throw new NotImplementedException();
        }

        public NLinearMapEntity Find(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<NLinearMapEntity> FindAll() {
            throw new NotImplementedException();
        }

        public void Update(NLinearMapEntity item) {
            throw new NotImplementedException();
        }
    }
}
