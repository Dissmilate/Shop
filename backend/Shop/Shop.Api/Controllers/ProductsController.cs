using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shop.Api.Contracts;
using Shop.Api.Entities;
using Shop.Api.Infrastructure;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<ProductResponse>> GetAll(CancellationToken ct) =>
        await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price))
            .ToListAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price))
            .FirstOrDefaultAsync(ct);

        return product is null ? NotFound() : product;
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var normalized = name.ToUpperInvariant();

        if (await db.Products.AnyAsync(p => p.NormalizedName == normalized, ct))
            return DuplicateName(name);

        var product = new Product() { Name = name, NormalizedName = normalized, Price = request.Price };
        db.Products.Add(product);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsPostgresError(ex, PostgresErrorCodes.UniqueViolation))
        {
            return DuplicateName(name);
        }

        var response = new ProductResponse(product.Id, product.Name, product.Price);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProductRequest request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null)
            return NotFound();

        var name = request.Name.Trim();
        var normalized = name.ToUpperInvariant();

        if (await db.Products.AnyAsync(p => p.NormalizedName == normalized && p.Id != id, ct))
            return DuplicateName(name);

        product.Name = name;
        product.NormalizedName = normalized;
        product.Price = request.Price;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsPostgresError(ex, PostgresErrorCodes.UniqueViolation))
        {
            return DuplicateName(name);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null)
            return NotFound();

        if (await db.OrderItems.AnyAsync(i => i.ProductId == id, ct))
            return UsedInOrders();

        db.Products.Remove(product);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsPostgresError(ex, PostgresErrorCodes.ForeignKeyViolation))
        {
            return UsedInOrders();
        }

        return NoContent();
    }

    private ObjectResult DuplicateName(string name) =>
        Problem(statusCode: StatusCodes.Status409Conflict,
            title: "Product name already exists",
            detail: $"Product with name '{name}' already exists.");

    private ObjectResult UsedInOrders() =>
        Problem(statusCode: StatusCodes.Status409Conflict,
            title: "Product is used in orders",
            detail: "Product cannot be deleted because it is used in existing orders.");

    private static bool IsPostgresError(DbUpdateException ex, string sqlState) =>
        ex.InnerException is PostgresException pg && pg.SqlState == sqlState;
}