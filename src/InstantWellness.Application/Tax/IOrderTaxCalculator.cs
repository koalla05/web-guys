using InstantWellness.Domain;

namespace InstantWellness.Application.Tax;

public interface IOrderTaxCalculator
{
    /// <summary>
    /// Ensures the order has tax calculated. If not, calculates and updates the order.
    /// </summary>
    Task<Order> EnsureTaxCalculatedAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures all orders in the list have tax calculated.
    /// </summary>
    Task<List<Order>> EnsureTaxCalculatedAsync(List<Order> orders, CancellationToken cancellationToken = default);
}
