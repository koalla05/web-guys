using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;
using InstantWellness.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InstantWellness.Infrastructure;

public class OrderRepositoryEf : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepositoryEf(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _db.Orders.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default)
    {
        var list = orders.ToList();
        await _db.Orders.AddRangeAsync(list, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return list;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Orders.FindAsync([id], cancellationToken);
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(
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
        var query = _db.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(orderIdSearch))
            query = query.Where(o => EF.Functions.ILike(o.Id.ToString(), $"%{orderIdSearch}%"));
        if (fromDate.HasValue)
            query = query.Where(o => o.Timestamp >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.Timestamp <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(county))
            query = query.Where(o => o.County != null && EF.Functions.ILike(o.County, $"%{county}%"));
        if (minAmount.HasValue)
            query = query.Where(o => o.Subtotal + (o.TaxAmount ?? 0) >= minAmount.Value);
        if (maxAmount.HasValue)
            query = query.Where(o => o.Subtotal + (o.TaxAmount ?? 0) <= maxAmount.Value);
        if (minTaxRate.HasValue)
            query = query.Where(o => o.CompositeTaxRate.HasValue && o.CompositeTaxRate >= minTaxRate.Value);
        if (maxTaxRate.HasValue)
            query = query.Where(o => o.CompositeTaxRate.HasValue && o.CompositeTaxRate <= maxTaxRate.Value);
        if (minLat.HasValue)
            query = query.Where(o => o.Latitude >= minLat.Value);
        if (maxLat.HasValue)
            query = query.Where(o => o.Latitude <= maxLat.Value);
        if (minLon.HasValue)
            query = query.Where(o => o.Longitude >= minLon.Value);
        if (maxLon.HasValue)
            query = query.Where(o => o.Longitude <= maxLon.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
