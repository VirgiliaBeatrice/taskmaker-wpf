using AutoMapper.Features;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BaseEntityManager<T> where T : BaseEntity {
        protected readonly Dictionary<int, T> entities = new Dictionary<int, T>();
        private int nextId = 1;

        public virtual T Create(T entity) {
            entity.Id = nextId++;
            entities[entity.Id] = entity;
            return entity;
        }

        public virtual T Read(int id) {
            entities.TryGetValue(id, out var entity);
            return entity;
        }

        public virtual bool Update(T entity) {
            if (entities.ContainsKey(entity.Id)) {
                entities[entity.Id] = entity;

                return true;
            }
            return false;
        }

        public virtual bool Delete(int id) {
            return entities.Remove(id);
        }

        public virtual T[] GetAll() {
            return entities.Values.ToArray();
        }
    }

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
