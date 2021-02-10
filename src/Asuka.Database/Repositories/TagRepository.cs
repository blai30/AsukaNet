using System.Data;
using Asuka.Database.Models;

namespace Asuka.Database.Repositories
{
    public class TagRepository : RepositoryBase<Tag>
    {
        public TagRepository(IDbConnection db) : base(db)
        {
        }
    }
}
