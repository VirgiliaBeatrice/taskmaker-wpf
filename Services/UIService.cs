using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using taskmaker_wpf.Domain;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    public class UIService {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly NLinearMapInteractorBus _mapBus;
        private readonly ControlUiInteractorBus _uiBus;

        public List<ControlUiEntity> UIs { get; set; } = new List<ControlUiEntity>();

        public UIService(
            NLinearMapInteractorBus mapBus,
            ControlUiInteractorBus uiBus) {
            logger.Info("UI Service started");
            _mapBus = mapBus;
            _uiBus = uiBus;
        }

        public void AddNode(ControlUiEntity ui, Point p) {
            var targetIdx = UIs.FindIndex(e => e == ui);
            var req = new AddNodeRequest {
                UiId = ui.Id,
                Value = p
            };

            _uiBus.Handle(req, out ControlUiEntity output);

            UIs[targetIdx] = output;

            logger.Info("Added node {0}", output.Id);
        }

        public void RemoveNode(ControlUiEntity ui, int nodeId) {
            var targetIdx = UIs.FindIndex(e => e == ui);
            var req = new DeleteNodeRequest {
                UiId = ui.Id,
                NodeId = nodeId
            };

            _uiBus.Handle(req, out bool _);

            var req1 = new ListControlUiRequest {
                Id = ui.Id
            };

            _uiBus.Handle(req1, out ui);

            UIs[targetIdx] = ui;

            logger.Info("Removed node {0}", ui.Id);
        }

        public void MoveNode() {
            throw new NotImplementedException();
        }

        public void AssignValues() {
            throw new NotImplementedException();
        }

        public ControlUiEntity AddUi() {
            var req = new AddControlUiRequest();

            _uiBus.Handle(req, out ControlUiEntity output);
            UIs.Add(output);

            logger.Info("Added UI {0}", output.Id);

            return output;
        }

        public void RemoveUi(ControlUiEntity ui) {
            var req = new DeleteControlUiRequest {
                Id = ui.Id
            };

            _uiBus.Handle(req, out bool _);

            UIs.Remove(ui);
        }

        public NLinearMapEntity AddMap() {
            var req = new AddNLinearMapRequest();

            _mapBus.Handle(req, out NLinearMapEntity output);

            return output;
        }

        public NLinearMapEntity[] ListMaps() {
            var req = new ListNLinearMapRequest();

            _mapBus.Handle(req, out NLinearMapEntity[] output);

            return output;
        }

        public void BindMotorsToUi(ref NLinearMapEntity map, InPlug[] inPlugs, OutPlug[] outPlugs) {
            var req = new UpdateNLinearMapRequest {
                Id = map.Id,
                PropertyType = "UpdateInputs",
                PropertyValue = inPlugs,
            };

            _mapBus.Handle(req, out map);

            req = new UpdateNLinearMapRequest {
                Id = map.Id,
                PropertyType = "UpdateOutputs",
                PropertyValue = outPlugs,
            };

            _mapBus.Handle(req, out map);
        }
    }
}
