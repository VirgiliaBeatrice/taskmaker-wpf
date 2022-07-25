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
    internal interface IUseCase {
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

        public ITarget[] GetTargets() {
            var motors = _motorRepo.GetMotors();
            var controlUis = _controlUiRepo.GetControlUis();

            var targets = new List<ITarget>();

            targets.AddRange(motors);
            targets.AddRange(controlUis);

            return targets.ToArray();
        }
    }

    public class ControlUiUseCase {
        private ControlUiRepository _controlUiRepo;

        public ControlUiUseCase(
            ControlUiRepository controlUiRepo) {
            _controlUiRepo = controlUiRepo;
        }

        public void AddNode(ControlUiEnity controlUi, NDarray<float> pt) {
            // Domain
            var node = controlUi.AddNode(pt);

            // Update repo
            _controlUiRepo.AddNode(node);
        }

        public void Build(ControlUi controlUi) {
            // Domain
            controlUi.Complex.Build();

            // Update repo
            _controlUiRepo.Build(controlUi.Complex.Regions);
        }

        public void GetRegions() {
            // fetch from repo

        }
    }

    public class BuildRegionUseCase {
        private ControlUiRepository _controlUiRepository;

        public BuildRegionUseCase(ControlUiRepository controlUiRepository) {
            _controlUiRepository = controlUiRepository;
        }

        // Nodes => Simplices => Voronois
        public void Build() {
            var controlUi = _controlUiRepository.FindByName();
            var nodes = controlUi.Complex.Nodes;
            var nodeInput = np.array(
                controlUi.Complex.Nodes
                    .Select(e => new[] { e.Value.X, e.Value.Y })
                    .SelectMany(e => e)
                    .ToArray());

            // Nodes
            var simplices = QhullCSharp.RunDelaunay(nodeInput)
                .Select(indices => new SimplexRegionEntity {
                    Nodes = indices.Select(idx => nodes.ElementAt(idx)).ToArray()
                })
                .ToArray();

            var extremes = QhullCSharp.RunConvex(nodeInput)
                .Select(idx => nodes.ElementAt(idx))
                .ToArray();


        }

        private void BuildVoronoiRegions(NodeEntity[] extremes, SimplexRegionEntity[] simplices) {
            if (extremes.Length <= 2)
                return;

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

                var prevGov = Array.Find(simplices, e => IsVertex(e, prev) & IsVertex(e, it));
                var nextGov = Array.Find(simplices, e => IsVertex(e, it) & IsVertex(e, next));

                var sector = new 
            }
        }

        private bool IsVertex(SimplexRegionEntity simplex, NodeEntity node) {
            return simplex.Nodes.Any(e => node == e);
        }
    }
}
