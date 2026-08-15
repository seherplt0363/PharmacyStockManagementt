using Microsoft.EntityFrameworkCore;
using PharmacyStock.Business.Interfaces;
using PharmacyStock.DataAccess.Repositories.Interfaces;
using PharmacyStock.DTO.PurchaseOrderDTO;
using PharmacyStock.Entities.Enum;
using PharmacyStock.Entities.Models;

namespace PharmacyStock.Business.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================================
        // TÜM SİPARİŞLERİ GETİR
        // =========================================================
        public async Task<List<PurchaseOrderListDto>> GetAllOrdersAsync()
        {
            return await _unitOfWork.PurchaseOrders
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new PurchaseOrderListDto
                {
                    Id = x.Id,

                    OrderCode = x.OrderCode,

                    SupplierId = x.SupplierId,

                    SupplierName = x.Supplier != null
                        ? x.Supplier.Name
                        : string.Empty,

                    OrderDate = x.OrderDate,

                    DeliveryDate = x.DeliveryDate,

                    Status = x.Status,

                    TotalAmount = x.TotalAmount,

                    OrderItems = x.OrderItems
                        .Select(item => new PurchaseOrderItemDto
                        {
                            Id = item.Id,

                            ProductId = item.ProductId,

                            ProductName = item.Product != null
                                ? item.Product.Name
                                : string.Empty,

                            Quantity = item.Quantity,

                            UnitPrice = item.UnitPrice
                        })
                        .ToList()
                })
                .ToListAsync();
        }


        // =========================================================
        // ID'YE GÖRE SİPARİŞ GETİR
        // =========================================================
        public async Task<PurchaseOrderListDto?> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.PurchaseOrders
                .GetAll()
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                .Where(x => x.Id == id)
                .Select(x => new PurchaseOrderListDto
                {
                    Id = x.Id,

                    OrderCode = x.OrderCode,

                    SupplierId = x.SupplierId,

                    SupplierName = x.Supplier != null
                        ? x.Supplier.Name
                        : string.Empty,

                    OrderDate = x.OrderDate,

                    DeliveryDate = x.DeliveryDate,

                    Status = x.Status,

                    TotalAmount = x.TotalAmount,

                    OrderItems = x.OrderItems
                        .Select(item => new PurchaseOrderItemDto
                        {
                            Id = item.Id,

                            ProductId = item.ProductId,

                            ProductName = item.Product != null
                                ? item.Product.Name
                                : string.Empty,

                            Quantity = item.Quantity,

                            UnitPrice = item.UnitPrice
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }


        // =========================================================
        // SİPARİŞ TASLAĞINDAN SATIN ALMA SİPARİŞİ OLUŞTUR
        // =========================================================
        public async Task<PurchaseOrderListDto?> CreateOrderFromDraftAsync(
            int supplierId,
            List<PurchaseOrderItemDto> items)
        {
            // Temel kontroller
            if (supplierId <= 0 ||
                items == null ||
                items.Count == 0)
            {
                return null;
            }


            // =====================================================
            // TEDARİKÇİ KONTROLÜ
            // =====================================================
            var supplier = await _unitOfWork.Suppliers
                .GetByIdAsync(supplierId);

            if (supplier == null)
            {
                return null;
            }


            // =====================================================
            // SİPARİŞ KALEMLERİNİ KONTROL ET
            // =====================================================
            if (items.Any(x =>
                x.ProductId <= 0 ||
                x.Quantity <= 0 ||
                x.UnitPrice <= 0))
            {
                return null;
            }


            // =====================================================
            // ÜRÜNLER VERİTABANINDA VAR MI?
            // =====================================================
            var productIds = items
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();


            var existingProductIds = await _unitOfWork.Products
                .GetAll()
                .Where(x => productIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();


            if (existingProductIds.Count != productIds.Count)
            {
                return null;
            }


            // =====================================================
            // SATIN ALMA SİPARİŞİ OLUŞTUR
            // =====================================================
            var order = new PurchaseOrder
            {
                OrderCode =
                    $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",

                SupplierId = supplierId,

                OrderDate = DateTime.Now,

                Status = OrderStatus.Ordered,

                TotalAmount = items.Sum(x =>
                    x.Quantity * x.UnitPrice)
            };


            // =====================================================
            // SİPARİŞ KALEMLERİNİ EKLE
            // =====================================================
            foreach (var item in items)
            {
                order.OrderItems.Add(
                    new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,

                        Quantity = item.Quantity,

                        UnitPrice = item.UnitPrice
                    });
            }


            // =====================================================
            // TRANSACTION BAŞLAT
            // =====================================================
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.PurchaseOrders
                    .AddAsync(order);


                await _unitOfWork.SaveChangesAsync();


                await _unitOfWork.CommitTransactionAsync();


                // =================================================
                // DTO OLARAK GERİ DÖNDÜR
                // =================================================
                return new PurchaseOrderListDto
                {
                    Id = order.Id,

                    OrderCode = order.OrderCode,

                    SupplierId = order.SupplierId,

                    SupplierName = supplier.Name,

                    OrderDate = order.OrderDate,

                    DeliveryDate = order.DeliveryDate,

                    Status = order.Status,

                    TotalAmount = order.TotalAmount,

                    OrderItems = order.OrderItems
                        .Select(item => new PurchaseOrderItemDto
                        {
                            Id = item.Id,

                            ProductId = item.ProductId,

                            ProductName = items
                                .FirstOrDefault(x =>
                                    x.ProductId == item.ProductId)?
                                .ProductName ?? string.Empty,

                            Quantity = item.Quantity,

                            UnitPrice = item.UnitPrice
                        })
                        .ToList()
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }


        // =========================================================
        // SİPARİŞ DURUMUNU GÜNCELLE
        // =========================================================
        public async Task<bool> UpdateOrderStatusAsync(
            int orderId,
            OrderStatus newStatus)
        {
            var order = await _unitOfWork.PurchaseOrders
                .GetAll()
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == orderId);


            if (order == null)
            {
                return false;
            }


            // Aynı durum tekrar gönderildiyse işlem yapma
            if (order.Status == newStatus)
            {
                return true;
            }


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // =================================================
                // TESLİM EDİLDİYSE STOKLARI ARTIR
                // =================================================
                if (newStatus == OrderStatus.Delivered &&
                    order.Status != OrderStatus.Delivered)
                {
                    order.DeliveryDate = DateTime.Now;


                    foreach (var item in order.OrderItems)
                    {
                        var product = await _unitOfWork.Products
                            .GetByIdAsync(item.ProductId);


                        if (product == null)
                        {
                            await _unitOfWork
                                .RollbackTransactionAsync();

                            return false;
                        }


                        product.CurrentStock += item.Quantity;


                        _unitOfWork.Products.Update(product);
                    }
                }


                // =================================================
                // SİPARİŞ DURUMUNU GÜNCELLE
                // =================================================
                order.Status = newStatus;


                _unitOfWork.PurchaseOrders.Update(order);


                await _unitOfWork.SaveChangesAsync();


                await _unitOfWork.CommitTransactionAsync();


                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }
    }
}