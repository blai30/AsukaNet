using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asuka.Database.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<int> AddAsync(TEntity entity);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> RemoveAsync(int id);
        Task<int> RemoveAsync(TEntity entity);
    }
}
