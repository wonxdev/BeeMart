using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.Models.DTOs.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

/// <summary>
/// Master data endpoints for products (<c>GET /api/v1/products/search</c>).
/// </summary>
[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly BeeMartDbContext _dbContext;

    public ProductsController(BeeMartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Searches active in-stock products with pagination.
    /// Accessible by authenticated <c>Admin</c> and <c>Cashier</c> users.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 1;
        }
        else if (pageSize > 50)
        {
            pageSize = 50;
        }

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Stsrc && p.Stock > 0);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(trimmedSearch));
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductSearchItemResponse
            {
                IdProduct = p.IdProduct,
                ProductName = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                ProductImage = null
            })
            .ToListAsync();

        var responseData = new ProductSearchResponse
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return Ok(ApiResponse<ProductSearchResponse>.Success(responseData));
    }
}
