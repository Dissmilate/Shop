using Microsoft.AspNetCore.Mvc;
using Shop.Api.Contracts;
using Shop.Api.Services;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderService ordersService) : ControllerBase
{
    [HttpGet]
    public Task<List<OrderResponse>> GetAll(CancellationToken ct) => ordersService.GetAllAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var order = await ordersService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : order;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(OrderRequest request, CancellationToken ct)
    {
        try
        {
            var order = await ordersService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (OrderValidationException ex)
        {
            return InvalidItems(ex);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, OrderRequest request, CancellationToken ct)
    {
        try
        {
            return await ordersService.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
        }
        catch (OrderValidationException ex)
        {
            return InvalidItems(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await ordersService.DeleteAsync(id, ct) ? NoContent() : NotFound();

    private ActionResult InvalidItems(OrderValidationException ex)
    {
        ModelState.AddModelError(nameof(OrderRequest.Items), ex.Message);
        return ValidationProblem(ModelState);
    }
}