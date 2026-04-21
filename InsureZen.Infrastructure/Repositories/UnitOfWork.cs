using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using InsureZen.Core.Entities;
using InsureZen.Core.Interfaces;
using InsureZen.Infrastructure.Data;

namespace InsureZen.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        
        public IRepository<Claim> Claims { get; private set; }
        public IRepository<User> Users { get; private set; }
        public IRepository<MakerReview> MakerReviews { get; private set; }
        public IRepository<CheckerReview> CheckerReviews { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Claims = new Repository<Claim>(_context);
            Users = new Repository<User>(_context);
            MakerReviews = new Repository<MakerReview>(_context);
            CheckerReviews = new Repository<CheckerReview>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}