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
    public class ListMotorRequest : Request { }


    public class MotorInteractorBus {
        private AddMotorInteractor _add;
        private UpdateMotorInteractor _update;
        private DeleteMotorInteractor _delete;
        private ListMotorInteractor _list;

        public MotorInteractorBus(IRepository repository) {
            _add = new AddMotorInteractor(repository);
            _update = new UpdateMotorInteractor(repository);
            _delete = new DeleteMotorInteractor(repository);
            _list = new ListMotorInteractor(repository);
        }

        public void Handle<T, K>(T request, Action<K> callback) {
            var requestType = typeof(T);

            if (requestType == typeof(AddMotorRequest))
                _add.Handle(request, callback);
            else if (requestType == typeof(UpdateMotorRequest))
                _update.Handle(request, callback);
            else if (requestType == typeof(DeleteMotorRequest))
                _delete.Handle(request, callback);
            else if (requestType == typeof(ListMotorRequest))
                _list.Handle(request, callback);
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
    }

    public class UpdateMotorInteractor : BaseInteractor {
        public UpdateMotorInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var req = request as UpdateMotorRequest;
            var motor = Repository.Find<MotorEntity>(req.Id);

            motor.GetType().GetProperty(req.PropertyName).SetValue(motor, req.Value);
            
            //Request.Spread(req, motor);

            Repository.Update(motor);

            callback((K)(object)true);
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
            var targets = Repository.FindAll<MotorEntity>();

            callback((K)(object)targets);
        }
    }

    public class ListTargetRequest { }

    public class ListTargetInteractor : BaseInteractor {
        public ListTargetInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var motors = Repository.FindAll<MotorEntity>();
            var uis = Repository.FindAll<ControlUiEntity>();

            var targets = new List<ITargetable>().Concat(motors).Concat(uis);

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
    public class ListControlUiRequest : Request { }
    public class BuildRegionRequest { }

    public class ControlUiInteractorBus {
        private AddControlUiInteractor _add;
        private UpdateControlUiInteractor _update;
        private DeleteControlUiInteractor _delete;
        private ListControlUiInteractor _list;

        public ControlUiInteractorBus(IRepository repository) {
            _add = new AddControlUiInteractor(repository);
            _update = new UpdateControlUiInteractor(repository);
            _delete = new DeleteControlUiInteractor(repository);
            _list = new ListControlUiInteractor(repository);
        }

        public void Handle<T, K>(T request, Action<K> callback) {
            var requestType = typeof(T);

            if (requestType == typeof(AddControlUiRequest))
                _add.Handle(request, callback);
            else if (requestType == typeof(UpdateControlUiRequest))
                _update.Handle(request, callback);
            else if (requestType == typeof(DeleteControlUiRequest))
                _delete.Handle(request, callback);
            else if (requestType == typeof(ListControlUiRequest))
                _list.Handle(request, callback);
        }
    }

    public class AddNodeInteractor : BaseInteractor {
        public AddNodeInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var ui = new ControlUiEntity();
            var node = new NodeEntity { };

            Repository.Update(ui);
            //Repository.Add(node);

            callback((K)(object)true);
        }
    }

    public class UpdateNodeInteractor : BaseInteractor {
        public UpdateNodeInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var node = new NodeEntity();

            Repository.Update(node);

            callback((K)(object)true);
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
            var targets = Repository.FindAll<NodeEntity>();

            callback((K)(object)targets);
        }
    }

    public class AddControlUiInteractor : BaseInteractor {
        public AddControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var ui = new ControlUiEntity {
            };

            Repository.Add(ui);

            callback((K)(object)true);
        }
    }

    public class UpdateControlUiInteractor : BaseInteractor {
        public UpdateControlUiInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var ui = new ControlUiEntity();

            Repository.Update(ui);

            callback((K)(object)true);
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
            var uis = Repository.FindAll<ControlUiEntity>();

            callback((K)(object)uis);
        }
    }


    public class BuildRegionInteractor : BaseInteractor {
        public BuildRegionInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var ui = new ControlUiEntity();

            ui.Build();

            Repository.Update(ui);

            callback((K)(object)true);
        }
    }


    public class AddNLinearMapRequest { }
    public class UpdateNLinearMapRequest { }
    public class ListNLinearMapRequest { }

    public class AddNLinearMapInteractor : BaseInteractor {
        public AddNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var map = new NLinearMapEntity();

            Repository.Add(map);

            callback((K)(object)true);
        }
    }

    public class UpdateNLinearMapInteractor : BaseInteractor {
        public UpdateNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var map = new NLinearMapEntity();

            Repository.Update(map);

            callback((K)(object)true);
        }
    }

    public class ListNLinearMapInteractor : BaseInteractor {
        public ListNLinearMapInteractor(IRepository repository) : base(repository) {
        }

        public override void Handle<T, K>(T request, Action<K> callback) {
            var targets = Repository.FindAll<NLinearMapEntity>();

            callback((K)(object)targets);
        }
    }


    public class NLinearMapInteractorBus {
        //public IRepository Repository { get; set; }
        //public IPresenter Presenter { get; set; }

        private AddNLinearMapInteractor _add;
        private UpdateNLinearMapInteractor _update;
        private ListNLinearMapInteractor _list;

        public NLinearMapInteractorBus(IRepository repository) {
            //Repository = repository;
            //Presenter = presenter;

            _add = new AddNLinearMapInteractor(repository);
            _update = new UpdateNLinearMapInteractor(repository);
            _list = new ListNLinearMapInteractor(repository);
        }

        public void Handle<T, K>(T request, Action<K> callback) {
            var requestType = typeof(T);

            if (requestType == typeof(AddNLinearMapRequest))
                _add.Handle(request, callback);
            else if (requestType == typeof(UpdateNLinearMapRequest))
                _update.Handle(request, callback);
            else if (requestType == typeof(ListNLinearMapRequest))
                _list.Handle(request, callback);
        }
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
