namespace Arqora.Domain.Enums;

/// <summary>
/// Represents the lifecycle of a quotation. This enum acts as a simple
/// state machine — transitions (e.g. Draft → Sent → Accepted) can be
/// validated in domain services to prevent illegal state changes.
/// </summary>
public enum QuotationStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired
}
