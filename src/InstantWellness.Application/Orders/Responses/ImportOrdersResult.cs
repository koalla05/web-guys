namespace InstantWellness.Application.Orders.Responses;

public record ImportOrdersResult(
    int ImportedCount,
    int FailedCount,
    IReadOnlyList<string> Errors);
