using System.Threading.Tasks;
using Asuka.Database.Models;

namespace Asuka.Database.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag> GetByNameAsync(string name);
    }
}
