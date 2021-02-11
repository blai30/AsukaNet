using System;
using Asuka.Database.Repositories;

namespace Asuka.Database
{
    public interface IUnitOfWork : IDisposable
    {
        ITagRepository Tags { get; }

        void Complete();
    }
}
