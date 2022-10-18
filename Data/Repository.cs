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
using taskmaker_wpf.Services;
using System.Xml.Serialization;
using System.IO;

namespace taskmaker_wpf.Data {

    // Hold DTO, depend on Entity
    public interface IRepository {
        IDataSource DataSource { get; }
        void Add<T>(T item);
        void Update<T>(T item);
        void Delete<T>(T item);
        void Save();
        void Load(string filename);
        T Find<T>(int id); 
        IEnumerable<T> FindAll<T>();

    }

    public class ProjectDataObject {
        public List<MotorEntity> Motors { get; set; } = new List<MotorEntity>();
        public List<ControlUiEntity> Uis { get; set; } = new List<ControlUiEntity>();
        public List<NLinearMapEntity> Entities { get; set; } = new List<NLinearMapEntity>();

    }

    public class ProjectRepository : IRepository {
        public IDataSource DataSource { get; set;}
        public ProjectDataObject Project { get; set; } = new ProjectDataObject();

        private List<MotorEntity> Motors => Project.Motors;

        public void Add<T>(T item) {
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                Project.Motors.Add(item as MotorEntity);
            }
        }

        // TODO: Clone
        public void Update<T>(T item) {
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                var entity = item as MotorEntity;
                var target = Project.Motors.Find(e => e.Id == entity.Id);

                if (target != null)
                    target = entity;
            }
        }

        public void Delete<T>(T item) {
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                var entity = item as MotorEntity;

                Project.Motors.Remove(Project.Motors.Find(e => e.Id == entity.Id));
            }
        }

        public T Find<T>(int id){
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                var target = Project.Motors.Find(e => e.Id == id);

                return (T)(object)target;
            }

            return default(T);
        }

        public IEnumerable<T> FindAll<T>() {
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                return Project.Motors.Cast<T>().ToArray();
            }

            return new[] { default(T) };
        }

        public void Save() {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker"));

            var xmlFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");
            var xml = new XmlSerializer(typeof(ProjectDataObject));

            using (var xmlfs = File.Create(xmlFilePath)) {
                xml.Serialize(xmlfs, Project);
            }
        }

        public void Load(string filename) {
            var xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");

            var xml = new XmlSerializer(typeof(ProjectDataObject));

            if (!File.Exists(xmlPath)) {
                //var dataSource = new LocalDataSource();
                //dataSource.BindEventAggregator(ea);
                //return dataSource;
            }

            using (var fs = File.OpenRead(xmlPath)) {
                var xmlObject = (ProjectDataObject)xml.Deserialize(fs);
                Project = xmlObject;
                //var local = new LocalDataSource();

                //local.BindEventAggregator(ea);
                //local.ControlUis.AddRange(xmlObject.ControlUis);
                //local.Motors.AddRange(xmlObject.Motors);
                //local.Maps.AddRange(xmlObject.Maps);

                //return local;
            }
        }
    }

    public class MotorRepository {
        public IDataSource DataSource {
            get;
            private set;
        }
        private IMapper _mapper;
        private readonly SerialService _serial;

        public MotorRepository(IDataSource dataSource, MapperConfiguration config, SerialService serial) {
            DataSource = dataSource;
            _mapper = config.CreateMapper();
            _serial = serial;
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
            var motor = item as MotorEntity;

            src.Update(_mapper.Map<MotorDTO>(motor));
            _serial.Update(motor.BoardId, motor.MotorId, motor.Value[0]);

            //
             _serial.SendToNuibot();
        }
    }

    [Serializable]
    public class MotorDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public double[] Value { get; set; }
        public int BoardId { get; set; }
        public int MotorId { get; set; }
    }

    [Serializable]
    public class NodeDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point Value { get; set; }
    }

    [XmlInclude(typeof(SimplexRegionDTO))]
    [XmlInclude(typeof(VoronoiRegionDTO))]
    [Serializable]
    public abstract class RegionDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public abstract string Type { get; }
    }

    [Serializable]
    public class SimplexRegionDTO : RegionDTO {
        public NodeDTO[] Nodes { get; set; }
        public Point[] Vertices { get; set; }

        public override string Type { get; } = nameof(SimplexRegionDTO);
    }

    [Serializable]
    public class VoronoiRegionDTO : RegionDTO {
        public override string Type { get; } = nameof(VoronoiRegionDTO);
        public Point[] Vertices { get; set; }
        public SimplexRegionDTO[] Governors { get; set; }
    }

    [Serializable]
    public class ControlUiDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[] Value { get; set; }
        public NodeDTO[] Nodes { get; set; }
        public RegionDTO[] Regions { get; set; }
    }

    [Serializable]
    public class NLinearMapDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[] Tensor { get; set; }
        public int[] Shape { get; set; }
        public string[] Targets { get; set; }
    }

    public class ControlUiRepository{
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

    public class NLinearMapRepository {
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
            var src = DataSource as LocalDataSource;

            return (T)(object)_mapper.Map<NLinearMapEntity>(src.Find<NLinearMapDTO>(id));
        }

        public IEnumerable<T> FindAll<T>() {
            var src = DataSource as LocalDataSource;

            return _mapper.Map<NLinearMapEntity[]>(src.FindAllOfType<NLinearMapDTO>()).Cast<T>();
            //return src.FindAllOfType<NLinearMapDTO>().Cast<T>();
        }
        public void Update<T>(T item) {
            var src = DataSource as LocalDataSource;

            src.Update(_mapper.Map<NLinearMapDTO>(item));
        }
    }
}
