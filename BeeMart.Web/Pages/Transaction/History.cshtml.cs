using System.Globalization;
using System.Security.Claims;
using BeeMart.Web.Models.Transaction;
using BeeMart.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Transaction;

/// <summary>
/// PageModel for the Role-Aware Transaction History screen (<c>/Transaction/History</c>, <c>FEAT-HIST-01</c> &amp; <c>FEAT-HIST-02</c>).
/// Accessible by both <c>Admin</c> and <c>Cashier</c> roles (<c>[Authorize]</c>).
/// <c>Cashier</c> users see only their own transactions with no cashier filter selector;
/// <c>Admin</c> users see transactions across all cashiers and can filter by a specific cashier.
/// All data operations flow exclusively through <see cref="IApiClient"/> (<c>NFR-ARCH-001</c>, <c>NFR-ARCH-002</c>).
/// </summary>
[Authorize]
public class HistoryModel : PageModel
{
    private static readonly IReadOnlyList<CashierFilterOptionViewModel> DefaultSeededCashiers =
        new List<CashierFilterOptionViewModel>
        {
            new() { CashierId = "76985", CashierName = "Steven", Username = "steven" },
            new() { CashierId = "66859", CashierName = "Gabri", Username = "gabri" },
            new() { CashierId = "55794", CashierName = "Mona", Username = "mona" },
            new() { CashierId = "78886", CashierName = "Felix", Username = "felix" },
            new() { CashierId = "55356", CashierName = "Gobuta", Username = "gobuta" }
        };

    private readonly IApiClient _apiClient;

    public HistoryModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string CurrentUserId { get; private set; } = string.Empty;

    public string CurrentUsername { get; private set; } = string.Empty;

    public string CurrentFullName { get; private set; } = string.Empty;

    public string CurrentRole { get; private set; } = string.Empty;

    public bool IsAdmin => string.Equals(CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase)
                           || User.IsInRole("Admin");

    public bool IsCashier => string.Equals(CurrentRole, "Cashier", StringComparison.OrdinalIgnoreCase)
                             || User.IsInRole("Cashier");

    [BindProperty(SupportsGet = true, Name = "startDate")]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true, Name = "endDate")]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true, Name = "cashierId")]
    public string? CashierId { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "pageSize")]
    public int PageSize { get; set; } = 10;

    public TransactionHistoryViewModel ViewModel { get; private set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        PopulateUserIdentity();

        if (CurrentPage < 1)
        {
            CurrentPage = 1;
        }

        if (PageSize < 1)
        {
            PageSize = 10;
        }
        else if (PageSize > 50)
        {
            PageSize = 50;
        }

        // Cashiers cannot filter by other cashiers; clear any query-string cashierId if not Admin
        var effectiveCashierId = IsAdmin && !string.IsNullOrWhiteSpace(CashierId)
            ? CashierId.Trim()
            : null;

        if (!IsAdmin)
        {
            CashierId = null;
        }

        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value.Date > EndDate.Value.Date)
        {
            ErrorMessage = "Tanggal mulai (Start Date) tidak boleh lebih besar dari tanggal akhir (End Date).";
        }

        var historyEndpoint = BuildHistoryEndpoint(
            StartDate,
            EndDate,
            effectiveCashierId,
            CurrentPage,
            PageSize);

        var apiResponse = await _apiClient.GetAsync<TransactionHistoryResponse>(historyEndpoint, cancellationToken);

        var items = new List<TransactionHistoryItemViewModel>();
        var totalItems = 0;
        var totalPages = 0;
        var resolvedPage = CurrentPage;
        var resolvedPageSize = PageSize;

        if (apiResponse.IsSuccess && apiResponse.Data is not null)
        {
            items = apiResponse.Data.Items ?? new List<TransactionHistoryItemViewModel>();
            totalItems = apiResponse.Data.TotalItems;
            totalPages = apiResponse.Data.TotalPages;
            resolvedPage = apiResponse.Data.Page > 0 ? apiResponse.Data.Page : CurrentPage;
            resolvedPageSize = apiResponse.Data.PageSize > 0 ? apiResponse.Data.PageSize : PageSize;
        }
        else if (string.IsNullOrWhiteSpace(ErrorMessage) && !string.IsNullOrWhiteSpace(apiResponse.Message))
        {
            ErrorMessage = apiResponse.Message;
        }

        var cashierOptions = new List<CashierFilterOptionViewModel>();
        if (IsAdmin)
        {
            cashierOptions = BuildCashierFilterOptions(items, effectiveCashierId);
        }

        ViewModel = new TransactionHistoryViewModel
        {
            StartDate = StartDate,
            EndDate = EndDate,
            CashierId = effectiveCashierId,
            Page = resolvedPage,
            PageSize = resolvedPageSize,
            Items = items,
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = resolvedPage,
            CashierOptions = cashierOptions
        };

        CurrentPage = resolvedPage;
        PageSize = resolvedPageSize;

        return Page();
    }

    private void PopulateUserIdentity()
    {
        CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("idUser")?.Value
                        ?? string.Empty;

        CurrentUsername = User.Identity?.Name
                          ?? User.FindFirst("username")?.Value
                          ?? "User";

        CurrentFullName = User.FindFirst("fullName")?.Value
                          ?? CurrentUsername;

        CurrentRole = User.FindFirst(ClaimTypes.Role)?.Value
                      ?? User.FindFirst("role")?.Value
                      ?? string.Empty;
    }

    private static string BuildHistoryEndpoint(
        DateTime? startDate,
        DateTime? endDate,
        string? cashierId,
        int page,
        int pageSize)
    {
        var queryParts = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (startDate.HasValue)
        {
            var formattedStart = startDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            queryParts.Add($"startDate={Uri.EscapeDataString(formattedStart)}");
        }

        if (endDate.HasValue)
        {
            var formattedEnd = endDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            queryParts.Add($"endDate={Uri.EscapeDataString(formattedEnd)}");
        }

        if (!string.IsNullOrWhiteSpace(cashierId))
        {
            queryParts.Add($"cashierId={Uri.EscapeDataString(cashierId.Trim())}");
        }

        return "transactions/history?" + string.Join("&", queryParts);
    }

    private static List<CashierFilterOptionViewModel> BuildCashierFilterOptions(
        IEnumerable<TransactionHistoryItemViewModel> currentItems,
        string? selectedCashierId)
    {
        var map = new Dictionary<string, CashierFilterOptionViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var seeded in DefaultSeededCashiers)
        {
            map[seeded.CashierId] = new CashierFilterOptionViewModel
            {
                CashierId = seeded.CashierId,
                CashierName = seeded.CashierName,
                Username = seeded.Username
            };
        }

        foreach (var item in currentItems)
        {
            if (!string.IsNullOrWhiteSpace(item.CashierId) && !map.ContainsKey(item.CashierId))
            {
                map[item.CashierId] = new CashierFilterOptionViewModel
                {
                    CashierId = item.CashierId,
                    CashierName = !string.IsNullOrWhiteSpace(item.CashierName) ? item.CashierName : item.CashierId,
                    Username = string.Empty
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(selectedCashierId) && !map.ContainsKey(selectedCashierId))
        {
            map[selectedCashierId] = new CashierFilterOptionViewModel
            {
                CashierId = selectedCashierId,
                CashierName = $"Kasir #{selectedCashierId}",
                Username = string.Empty
            };
        }

        return map.Values
            .OrderBy(c => c.CashierName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
