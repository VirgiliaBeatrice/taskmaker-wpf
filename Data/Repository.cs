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
using System.Diagnostics;
using SkiaSharp;
using taskmaker_wpf.ViewModels;
using Numpy;

namespace taskmaker_wpf.Data {

    // Hold DTO, depend on Entity
    public interface IRepository {
        IDataSource DataSource { get; }
        void Add<T>(T item);
        void Update<T>(T item);
        void Delete<T>(T item);
        void Save(string filename);
        void Load(string filename);
        T Find<T>(int id); 
        IEnumerable<T> FindAll<T>();

    }

    public class SerializableMapEntity {
        public int Id { get; set; }
        public string Name { get; set; }
        public double[] Tensor { get; set; }
        public int[] Shape { get; set; }

        public OutputPort[] OutputPorts { get; set; }
        public InputPort[] InputPorts { get; set; }

        public bool IsDirty { get; set; } = false;

        //public static implicit operator SerilizableMapEntity(NLinearMapEntity e) => new SerilizableMapEntity(e);
        public static explicit operator SerializableMapEntity(NLinearMapEntity e) => new SerializableMapEntity(e);
        public static explicit operator NLinearMapEntity(SerializableMapEntity e) => e.ToEntity();

        public SerializableMapEntity() { }

        public SerializableMapEntity(NLinearMapEntity entity) {
            if (entity.IsFullySet) {
                Tensor = entity.Tensor.GetData<double>();
                Shape = entity.Shape;
            }
            else {
                IsDirty = false;
            }

            OutputPorts = entity.OutputPorts;
            InputPorts = entity.InputPorts;
            Id= entity.Id;
            Name = entity.Name;
            IsDirty = false;

        }

        public NLinearMapEntity ToEntity() {
            return new NLinearMapEntity() {
                Id = Id,
                Name = Name,
                Tensor = Tensor != null ? np.array(Tensor).reshape(Shape) : null,
                Shape = Shape,
                OutputPorts = OutputPorts,
                InputPorts = InputPorts,
                IsDirty = IsDirty
            };
        }
    }

    public class ProjectDataObject {
        public List<MotorEntity> Motors { get; set; } = new List<MotorEntity>();
        public List<ControlUiEntity> Uis { get; set; } = new List<ControlUiEntity>();

        [XmlIgnore]
        public List<NLinearMapEntity> Maps { get; set; } = new List<NLinearMapEntity>();

        public List<SerializableMapEntity> SerialMaps { get; set; } = new List<SerializableMapEntity>();


        public void PreSerialize() {
            SerialMaps = Maps.Select(e => (SerializableMapEntity)e).ToList(); 
        }

        public void AfterDeserialize() {
            Maps = SerialMaps.Select(e => (NLinearMapEntity)e).ToList();
        }

    }

    public class ProjectRepository : IRepository {
        public IDataSource DataSource { get; set;}
        public ProjectDataObject Project { get; set; } = new ProjectDataObject();

        private List<MotorEntity> Motors => Project.Motors;

        public void Add<T>(T item) {
            if (item is MotorEntity motor) {
                Project.Motors.Add(motor);
            }
            else if (item is ControlUiEntity ui) {
                Project.Uis.Add(ui);
            }
            else if (item is NLinearMapEntity map) {
                Project.Maps.Add(map);
            }
        }

        // TODO: Clone
        public void Update<T>(T item) {
            if (item is MotorEntity motor) {
                var targetIdx = Project.Motors.FindIndex(e => e.Id == motor.Id);

                if (targetIdx != -1)
                    Project.Motors[targetIdx] = motor;
            }
            else if (item is ControlUiEntity ui) {
                var targetIdx = Project.Uis.FindIndex(e => e.Id == ui.Id);

                if (targetIdx != -1)
                    Project.Uis[targetIdx] = ui;
            }
            else if (item is NLinearMapEntity map) {
                var targetIdx = Project.Maps.FindIndex(e => e.Id == map.Id);

                if (targetIdx != -1)
                    Project.Maps[targetIdx] = map;
            }
        }

        public void Delete<T>(T item) {
            if (item is MotorEntity motor) {
                Project.Motors.Remove(Project.Motors.Find(e => e.Id == motor.Id));
            }
            else if (item is ControlUiEntity ui) {
                Project.Uis.Remove(Project.Uis.Find(e => e.Id == ui.Id));
            }
            else if (item is NLinearMapEntity map) {
                Project.Maps.Remove(Project.Maps.Find(e => e.Id == map.Id));
            }
        }

        public T Find<T>(int id){
            var itemType = typeof(T);
            object target = null;

            if (itemType == typeof(MotorEntity)) {
                target = Project.Motors.Find(e => e.Id == id);
            }
            else if (itemType == typeof(ControlUiEntity)) {
                target = Project.Uis.Find(e => e.Id == id);

            }
            else if (itemType == typeof(NLinearMapEntity)) {
                target = Project.Maps.Find(e => e.Id == id);
            }
            
            return target != null ? (T)target : default;
        }

        public IEnumerable<T> FindAll<T>() {
            var itemType = typeof(T);

            if (itemType == typeof(MotorEntity)) {
                return Project.Motors.Cast<T>().ToArray();
            }
            else if (itemType == typeof(ControlUiEntity)) {
                return Project.Uis.Cast<T>().ToArray();
            }
            else if (itemType == typeof(NLinearMapEntity)) {
                return Project.Maps.Cast<T>().ToArray();
            }

            return new[] { default(T) };
        }

        public void Save(string filename) {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker"));

            //var xmlFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");
            var xmlFilePath = filename;
            var xml = new XmlSerializer(typeof(ProjectDataObject));

            using (var xmlfs = File.Create(xmlFilePath)) {
                Project.PreSerialize();
                xml.Serialize(xmlfs, Project);
            }
        }


        public void Load(string filename) {
            //var xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TaskMaker", "project.xml");
            var xmlPath = filename;

            var xml = new XmlSerializer(typeof(ProjectDataObject));

            if (!File.Exists(xmlPath)) {
                //var dataSource = new LocalDataSource();
                //dataSource.BindEventAggregator(ea);
                //return dataSource;
            }

            using (var fs = File.OpenRead(xmlPath)) {
                var xmlObject = (ProjectDataObject)xml.Deserialize(fs);
                Project = xmlObject;

                Project.AfterDeserialize();
            }
        }
    }

    //public class MotorRepository {
    //    public IDataSource DataSource {
    //        get;
    //        private set;
    //    }
    //    private IMapper _mapper;
    //    private readonly SerialService _serial;

    //    public MotorRepository(IDataSource dataSource, MapperConfiguration config, SerialService serial) {
    //        DataSource = dataSource;
    //        _mapper = config.CreateMapper();
    //        _serial = serial;
    //    }

    //    public void Add<T>(T item) {
    //        var src = DataSource as LocalDataSource;

    //        src.Add(_mapper.Map<MotorDTO>(item));
    //    }

    //    public void Delete<T>(T item) {
    //        var src = DataSource as LocalDataSource;

    //        src.Remove(_mapper.Map<MotorDTO>(item));
    //    }

    //    public T Find<T>(int id) {
    //        var src = DataSource as LocalDataSource;

    //        src.Find<MotorDTO>(id);

    //        return default(T);
    //    }

    //    public IEnumerable<T> FindAll<T>() {
    //        var src = DataSource as LocalDataSource;

    //        var dto = src.FindAllOfType<MotorDTO>().FirstOrDefault();
    //        var entity = _mapper.Map<MotorEntity>(dto);
    //        return src.FindAllOfType<MotorDTO>().Select(e => _mapper.Map<MotorEntity>(e)).Cast<T>();
    //    }

    //    public void Update<T>(T item) {
    //        var src = DataSource as LocalDataSource;
    //        var motor = item as MotorEntity;

    //        src.Update(_mapper.Map<MotorDTO>(motor));
    //        _serial.Update(motor.BoardId, motor.MotorId, motor.Value[0]);

    //        //
    //         _serial.SendToNuibot();
    //    }
    //}

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
