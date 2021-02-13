using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka.Database.Controllers
{
    public abstract class DbControllerBase<T> where T : DbContext
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private T _context;

        /// <summary>
        /// Do not use using statements, dependency injection will handle disposing.
        /// </summary>
        protected T Context
        {
            get
            {
                if (_context == null)
                {
                    var scope = _scopeFactory.CreateScope();
                    _context = scope.ServiceProvider.GetRequiredService<T>();
                }

                return _context;
            }
        }

        public DbControllerBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
    }
}
