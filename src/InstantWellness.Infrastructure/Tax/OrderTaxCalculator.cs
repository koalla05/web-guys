using InstantWellness.Application.Tax;
using InstantWellness.Domain;
using InstantWellness.Domain.Interfaces;

namespace InstantWellness.Infrastructure.Tax;

public class OrderTaxCalculator : IOrderTaxCalculator
{
    private readonly ITaxService _taxService;
    private readonly IOrderRepository _orderRepository;

    public OrderTaxCalculator(ITaxService taxService, IOrderRepository orderRepository)
    {
        _taxService = taxService;
        _orderRepository = orderRepository;
    }

    public async Task<Order> EnsureTaxCalculatedAsync(Order order, CancellationToken cancellationToken = default)
    {
        // If tax is already calculated, return as is
        if (order.HasTaxCalculated())
        {
            return order;
        }

        // Calculate tax
        var taxCalculation = await _taxService.CalculateTaxAsync(
            order.Latitude,
            order.Longitude,
            order.Subtotal,
            cancellationToken);

        // Update order with tax information
        order.CompositeTaxRate = taxCalculation.CompositeTaxRate;
        order.TaxAmount = taxCalculation.TaxAmount;
        order.StateRate = taxCalculation.StateRate;
        order.CountyRate = taxCalculation.CountyRate;
        order.CityRate = taxCalculation.CityRate;
        order.SpecialRates = taxCalculation.SpecialRates;
        order.State = taxCalculation.State;
        order.County = taxCalculation.County;
        order.City = taxCalculation.City;
        order.SpecialJurisdiction = string.Join(", ", taxCalculation.SpecialJurisdictions);

        // Persist the calculated tax
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return order;
    }

    public async Task<List<Order>> EnsureTaxCalculatedAsync(List<Order> orders, CancellationToken cancellationToken = default)
    {
        var tasks = orders.Select(order => EnsureTaxCalculatedAsync(order, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
