using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Asuka.Database.Models;
using Dapper;

namespace Asuka.Database.Repositories
{
    internal class TagRepository : RepositoryBase, ITagRepository
    {
        public TagRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public async Task<Tag> GetAsync(int id)
        {
            var sql = @"SELECT * FROM tags WHERE id = @id;";
            var row = await Connection.QueryFirstAsync<Tag>(sql, new { Id = id }, Transaction);
            return row;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            var sql = @"SELECT * FROM tags;";
            var rows = await Connection.QueryAsync<Tag>(sql, transaction: Transaction);
            return rows;
        }

        public async Task<int> AddAsync(Tag entity)
        {
            var sql = @"INSERT INTO tags (name, content, user_id, guild_id) VALUES (@Name, @Content, @UserId, @GuildId);";
            var affectedRows = await Connection.ExecuteAsync(sql, entity, Transaction);
            return affectedRows;
        }

        public async Task<int> UpdateAsync(Tag entity)
        {
            var sql = @"UPDATE tags SET content = @Content WHERE name = @Name;";
            var affectedRows = await Connection.ExecuteAsync(sql, entity, Transaction);
            return affectedRows;
        }

        public async Task<int> RemoveAsync(int id)
        {
            var sql = @"DELETE FROM tags WHERE id = @Id;";
            var affectedRows = await Connection.ExecuteAsync(sql, new { Id = id }, Transaction);
            return affectedRows;
        }

        public async Task<int> RemoveAsync(Tag entity)
        {
            return await RemoveAsync(entity.Id);
        }

        public async Task<Tag> GetByNameAsync(string name)
        {
            var sql = @"SELECT * FROM tags WHERE name = @Name";
            var row = await Connection.QueryFirstOrDefaultAsync<Tag>(sql, new {Name = name}, Transaction);
            return row;
        }
    }
}
