using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;

namespace InstantWellness.Infrastructure;

public class OrderRepositoryInMemory : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private readonly object _lock = new();

    public Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            _orders.Add(order);
        }
        return Task.FromResult(order);
    }

    public Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var list = orders.ToList();
        lock (_lock)
        {
            _orders.AddRange(list);
        }
        return Task.FromResult<IEnumerable<Order>>(list);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(order);
        }
    }

    public Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
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
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var query = _orders.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(orderIdSearch))
                query = query.Where(o => o.Id.ToString().Contains(orderIdSearch, StringComparison.OrdinalIgnoreCase));
            if (fromDate.HasValue)
                query = query.Where(o => o.Timestamp >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(o => o.Timestamp <= toDate.Value);
            if (!string.IsNullOrWhiteSpace(county))
                query = query.Where(o => o.County != null && o.County.Contains(county, StringComparison.OrdinalIgnoreCase));
            if (minAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= maxAmount.Value);
            if (minTaxRate.HasValue)
                query = query.Where(o => o.CompositeTaxRate >= minTaxRate.Value);
            if (maxTaxRate.HasValue)
                query = query.Where(o => o.CompositeTaxRate <= maxTaxRate.Value);
            if (minLat.HasValue)
                query = query.Where(o => o.Latitude >= minLat.Value);
            if (maxLat.HasValue)
                query = query.Where(o => o.Latitude <= maxLat.Value);
            if (minLon.HasValue)
                query = query.Where(o => o.Longitude >= minLon.Value);
            if (maxLon.HasValue)
                query = query.Where(o => o.Longitude <= maxLon.Value);

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(o => o.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<(IEnumerable<Order>, int)>((items, totalCount));
        }
    }
}
