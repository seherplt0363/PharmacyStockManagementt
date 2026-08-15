using PharmacyStock.Entities.Models;

namespace PharmacyStock.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Product> Products { get; }

        IGenericRepository<Category> Categories { get; }

        IGenericRepository<Brand> Brands { get; }

        IGenericRepository<StockTransaction> StockTransactions { get; }

        IGenericRepository<Supplier> Suppliers { get; }

        IGenericRepository<PurchaseOrder> PurchaseOrders { get; }

        IGenericRepository<PurchaseOrderItem> PurchaseOrderItems { get; }

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}