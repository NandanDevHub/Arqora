namespace Arqora.Domain.Enums;

/// <summary>
/// Tiers drive which material rate column (Basic / Standard / Premium)
/// is used when generating a quotation. Encoding them as an enum makes
/// the pricing logic type-safe and avoids magic strings.
/// </summary>
public enum QuotationTier
{
    Basic,
    Standard,
    Premium
}
