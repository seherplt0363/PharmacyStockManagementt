using Microsoft.EntityFrameworkCore.Storage;
using PharmacyStock.DataAccess.Context;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.DataAccess.Repositories.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IGenericRepository<Product> Products { get; }

        public IGenericRepository<Category> Categories { get; }

        public IGenericRepository<Brand> Brands { get; }

        public IGenericRepository<StockTransaction> StockTransactions { get; }

        public IGenericRepository<Supplier> Suppliers { get; }

        public IGenericRepository<PurchaseOrder> PurchaseOrders { get; }

        public IGenericRepository<PurchaseOrderItem> PurchaseOrderItems { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Products = new GenericRepository<Product>(_context);
            Categories = new GenericRepository<Category>(_context);
            Brands = new GenericRepository<Brand>(_context);
            StockTransactions = new GenericRepository<StockTransaction>(_context);
            Suppliers = new GenericRepository<Supplier>(_context);
            PurchaseOrders = new GenericRepository<PurchaseOrder>(_context);
            PurchaseOrderItems = new GenericRepository<PurchaseOrderItem>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}