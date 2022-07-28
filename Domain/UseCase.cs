using Numpy;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    }

    public class ListTargetUseCase {
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
            var motors = _motorRepo.FindAll();
            var controlUis = _controlUiRepo.FindAll();

            var targets = new List<BaseEntity>();

            targets.AddRange(motors);
            targets.AddRange(controlUis);

            return targets.ToArray();
        }
    }


    public class BuildRegionUseCase : IUseCase {
        private ControlUiRepository _controlUiRepository;

        public BuildRegionUseCase(ControlUiRepository controlUiRepository) {
            _controlUiRepository = controlUiRepository;
        }

        public void AddNode(ControlUiEnity ui, Point pt) {
            // Domain
            ui.Nodes.Add(new NodeEntity { Value = pt });

            // Data
            _controlUiRepository.Update(ui);
        }

        // Nodes => Simplices => Voronois
        public void Build(ControlUiEnity ui) {
            //var ui = _controlUiRepository.Find(ui.Name);
            var nodes = ui.Nodes;
            var nodeInput = np.array(
                nodes.Select(e => new[] { e.Value.X, e.Value.Y })
                    .SelectMany(e => e)
                    .ToArray());

            if (nodes.Count <= 2) return;

            // Domain
            var simplices = QhullCSharp.RunDelaunay(nodeInput)
                .Select(indices => new SimplexRegionEntity(indices.Select(idx => nodes.ElementAt(idx)).ToArray()))
                .ToArray();

            foreach(var item in simplices) {
                ui.Regions.Add(item);
            }

            var extremes = QhullCSharp.RunConvex(nodeInput)
                .Select(idx => nodes.ElementAt(idx))
                .ToArray();
            var voronois = BuildVoronoiRegions(extremes, simplices);

            foreach(var item in voronois) {
                ui.Regions.Add(item);
            }

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
}
