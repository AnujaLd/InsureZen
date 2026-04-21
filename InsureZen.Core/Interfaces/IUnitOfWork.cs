using System;
using System.Threading.Tasks;
using InsureZen.Core.Entities;

namespace InsureZen.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Claim> Claims { get; }
        IRepository<User> Users { get; }
        IRepository<MakerReview> MakerReviews { get; }
        IRepository<CheckerReview> CheckerReviews { get; }
        
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}