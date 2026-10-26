using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shop.Api.Contracts;
using Shop.Api.Entities;
using Shop.Api.Infrastructure;

namespace Shop.Api.Services;

public class OrderService(ShopDbContext db)
{
    public Task<List<OrderResponse>> GetAllAsync(CancellationToken ct) =>
        Project(db.Orders.OrderByDescending(o => o.CreatedAt)).ToListAsync(ct);

    public Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Project(db.Orders.Where(o => o.Id == id)).FirstOrDefaultAsync(ct);

    public async Task<OrderResponse> CreateAsync(OrderRequest request, CancellationToken ct)
    {
        var products = await LoadProductsAsync(request.Items, ct);

        var order = new Order
        {
            DeliveryAddress = request.DeliveryAddress.Trim(),
            CreatedAt = DateTime.UtcNow,
            Items = request.Items
                .Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = products[i.ProductId].Price
                })
                .ToList()
        };

        db.Orders.Add(order);
        await SaveAsync(ct);

        return (await GetByIdAsync(order.Id, ct))!;
    }

    public async Task<bool> UpdateAsync(Guid id, OrderRequest request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return false;

        var products = await LoadProductsAsync(request.Items, ct);
        var requestedIds = request.Items.Select(i => i.ProductId).ToHashSet();

        order.DeliveryAddress = request.DeliveryAddress.Trim();
        order.Items.RemoveAll(i => !requestedIds.Contains(i.ProductId));

        foreach (var item in request.Items)
        {
            var existing = order.Items.Find(i => i.ProductId == item.ProductId);
            if (existing is not null)
            {
                existing.Quantity = item.Quantity;
            }
            else
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = products[item.ProductId].Price
                });
            }
        }

        await SaveAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.Orders.Where(o => o.Id == id).ExecuteDeleteAsync(ct) > 0;

    private async Task<Dictionary<Guid, Product>> LoadProductsAsync(List<OrderItemRequest> items, CancellationToken ct)
    {
        var ids = items.Select(i => i.ProductId).ToList();
        if (ids.Distinct().Count() != ids.Count)
            throw new OrderValidationException("Each product can appear in an order only once.");

        var products = await db.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var missing = ids.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new OrderValidationException($"Products not found: {string.Join(", ", missing)}.");

        return products;
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new OrderValidationException("One of the products no longer exists.");
        }
    }

    private static IQueryable<OrderResponse> Project(IQueryable<Order> orders) =>
        orders.Select(o => new OrderResponse(
            o.Id,
            o.Number,
            o.DeliveryAddress,
            o.CreatedAt,
            o.Items
                .OrderBy(i => i.Product.Name)
                .Select(i => new OrderItemResponse(i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice))
                .ToList(),
            o.Items.Sum(i => i.UnitPrice * i.Quantity)));
}