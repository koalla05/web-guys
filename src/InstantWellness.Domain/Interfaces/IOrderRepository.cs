using InstantWellness.Domain;

namespace InstantWellness.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? orderIdSearch = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? county = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        decimal? minTaxRate = null,
        decimal? maxTaxRate = null,
        double? minLat = null,
        double? maxLat = null,
        double? minLon = null,
        double? maxLon = null,
        CancellationToken cancellationToken = default);
}
