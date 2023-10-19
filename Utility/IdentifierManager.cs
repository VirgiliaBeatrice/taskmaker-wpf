using System;
using System.Collections.Generic;
using System.Linq;

namespace taskmaker_wpf.Utility {
    public interface IIdentifierManager {
        void Add(IIdentifiable entity);
        IEnumerable<IIdentifiable> GetAll();
        IIdentifiable GetById(int id);
        void InsertAt(int index, IIdentifiable entity);
        void Remove(int id);
        void ResetIDs();
    }

    public interface IIdentifiable {
        int Id { get; set; }
    }

    public class BaseIdentifierManager : IIdentifierManager {
        private List<IIdentifiable> entities = new List<IIdentifiable>();
        private int nextId = 1;

        // Add a new entity
        public void Add(IIdentifiable entity) {
            entity.Id = nextId++;
            entities.Add(entity);
        }

        // Remove an entity by ID
        public void Remove(int id) {
            var entity = entities.FirstOrDefault(e => e.Id == id);
            if (entity != null) {
                entities.Remove(entity);
            }
        }

        // Insert an entity at a specific index
        public void InsertAt(int index, IIdentifiable entity) {
            if (index < 0 || index > entities.Count) throw new ArgumentOutOfRangeException(nameof(index));
            entity.Id = nextId++;
            entities.Insert(index, entity);
        }

        // Get an entity by ID
        public IIdentifiable GetById(int id) {
            return entities.FirstOrDefault(e => e.Id == id);
        }

        // Get all entities
        public IEnumerable<IIdentifiable> GetAll() {
            return entities;
        }

        // Reset IDs (this can be useful in some scenarios where you want to compact the IDs)
        public void ResetIDs() {
            int id = 1;
            foreach (var entity in entities) {
                entity.Id = id++;
            }
            nextId = id;
        }
    }
}