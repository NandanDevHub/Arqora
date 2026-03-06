namespace Arqora.Application.Interfaces;

/// <summary>
/// Generates unique, human-readable quotation numbers (e.g. "ARQ-2026-0001").
/// This is an Infrastructure concern because it requires querying the database
/// for the last-used number — keeping it behind an interface lets the
/// Application layer remain database-agnostic.
/// </summary>
public interface IQuotationNumberGenerator
{
    Task<string> GenerateAsync();
}
