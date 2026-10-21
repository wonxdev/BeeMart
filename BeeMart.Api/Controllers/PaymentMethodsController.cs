using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.Models.DTOs.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

/// <summary>
/// Master data endpoints for payment methods (<c>GET /api/v1/payment-methods</c>).
/// </summary>
[ApiController]
[Route("api/v1/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly BeeMartDbContext _dbContext;

    public PaymentMethodsController(BeeMartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves all active payment methods (<c>API-PAY-01</c>).
    /// Accessible by authenticated <c>Admin</c> and <c>Cashier</c> users.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var paymentMethods = await _dbContext.PaymentMethods
            .AsNoTracking()
            .Where(pm => pm.Stsrc)
            .OrderBy(pm => pm.IdPaymentMethod)
            .Select(pm => new PaymentMethodResponse
            {
                IdPaymentMethod = pm.IdPaymentMethod.ToString(),
                PaymentMethodName = pm.MethodName
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PaymentMethodResponse>>.Success(paymentMethods));
    }
}
