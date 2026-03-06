namespace Arqora.Domain.Enums;

/// <summary>
/// Categorises every line item into a trade/discipline. This lets the
/// quotation group costs by category (e.g. total Furniture cost vs.
/// total Electrical cost), which is how real interior estimates are
/// presented to homeowners.
/// </summary>
public enum WorkCategory
{
    Furniture,
    Electrical,
    Plumbing,
    Painting,
    Flooring,
    FalseCeiling,
    Lighting,
    Decor,
    CivilWork,
    Modular
}
