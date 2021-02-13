using System;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka.Database.Controllers
{
    public class AsukaDbController : DbControllerBase<AsukaDbContext>
    {
        public AsukaDbController(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        /// <summary>
        /// Inserts a new tag entity into the database.
        /// </summary>
        /// <param name="tag">The entity to be inserted.</param>
        /// <returns>The entity that was inserted.</returns>
        public async Task<Tag> AddAsync(Tag tag)
        {
            await Context.Tags.AddAsync(tag);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return tag;
        }

        /// <summary>
        /// Gets the content of a tag identified by name.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>Tag content</returns>
        public async Task<string> GetTagAsync(string tagName)
        {
            // Get content from tag retrieved by name.
            var content = await Context.Tags.AsQueryable()
                .Where(t => t.Name == tagName)
                .Select(t => t.Content)
                .FirstOrDefaultAsync();

            return content;
        }
    }
}
