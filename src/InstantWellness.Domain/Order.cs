namespace InstantWellness.Domain;

public class Order
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime Timestamp { get; set; }

    // Tax fields - placeholders for future implementation
    public decimal CompositeTaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount => Subtotal + TaxAmount;
}
