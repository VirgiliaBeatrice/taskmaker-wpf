using System;
using System.Collections.Generic;
using System.Windows;
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
        T Find<T>(int id);
        IEnumerable<T> FindAll<T>();

    }

    public class MotorRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }
        private IMapper _mapper;

        public MotorRepository(IDataSource dataSource, MapperConfiguration config) {
            DataSource = dataSource;
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

        public T Find<T>(int id) {
            var src = DataSource as LocalDataSource;

            src.Find<MotorDTO>(id);

            return default(T);
        }

        public IEnumerable<T> FindAll<T>() {
            var src = DataSource as LocalDataSource;

            var dto = src.FindAllOfType<MotorDTO>().FirstOrDefault();
            var entity = _mapper.Map<MotorEntity>(dto);
            return src.FindAllOfType<MotorDTO>().Select(e => _mapper.Map<MotorEntity>(e)).Cast<T>();
        }

        public void Update<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Update(_mapper.Map<MotorDTO>(item as MotorEntity));
        }
    }

    public class MotorDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public double Value { get; set; }
        public int BoardId { get; set; }
        public int MotorId { get; set; }
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
        public object[] Targets { get; set; }
    }

    public class ControlUiRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }
        private IMapper _mapper;

        public ControlUiRepository(IDataSource dataSource, MapperConfiguration config) {
            DataSource = dataSource;
            _mapper = config.CreateMapper();
        }

        public void Add<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Add(_mapper.Map<ControlUiDTO>(item));
        }

        public void Update<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Update(_mapper.Map<ControlUiDTO>(item));
        }

        public void Delete<T>(T item) {
            throw new NotImplementedException();
        }

        public T Find<T>(int id) {
            var src = DataSource as LocalDataSource;

            return (T)(object)_mapper.Map<ControlUiEntity>(src.Find<ControlUiDTO>(id));
        }

        public IEnumerable<T> FindAll<T>() {
            var src = DataSource as LocalDataSource;

            return src.FindAllOfType<ControlUiDTO>().Select(e => _mapper.Map<ControlUiEntity>(e)).Cast<T>();
        }
    }

    public class NLinearMapRepository : IRepository {
        public IDataSource DataSource {
            get;
            private set;
        }
        private IMapper _mapper;

        public NLinearMapRepository(IDataSource dataSource, MapperConfiguration config) {
            DataSource = dataSource;
            _mapper = config.CreateMapper();
        }

        public void Add<T>(T item) {
            var src = DataSource as LocalDataSource;
            var dto = src.Add(_mapper.Map<NLinearMapDTO>(item));
        }

        public void Delete<T>(T item) {
            throw new NotImplementedException();
        }

        public T Find<T>(int id) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> FindAll<T>() {
            var src = DataSource as LocalDataSource;

            return src.FindAllOfType<NLinearMapDTO>().Cast<T>();
        }
        public void Update<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Update(_mapper.Map<NLinearMapDTO>(item));
        }
    }
}
