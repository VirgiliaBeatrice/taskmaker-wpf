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

namespace taskmaker_wpf.Domain {
    public interface IUseCase {
    }

    public class MotorUseCase : IUseCase {
        private MotorRepository _motorRepository;

        public MotorUseCase(MotorRepository motorRepository) {
            _motorRepository = motorRepository;
        }

        public IEnumerable<MotorEntity> GetMotors() {
            return _motorRepository.FindAll<MotorEntity>();
        }

        public MotorEntity GetMotor(int id) {
            return _motorRepository.Find<MotorEntity>(id);
        }

        public void AddMotor() {
            _motorRepository.Add(new MotorEntity());
        }

        public void RemoveMotor(MotorEntity motor) {
            _motorRepository.Delete(motor);
        }

        public void UpdateMotor(MotorEntity motor) {
            _motorRepository.Update(motor);
        }
    }

    public class ListTargetUseCase :IUseCase{
        private MotorRepository _motorRepo;
        private ControlUiRepository _controlUiRepo;

        public ListTargetUseCase(
            MotorRepository motorRepo,
            ControlUiRepository controlUiRepo
            ) {
            _motorRepo = motorRepo;
            _controlUiRepo = controlUiRepo;
        }

        public BaseEntity[] GetTargets() {
            var motors = _motorRepo.FindAll<MotorEntity>();
            var controlUis = _controlUiRepo.FindAll<ControlUiEntity>();

            var targets = new List<BaseEntity>();

            targets.AddRange(motors);
            targets.AddRange(controlUis);

            return targets.ToArray();
        }
    }

    public class ControlUiUseCase : IUseCase {
        private ControlUiRepository _repository;

        public ControlUiUseCase(ControlUiRepository repository) {
            _repository = repository;
        }

        public ControlUiEntity[] GetControlUis() {
            var uis = _repository.FindAll<ControlUiEntity>();

            return uis.ToArray();
        }

        public ControlUiEntity GetControlUi(int id) {
            return _repository.Find<ControlUiEntity>(id);
        }

        public ControlUiEntity AddUi() {
            var ui = new ControlUiEntity();
            _repository.Add(ui);

            return ui;
        }

        public void AddNode(int id, Point pt) {
            var node = new NodeEntity {
                Value = pt
            };
            var ui = _repository.Find<ControlUiEntity>(id);

            ui.Nodes = ui.Nodes.Concat(new NodeEntity[] { node }).ToArray();
            node.Id = ui.Nodes.Length - 1;

            _repository.Update(ui);
        }

        public void Update(ControlUiEntity ui) {
            _repository.Update(ui);
        }

        public double[] GetLambdas<T>(int id, int regionId, Point pt) where T: BaseRegionEntity {
            var ui = _repository.Find<ControlUiEntity>(id);
            var region = ui.Regions.Where(e => e.Id == regionId).OfType<T>().FirstOrDefault();

            return region.GetLambdas(pt, ui.Nodes);
        }
    }


    public class BuildRegionUseCase : IUseCase {
        private ControlUiRepository _controlUiRepository;

        public BuildRegionUseCase(ControlUiRepository controlUiRepository) {
            _controlUiRepository = controlUiRepository;
        }

        // Nodes => Simplices => Voronois
        public void Build(int id) {
            var ui = _controlUiRepository.Find<ControlUiEntity>(id);
            var nodes = ui.Nodes.OrderBy(e => e.Id).ToArray();
            var nodeInput = np.array(
                nodes.Select(e => np.array(new[] { e.Value.X, e.Value.Y }))
                    .ToArray());

            if (nodes.Length <= 2) return;

            // Domain
            var simplices = QhullCSharp.RunDelaunay(nodeInput)
                .Select(indices => new SimplexRegionEntity(indices.Select(idx => nodes.ElementAt(idx)).ToArray()))
                .ToArray();

            var regions = new List<BaseRegionEntity>();

            foreach(var item in simplices) {
                item.Id = regions.Count();
                regions.Add(item);
                //ui.Regions.Add(item);
            }

            var extremes = QhullCSharp.RunConvex(nodeInput)
                .Select(idx => nodes.ElementAt(idx))
                .Reverse()
                .ToArray();
            var voronois = BuildVoronoiRegions(extremes, simplices);

            foreach(var item in voronois) {
                item.Id = regions.Count();
                regions.Add(item);
                //ui.Regions.Add(item);
            }

            ui.Regions = regions.ToArray();

            // Data
            _controlUiRepository.Update(ui);
        }

        private VoronoiRegionEntity[] BuildVoronoiRegions(NodeEntity[] extremes, SimplexRegionEntity[] simplices) {
            NodeEntity prev, it, next;
            
            var voronois = new List<VoronoiRegionEntity>();

            for (var i = 0; i < extremes.Length; i++) {
                it = extremes[i];

                if (i <= 0) {
                    prev = extremes.Last();
                    next = extremes[i + 1];
                }
                else if (i < extremes.Length - 1) {
                    prev = extremes[i - 1];
                    next = extremes[i + 1];
                }
                else {
                    prev = extremes[i - 1];
                    next = extremes.First();
                }

                // Sectoral
                var prevGov = Array.Find(simplices, e => IsVertex(e, prev) & IsVertex(e, it));
                var nextGov = Array.Find(simplices, e => IsVertex(e, it) & IsVertex(e, next));

                var sectorVoronoi = new SectoralVoronoiRegionEntity(
                    new[] { prev, it, next },
                    new[] { prevGov, nextGov });

                voronois.Add(sectorVoronoi);

                // Rect
                var gov = Array.Find(simplices, e => IsVertex(e, it) & IsVertex(e, next));
                var rectVoronoi = new RectVoronoiRegionEntity(new[] { it, next }, gov);

                voronois.Add(rectVoronoi);
            }

            return voronois.ToArray();
        }

        private bool IsVertex(SimplexRegionEntity simplex, NodeEntity node) {
            return simplex.Nodes.Any(e => node == e);
        }
    }

    public class NLinearMapUseCase : IUseCase {
        private readonly NLinearMapRepository _mapRepository;
        private readonly ControlUiRepository _uiRepository;
        private readonly MotorRepository _motorRepository;

        public NLinearMapUseCase(NLinearMapRepository mapRepository,
            ControlUiRepository uiRepository,
            MotorRepository motorRepository
            ) {
            _mapRepository = mapRepository;
            _uiRepository = uiRepository;
            _motorRepository = motorRepository;
        }

        public NLinearMapEntity[] GetMaps() {
            return _mapRepository.FindAll<NLinearMapEntity>().ToArray();
        }

        public NLinearMapEntity AddMap() {
            var map = new NLinearMapEntity();
            _mapRepository.Add(map);

            return map;        
        }

        public void UpdateMap(NLinearMapEntity map) {
            _mapRepository.Update(map);
        }

        public void UpdateMapValue(int mapId, int[] indices, double[] value) {
            var map = _mapRepository.Find<NLinearMapEntity>(mapId);

            map.SetTensorValue(indices, np.array(value));

            _mapRepository.Update(map);
        }

        public void InitializeMap(int mapId, int[] sourceIds, int[] targetIds) {
            var map = _mapRepository.Find<NLinearMapEntity>(mapId);
            var sources = sourceIds.Select(id => _uiRepository.Find<ControlUiEntity>(id)).ToArray();

            // todo: now for motor only, test purpose
            var targets = targetIds.Select(id => _motorRepository.Find<MotorEntity>(id)).ToArray();

            map.SetTargets(targets);
            map.SetSources(sources);
            map.InitializeTensor();

            _mapRepository.Update(map);
        }

        public IEnumerable<int[]> GetTensorIndices(int mapId) {
            var map = _mapRepository.Find<NLinearMapEntity>(mapId);

            return NLinearMapEntity.CartesianProduct(map.Shape.Skip(1));
        }
    }
}
