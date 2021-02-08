using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Asuka.Database.Models;
using Dapper;

namespace Asuka.Database.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly IDbConnection _db;

        public TagRepository(IDbConnection db) {
            _db = db;
        }

        public async Task<Tag> GetByIdAsync(int id)
        {
            var query = @"select * from tag where id = @id";
            return await _db.QueryFirstOrDefaultAsync(query);
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            var query = @"select * from tag";
            return await _db.QueryAsync<Tag>(query);
        }

        public async Task<int> AddAsync(Tag entity)
        {
            throw new System.NotImplementedException();
        }

        public async  Task<int> DeleteAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> UpdateAsync(Tag entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
