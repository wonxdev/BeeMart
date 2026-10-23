using System.Globalization;
using System.Security.Claims;
using BeeMart.Web.Models.Transaction;
using BeeMart.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Transaction;

/// <summary>
/// PageModel for the role-aware Transaction History screen (<c>/Transaction/History</c>).
/// Cashiers view only their own transactions, while Admins can view and filter across all cashiers.
/// </summary>
[Authorize]
public class HistoryModel : PageModel
{
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
            cashierOptions = await ResolveCashierFilterOptionsAsync(items, effectiveCashierId, cancellationToken);
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

    private async Task<List<CashierFilterOptionViewModel>> ResolveCashierFilterOptionsAsync(
        IReadOnlyCollection<TransactionHistoryItemViewModel> currentItems,
        string? selectedCashierId,
        CancellationToken cancellationToken)
    {
        var map = new Dictionary<string, CashierFilterOptionViewModel>(StringComparer.OrdinalIgnoreCase);

        void AddItems(IEnumerable<TransactionHistoryItemViewModel> source)
        {
            foreach (var item in source)
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
        }

        AddItems(currentItems);

        // When filtered or paged, also query recent unfiltered history from the API to keep all active cashiers selectable
        if (!string.IsNullOrWhiteSpace(selectedCashierId) || StartDate.HasValue || EndDate.HasValue || CurrentPage > 1)
        {
            var lookupResponse = await _apiClient.GetAsync<TransactionHistoryResponse>(
                "transactions/history?page=1&pageSize=50",
                cancellationToken);

            if (lookupResponse.IsSuccess && lookupResponse.Data?.Items is not null)
            {
                AddItems(lookupResponse.Data.Items);
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
