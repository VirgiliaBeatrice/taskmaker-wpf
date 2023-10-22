using System.Collections.Generic;
using System.Linq;
using taskmaker_wpf.Entity;

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
}
