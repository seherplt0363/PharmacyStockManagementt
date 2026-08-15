using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyStock.Data;
using pharmacystock.Models;
using pharmacystock.Services.Interfaces;

namespace pharmacystock.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseOrder>> GetAllOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PurchaseOrder> CreateOrderFromDraftAsync(int supplierId, List<(int productId, int quantity, decimal unitPrice)> items)
        {
            var order = new PurchaseOrder
            {
                OrderCode = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                SupplierId = supplierId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Ordered, // Mail atıldığı an 'Sipariş Verildi' kabul ediyoruz
                TotalAmount = items.Sum(x => x.quantity * x.unitPrice)
            };

            foreach (var item in items)
            {
                order.OrderItems.Add(new PurchaseOrderItem
                {
                    ProductId = item.productId,
                    Quantity = item.quantity,
                    UnitPrice = item.unitPrice
                });
            }

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.PurchaseOrders
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == orderId);

            if (order == null) return false;

            // DİKKAT: İş Mantığı (Business Logic)
            // Eğer sipariş 'Teslim Alındı' (Delivered) yapıldıysa ve önceden Teslim Alınmadıysa stokları güncelle!
            if (newStatus == OrderStatus.Delivered && order.Status != OrderStatus.Delivered)
            {
                order.DeliveryDate = DateTime.Now;

                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // STOK MİKTARINI OTOMATİK ARTIR
                        product.CurrentStock += item.Quantity; // Sizin Product sınıfındaki stok alanınızın adı (örn: Stock vs.)
                    }
                }
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}