using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dommel;

namespace Asuka.Database.Repositories
{
    public abstract class RepositoryBase<TEntity> where TEntity : class
    {
        protected readonly IDbConnection Db;

        protected RepositoryBase(IDbConnection db)
        {
            Db = db;
        }

        public virtual async Task<TEntity> GetAsync(object id)
        {
            var entity = await Db.GetAsync<TEntity>(id);
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var entities = await Db.GetAllAsync<TEntity>();
            return entities;
        }

        public virtual async Task<object> InsertAsync(TEntity entity)
        {
            var id = await Db.InsertAsync(entity);
            return id;
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            var success = await Db.UpdateAsync(entity);
            return success;
        }

        public virtual async Task<bool> DeleteAsync(TEntity entity)
        {
            var success = await Db.DeleteAsync(entity);
            return success;
        }

        public virtual async Task<int> DeleteAllAsync()
        {
            var success = await Db.DeleteAllAsync<TEntity>();
            return success;
        }
    }
}
