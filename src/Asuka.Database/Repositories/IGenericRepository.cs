using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asuka.Database.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<int> AddAsync(TEntity entity);
        Task<int> DeleteAsync(int id);
        Task<int> UpdateAsync(TEntity entity);
    }
}
