using AutoMapper.Features;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using System;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services {

    public class UiUpdatedMessage {
        public ControlUiEntity Entity { get; set; }
    }

    public class UIService : BaseEntityManager<ControlUiEntity> {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        //private readonly NLinearMapInteractorBus _mapBus;
        //private readonly ControlUiInteractorBus _uiBus;

        public UIService() {

            logger.Info("UI Service started");
        }

        public override bool Update(ControlUiEntity entity) {
            var result = base.Update(entity);
            //WeakReferenceMessenger.Default.Send(new UiUpdatedMessage() { Entity = entity });
            return result;
        }
    }
}
