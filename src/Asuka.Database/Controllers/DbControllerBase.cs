using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka.Database.Controllers
{
    public abstract class DbControllerBase<T> where T : DbContext
    {
        protected readonly IServiceScopeFactory _scopeFactory;

        public DbControllerBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
    }
}
