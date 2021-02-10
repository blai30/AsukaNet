using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Database.Models;
using Dommel;

namespace Asuka.Database.Repositories
{
    public class TagRepository : RepositoryBase<Tag>
    {
        public TagRepository(IDbConnection db) : base(db)
        {
        }

        public async Task<Tag> GetByNameAsync(string name)
        {
            var tags = Db.Select<Tag>(t => t.Name == name);
            var tag = tags.FirstOrDefault();
            return tag;
        }
    }
}
