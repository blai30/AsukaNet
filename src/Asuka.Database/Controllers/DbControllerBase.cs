using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka.Database.Controllers
{
    public abstract class DbControllerBase<T> where T : DbContext
    {
        protected readonly IServiceScopeFactory _scopeFactory;

        protected T Context
        {
            get
            {
                var scope = _scopeFactory.CreateScope();
                return scope.ServiceProvider.GetRequiredService<T>();
            }
        }

        public DbControllerBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
    }
}
