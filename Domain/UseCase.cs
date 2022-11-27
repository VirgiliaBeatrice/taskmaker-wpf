using Numpy;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Data;
using taskmaker_wpf.Model.Data;
using taskmaker_wpf.Models;
using taskmaker_wpf.Qhull;
using System.Security.RightsManagement;
using System.Diagnostics;
using taskmaker_wpf.ViewModels;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.Domain {
    public interface IPresenter { }
    public interface IUseCase {
        IRepository Repository { get; set; }
        //IPresenter Presenter { get; set; }
        //void Handle<T>(T request);
        void Handle<T, K>(T request, Action<K> callback);

    }

    public interface IRequest { }
    public interface IResponse { }

    public class Request : IRequest {
        static public void Spread<T, K>(T src, K dst) {
            var srcType = typeof(T);
            var dstType = typeof(K);

            srcType.GetProperties()
                .ToList()
                .ForEach(e => {
                    var propSrc = e;
                    var propDst = dstType.GetProperty(e.Name);

                    if (propSrc.GetValue(src) != null) {
                        propDst.SetValue(dst, propSrc.GetValue(src));
                    }
                });
        }
    }

    public class AddMotorRequest : Request { }
    public class UpdateMotorRequest : Request {
        public int Id { get; set; }
        public string PropertyName { get; set; }
        public string ValueType {get;set;}
        public object Value { get; set; }
    }

    public class DeleteMotorRequest : Request {
        public int Id { get; set; }
    }
    public class ListMotorRequest : Request {
        public int Id { get; set; } = -1;
    }


    public class MotorInteractorBus : BaseInteractorBus {
        private SerialService _serial;
        public MotorInteractorBus(IRepository repository, SerialService serial) {
            _serial = serial;

            interactors = new BaseInteractor[] {
                new AddMotorInteractor(repository),
                new UpdateMotorInteractor(repository, _serial),
                new DeleteMotorInteractor(repository),
                new ListMotorInteractor(repository),
            };
        }
    }

    public class AddMotorInteractor : BaseInteractor {
        public AddMotorInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var idx = Repository.FindAll<MotorEntity>().Count();
            // Request => Entity
            var motor = new MotorEntity {
                Id = idx,
                Max = 10000,
                Min = -10000,
                Name = $"Motor[{idx}]",
            };

            Repository.Add(motor);

            callback((K)(object)true);
        }

        public override void Handle<T, K>(T request, out K result) {
            result = (K)(object)false;
            
            if (request is AddMotorRequest req) {
                var idx = Repository.FindAll<MotorEntity>().Count();
                // Request => Entity
                var motor = new MotorEntity {
                    Id = idx,
                    Max = 10000,
                    Min = -10000,
                    Name = $"Motor[{idx}]",
                };

                Repository.Add(motor);

                result = ((K)(object)true);
            }
        }
    }

    public class UpdateMotorInteractor : BaseInteractor {
        private readonly SerialService _serial;
        public UpdateMotorInteractor(IRepository repository, SerialService serial) : base(repository) {
            _serial = serial;
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var req = request as UpdateMotorRequest;
            var motor = Repository.Find<MotorEntity>(req.Id);

            motor.GetType().GetProperty(req.PropertyName).SetValue(motor, req.Value);
            
            //Request.Spread(req, motor);

            Repository.Update(motor);

            callback((K)(object)true);
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is UpdateMotorRequest req) {
                var motor = Repository.Find<MotorEntity>(req.Id);

                //motor.GetType().GetProperty(req.PropertyName).SetValue(motor, req.Value);
                motor.Value = (double[])req.Value;
                //Request.Spread(req, motor);

                Repository.Update(motor);

                // Send to serial service
                _serial.Update(motor);

                result = ((K)(object)motor);
            }

        }
    }

    public class DeleteMotorInteractor : BaseInteractor {
        public DeleteMotorInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var motor = new MotorEntity();

            Repository.Delete(motor);

            callback((K)(object)true);
        }
    }

    public class ListMotorInteractor : BaseInteractor {
        public ListMotorInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is ListMotorRequest req) {
                if (req.Id == -1) {
                    var targets = Repository.FindAll<MotorEntity>();

                    callback((K)(object)targets);
                }
                else {
                    var target = Repository.Find<MotorEntity>(req.Id);

                    callback((K)(object)target);
                }
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is ListMotorRequest req) {
                if (req.Id == -1) {
                    var targets = Repository.FindAll<MotorEntity>();

                    result = (K)(object)targets;
                }
                else {
                    var target = Repository.Find<MotorEntity>(req.Id);

                    result = (K)(object)target;
                }
            }
        }
    }

    public class ListTargetRequest { }

    public class ListTargetInteractor : BaseInteractor {
        public ListTargetInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var motors = Repository.FindAll<MotorEntity>();
            var uis = Repository.FindAll<ControlUiEntity>();

            var targets = new List<ITargetableEntity>().Concat(motors).Concat(uis).ToArray();

            callback((K)(object)targets);
        }
    }


    public abstract class BaseInteractor : IUseCase {
        public BaseInteractor(IRepository repository) {
            Repository = repository;
        }

        public IRepository Repository { get; set; }

        //public abstract void Handle<T>(T request);
        public abstract void Handle<T, K>(T request, Action<K> callback);
        public virtual Task<K> HandleAsync<K, T>(T request) {
            throw new NotImplementedException();
        }

        //public virtual K Handle<T, K>(T request) {
        //    throw new NotImplementedException();
        //}

        public virtual void Handle<T, K>(T request, out K result) {
            throw new NotImplementedException();
        }
    }

    //public class AddNodeRequest { }
    //public class UpdateNodeRequest { }
    //public class DeleteNodeRequest { }
    //public class ListNodeRequest { }

    public class AddControlUiRequest : Request { }

    public class UpdateControlUiRequest : Request {
        public int Id { get; set; }

        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }

    }
    public class DeleteControlUiRequest : Request {
        public int Id { get; set; }
    }
    public class ListControlUiRequest : Request {
        public int Id { get; set; } = -1;
    }
    public class BuildRegionRequest {
        public int Id { get; set; }
    }

    public class ControlUiInteractorBus : BaseInteractorBus {
        public ControlUiInteractorBus(IRepository repository) : base() {
            interactors = new BaseInteractor[] {
                new AddControlUiInteractor(repository),
                new UpdateControlUiInteractor(repository),
                new DeleteControlUiInteractor(repository),
                new ListControlUiInteractor(repository),
                new AddNodeInteractor(repository),
                new UpdateNodeInteractor(repository),
                new BuildRegionInteractor(repository)
            };
        }
    }

    public class AddNodeRequest : Request {
        public int UiId { get; set; }
        public Point Value { get; set; }
    }

    public class ListNodeRequest : Request { 
        public int UiId { get; set; }
    }

    public class AddNodeInteractor : BaseInteractor {
        public AddNodeInteractor(IRepository repository) : base(repository) { }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is AddNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);
                var idx = ui.Nodes.Length;
                var node = new NodeEntity {
                    Id = idx,
                    Name = $"ControlUi[{req.UiId}]_Node[{idx}]",
                    Value = req.Value,
                };

                ui.Nodes = ui.Nodes.Append(node).ToArray();

                Repository.Update(ui);
                
                callback((K)(object)true);
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = (K)(object)false;

            if (request is AddNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);
                var idx = ui.Nodes.Length;
                var node = new NodeEntity {
                    Id = idx,
                    Name = $"ControlUi[{req.UiId}]_Node[{idx}]",
                    Value = req.Value,
                };

                ui.Nodes = ui.Nodes.Append(node).ToArray();

                Repository.Update(ui);

                result = (K)(object)true;
            }
        }
    }

    public class UpdateNodeRequest : Request {
        public int UiId { get; set; }
        public int NodeId { get; set; }
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
    }

    public class UpdateNodeInteractor : BaseInteractor {
        public UpdateNodeInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is UpdateNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);

                if (req.PropertyName == "TargetValue") {
                    ui.Nodes[req.NodeId].TargetValue = (double[])req.PropertyValue;
                }
                
                Repository.Update(ui);
                
                callback((K)(object)ui);
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is UpdateNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);

                if (req.PropertyName == "TargetValue") {
                    ui.Nodes[req.NodeId].TargetValue = (double[])req.PropertyValue;
                }

                Repository.Update(ui);

                result = (K)(object)ui;
            }
        }
    }

    public class DeleteNodeInteractor : BaseInteractor {
        public DeleteNodeInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var node = new NodeEntity();

            Repository.Delete(node);

            callback((K)(object)true);
        }
    }

    public class ListNodeInteractor : BaseInteractor {
        public ListNodeInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is ListNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);

                callback((K)(object)ui.Nodes);
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is ListNodeRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.UiId);

                result = (K)(object)ui.Nodes;
            }
        }
    }

    public class AddControlUiInteractor : BaseInteractor {
        public AddControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var idx = Repository.FindAll<ControlUiEntity>().Count();
            var ui = new ControlUiEntity {
                Id = idx,
                Name = $"ControlUi[{idx}]",
                Nodes = new NodeEntity[0],
                Regions = new BaseRegionEntity[0],
            };

            Repository.Add(ui);

            callback((K)(object)true);
        }
    }

    public class UpdateControlUiInteractor : BaseInteractor {
        public UpdateControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is UpdateControlUiRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.Id);

                if (req.PropertyName == "UpdateTargets") {
                    ui.Targets = (TargetEntity[])req.PropertyValue;

                    Repository.Update(ui);

                    callback((K)(object)ui);
                }
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is UpdateControlUiRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.Id);

                if (req.PropertyName == "Value") {
                    //ui.Targets = (TargetEntity[])req.PropertyValue;

                    Repository.Update(ui);

                    result = ((K)(object)ui);
                }
            }
        }
    }

    public class DeleteControlUiInteractor : BaseInteractor {
        public DeleteControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var ui = new ControlUiEntity();

            Repository.Delete(ui);

            callback((K)(object)true);
        }
    }

    public class ListControlUiInteractor : BaseInteractor {
        public ListControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is ListControlUiRequest req) {
                if (req.Id == -1) {
                    var uis = Repository.FindAll<ControlUiEntity>();

                    callback((K)(object)uis);
                }
                else {
                    var ui = Repository.Find<ControlUiEntity>(req.Id);

                    callback((K)(object)ui);
                }
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is ListControlUiRequest req) {
                if (req.Id == -1) {
                    var uis = Repository.FindAll<ControlUiEntity>();

                    result = (K)(object)uis;
                }
                else {
                    var ui = Repository.Find<ControlUiEntity>(req.Id);

                    result = (K)(object)ui;
                }
            }

        }
    }


    public class BuildRegionInteractor : BaseInteractor {
        public BuildRegionInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is BuildRegionRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.Id);

                ui.Build();

                Repository.Update(ui);

                callback((K)(object)ui);
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;

            if (request is BuildRegionRequest req) {
                var ui = Repository.Find<ControlUiEntity>(req.Id);

                ui.Build();

                Repository.Update(ui);

                result = (K)(object)ui;
            }
        }
    }

    public class CreateNLinearMapRequest : Request {
        public int Id { get; set; }
    }


    public class AddNLinearMapRequest : Request {
        public int Id { get; set; }
    }
    public class UpdateNLinearMapRequest : Request {
        public int Id { get; set; }

        public string PropertyType { get; set; }
        public object PropertyValue { get; set; }
        //public int MapId { get; set; }
        //public string RequestType { get; set; }
        //public object Value { get; set; }
    }
    public class ListNLinearMapRequest : Request {
        public int Id { get; set; } = -1;
    }

    public class CreateNLinearMapInteractor : BaseInteractor {
        public CreateNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is CreateNLinearMapRequest req) {
                //var last = Repository.FindAll<NLinearMapEntity>().LastOrDefault() ??
                var ui = Repository.Find<ControlUiEntity>(req.Id);

                if (ui.Targets == null)
                    callback((K)(object)null);
                else {
                    var map = new NLinearMapEntity();

                    map.Name = $"Map[{ui.Name}]";

                    Repository.Add(map);

                    callback((K)(object)map);
                }
            }
        }
    }
    
    public class AddNLinearMapInteractor : BaseInteractor {
        public AddNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is AddNLinearMapRequest req) {
                var idx = Repository.FindAll<NLinearMapEntity>().Count();
                //var ui = Repository.Find<ControlUiEntity>(req.Id);

                var map = new NLinearMapEntity {
                    Id = idx,
                    Name = $"Map[{idx}]"
                };

                Repository.Add(map);

                callback((K)(object)true);

                //if (ui.Targets == null) {
                //    callback((K)(object)false);
                //}
                //else {
                //    var map = NLinearMapEntity.Create(ui);

                //    map.Id = idx;
                //    map.Name = $"Map[{idx}]";
                
                //    Repository.Add(map);

                //    callback((K)(object)true);
                //}
            }
        }
    }

    public struct ValueContract {
        public int[] Index { get; set; }
        public double[] Value { get; set; }
    }

    public class UpdateNLinearMapInteractor : BaseInteractor {
        public UpdateNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is UpdateNLinearMapRequest req) {
                var map = Repository.Find<NLinearMapEntity>(req.Id);

                if (req.PropertyType == "UpdateInputs") {
                    map.InputPorts = (InputPort[])req.PropertyValue;
                }
                else if (req.PropertyType == "UpdateOutputs") {
                    map.OutputPorts = (OutputPort[])req.PropertyValue;

                }
                else if (req.PropertyType == "SetValue") {
                    var contract = (ValueContract)req.PropertyValue;

                    map.SetValue(contract.Index, contract.Value);
                }
                else if (req.PropertyType == "Init") {
                    map.Initialize();
                }

                Repository.Update(map);

                callback((K)(object)map);
            }
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default(K);

            if (request is UpdateNLinearMapRequest req) {
                var map = Repository.Find<NLinearMapEntity>(req.Id);

                if (req.PropertyType == "UpdateInputs") {
                    map.InputPorts = (InputPort[])req.PropertyValue;

                    //map.Initialize();
                }
                else if (req.PropertyType == "UpdateOutputs") {
                    map.OutputPorts = (OutputPort[])req.PropertyValue;

                    //map.Initialize();
                }
                else if (req.PropertyType == "SetValue") {
                    var contract = (ValueContract)req.PropertyValue;

                    map.SetValue(contract.Index, contract.Value);
                }
                else if (req.PropertyType == "Init") {
                    map.Initialize();
                }

                Repository.Update(map);

                result = (K)(object)map;
            }
        }
    }

    public class ListNLinearMapInteractor : BaseInteractor {
        public ListNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var targets = Repository.FindAll<NLinearMapEntity>();

            callback((K)(object)targets);
        }

        public override void Handle<T, K>(T request, out K result) {
            result = default;
            
            if (request is ListNLinearMapRequest req) {
                if (req.Id != -1) {
                    var target = Repository.Find<NLinearMapEntity>(req.Id);

                    result = ((K)(object)target);
                }
                else {
                    var targets = Repository.FindAll<NLinearMapEntity>();

                    result = ((K)(object)targets);
                }
            }

        }
    }


    public class NLinearMapInteractorBus : BaseInteractorBus {
        //private AddNLinearMapInteractor _add;
        //private UpdateNLinearMapInteractor _update;
        //private ListNLinearMapInteractor _list;

        //private BaseInteractor[] _interactors;

        public NLinearMapInteractorBus(IRepository repository) {
            interactors = new BaseInteractor[] {
                new AddNLinearMapInteractor(repository),
                new UpdateNLinearMapInteractor(repository),
                new ListNLinearMapInteractor(repository),
                new CreateNLinearMapInteractor(repository)
            };
        }

        //public void Handle<T, K>(T request, Action<K> callback) {
        //    var requestType = typeof(T);

        //    if (requestType == typeof(AddNLinearMapRequest))
        //        _add.Handle(request, callback);
        //    else if (requestType == typeof(UpdateNLinearMapRequest))
        //        _update.Handle(request, callback);
        //    else if (requestType == typeof(ListNLinearMapRequest))
        //        _list.Handle(request, callback);
        //}
    }

    public class SystemInteractorBus {
        private readonly SaveInteractor _save;
        private readonly LoadInteractor _load;

        public SystemInteractorBus(IRepository repository) {
            _save = new SaveInteractor(repository);
            _load = new LoadInteractor(repository);
        }

        public void Handle<T, K>(T request, Action<K> callback) {
            var fieldName = "_" + typeof(T).Name.Replace("Request", "").ToLower();
            var interactor = (BaseInteractor)typeof(SystemInteractorBus).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);

            // "_[name]"
            interactor?.Handle(request, callback);
        }
    }

    public class BaseInteractorBus {
        protected BaseInteractor[] interactors;

        public void Handle<T, K>(T request, Action<K> callback) {
            var interactorName = typeof(T).Name.Replace("Request", "Interactor");
            var interactor = interactors.Where(e => e.GetType().Name == interactorName).FirstOrDefault();

            if (interactor == null)
                throw new NullReferenceException();
            else
                interactor.Handle(request, callback);
        }

        public void Handle<T, K>(T request, out K result) {
            var interactorName = typeof(T).Name.Replace("Request", "Interactor");
            var interactor = interactors.Where(e => e.GetType().Name == interactorName).FirstOrDefault();

            result = default;

            if (interactor == null)
                throw new NullReferenceException();
            else {
                interactor.Handle(request, out K resultDeep);
                result = resultDeep;
            }
        }
    }

    public class SaveRequest : Request {
        public string FileName { get; set; }
    }

    public class SaveInteractor : BaseInteractor {
        public SaveInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is SaveRequest req) {
                Repository.Save(req.FileName);

                callback((K)(object)true);
            }
        }
    }

    public class LoadRequest : Request {
        public string FileName { get; set; }
    }

    public class LoadInteractor : BaseInteractor {
        public LoadInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            if (request is LoadRequest req) {
                Repository.Load(req.FileName);

                callback((K)(object)true);
            }
        }
    }

    //public class BuildRegionUseCase : IUseCase {
    //    private ControlUiRepository _controlUiRepository;

    //    public BuildRegionUseCase(ControlUiRepository controlUiRepository) {
    //        _controlUiRepository = controlUiRepository;
    //    }

    //    // Nodes => Simplices => Voronois
    //    public void Build(int id) {
    //        var ui = _controlUiRepository.Find<ControlUiEntity>(id);
    //        var nodes = ui.Nodes.OrderBy(e => e.Id).ToArray();
    //        var nodeInput = np.array(
    //            nodes.Select(e => np.array(new[] { e.Value.X, e.Value.Y }))
    //                .ToArray());

    //        if (nodes.Length <= 2) return;

    //        // Domain
    //        var simplices = QhullCSharp.RunDelaunay(nodeInput)
    //            .Select(indices => new SimplexRegionEntity(indices.Select(idx => nodes.ElementAt(idx)).ToArray()))
    //            .ToArray();

    //        var regions = new List<BaseRegionEntity>();

    //        foreach(var item in simplices) {
    //            regions.Add(item);
    //            //ui.Regions.Add(item);
    //        }

    //        var extremes = QhullCSharp.RunConvex(nodeInput)
    //            .Select(idx => nodes.ElementAt(idx))
    //            .Reverse()
    //            .ToArray();
    //        var voronois = BuildVoronoiRegions(extremes, simplices);

    //        foreach(var item in voronois) {
    //            regions.Add(item);
    //            //ui.Regions.Add(item);
    //        }

    //        ui.Regions = regions.ToArray();

    //        // Data
    //        _controlUiRepository.Update(ui);
    //    }

    //    private VoronoiRegionEntity[] BuildVoronoiRegions(NodeEntity[] extremes, SimplexRegionEntity[] simplices) {
    //        NodeEntity prev, it, next;

    //        var voronois = new List<VoronoiRegionEntity>();

    //        for (var i = 0; i < extremes.Length; i++) {
    //            it = extremes[i];

    //            if (i <= 0) {
    //                prev = extremes.Last();
    //                next = extremes[i + 1];
    //            }
    //            else if (i < extremes.Length - 1) {
    //                prev = extremes[i - 1];
    //                next = extremes[i + 1];
    //            }
    //            else {
    //                prev = extremes[i - 1];
    //                next = extremes.First();
    //            }

    //            // Sectoral
    //            var prevGov = Array.Find(simplices, e => IsVertex(e, prev) & IsVertex(e, it));
    //            var nextGov = Array.Find(simplices, e => IsVertex(e, it) & IsVertex(e, next));

    //            var sectorVoronoi = new SectoralVoronoiRegionEntity(
    //                new[] { prev, it, next },
    //                new[] { prevGov, nextGov });

    //            voronois.Add(sectorVoronoi);

    //            // Rect
    //            var gov = Array.Find(simplices, e => IsVertex(e, it) & IsVertex(e, next));
    //            var rectVoronoi = new RectVoronoiRegionEntity(new[] { it, next }, gov);

    //            voronois.Add(rectVoronoi);
    //        }

    //        return voronois.ToArray();
    //    }

    //    private bool IsVertex(SimplexRegionEntity simplex, NodeEntity node) {
    //        return simplex.Nodes.Any(e => node == e);
    //    }
    //}

    //public class NLinearMapUseCase : IUseCase {
    //    private readonly NLinearMapRepository _repository;

    //    public NLinearMapUseCase(NLinearMapRepository repository) {
    //        _repository = repository;
    //    }

    //    public NLinearMapEntity[] GetMaps() {
    //        return _repository.FindAll<NLinearMapEntity>().ToArray();
    //    }

    //    public NLinearMapEntity AddMap() {
    //        var map = new NLinearMapEntity();
    //        _repository.Add(map);

    //        return map;        
    //    }

    //    public void UpdateMap(NLinearMapEntity map) {
    //        _repository.Update(map);
    //    }

    //    public void UpdateMapValue(int mapId, int nodeId, double[] value) {
    //        var map = _repository.Find<NLinearMapEntity>(mapId);

    //        map.SetValue(new int[] { nodeId }, value);
    //    }

    //    public void InitializeTensor(int id) {
    //        var map = _repository.Find<NLinearMapEntity>(id);

    //        map.InitializeTensor();
    //    }

    //    public void SetMapTargetDim(int id, int targetDim) {
    //        var map = _repository.Find<NLinearMapEntity>(id);

    //        map.SetTargetDim(targetDim);
    //    }

    //    public void SetBasisDim(int id, int[] basisDim) {
    //        var map = _repository.Find<NLinearMapEntity>(id);

    //        map.SetBasisDim(basisDim);
    //    }

    //}
}
