using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using AutoMapper;

namespace taskmaker_wpf.Data {
    internal interface IRepository {
        IDataSource DataSource { get; }
        void Add<T>(T item);
        void Update<T>(T item);
        void Delete<T>(T item);
        T Find<T>(string name);
        IEnumerable<T> FindAll<T>();

    }

    public class MotorRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }
        private IMapper _mapper;

        public MotorRepository(IDataSource dataSource, MapperConfiguration config) {
            _mapper = config.CreateMapper();
        }

        public void Add<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Add(_mapper.Map<MotorDTO>(item));
        }

        public void Delete<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Remove(_mapper.Map<MotorDTO>(item));
        }

        public T Find<T>(string name) {
            var src = DataSource as LocalDataSource;

            src.Find<MotorDTO>(name);

            return default(T);
        }

        public IEnumerable<T> FindAll<T>() {
            return null;
        }

        public void Update<T>(T item) {
            var src = DataSource as LocalDataSource;
            var motor = src.Find<MotorDTO>((item as MotorEntity).Name);

        }
    }

    public class MotorDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public double Value { get; set; }
    }

    public class NodeDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point Value { get; set; }
    }

    public class RegionDTO {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimplexRegionDTO : RegionDTO {
        public NodeDTO[] Nodes { get; set; }
        public Point[] Vertices { get; set; }
    }

    public class VoronoiRegionDTO : RegionDTO {
        public Point[] Vertices { get; set; }
        public SimplexRegionDTO[] Governors { get; set; }
    }

    public class ControlUiDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[] Value { get; set; }
        public NodeDTO[] Nodes { get; set; }
        public RegionDTO[] Regions { get; set; }
    }

    public class NLinearMapDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[] Tensor { get; set; }
        public int[] Shape { get; set; }
    }

    public class ControlUiRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }

        public void Add(ControlUiEnity item) {
            var src = DataSource as LocalDataSource;

            src.Add(item);
        }

        public void Update(ControlUiEnity item) {
            var src = DataSource as LocalDataSource;

            src.Update(item);
        }

        public void Delete(ControlUiEnity item) {
            var src = DataSource as LocalDataSource;

            src.Remove(item);
        }

        public ControlUiEnity Find(string name) {
            var src = DataSource as LocalDataSource;

            return src.Find<ControlUiEnity>("ControlUi").First();
        }

        public IEnumerable<ControlUiEnity> FindAll() {
            var src = DataSource as LocalDataSource;

            return src.Find<ControlUiEnity>("ControlUi");
        }

        public void Add<T>(T item) {
            throw new NotImplementedException();
        }

        public void Update<T>(T item) {
            throw new NotImplementedException();
        }

        public void Delete<T>(T item) {
            throw new NotImplementedException();
        }

        public T Find<T>(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> FindAll<T>() {
            throw new NotImplementedException();
        }
    }

    public class NLinearMapRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }

        public void Add(NLinearMapEntity item) {
            throw new NotImplementedException();
        }

        public void Add<T>(T item) {
            throw new NotImplementedException();
        }

        public void Delete(NLinearMapEntity item) {
            throw new NotImplementedException();
        }

        public void Delete<T>(T item) {
            throw new NotImplementedException();
        }

        public NLinearMapEntity Find(string name) {
            throw new NotImplementedException();
        }

        public T Find<T>(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<NLinearMapEntity> FindAll() {
            throw new NotImplementedException();
        }

        public IEnumerable<T> FindAll<T>() {
            throw new NotImplementedException();
        }

        public void Update(NLinearMapEntity item) {
            throw new NotImplementedException();
        }

        public void Update<T>(T item) {
            throw new NotImplementedException();
        }
    }
}
