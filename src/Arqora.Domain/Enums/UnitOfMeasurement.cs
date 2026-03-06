namespace Arqora.Domain.Enums;

/// <summary>
/// Different materials and labour are priced in different units.
/// Having a domain-level enum keeps unit conversions and validation
/// inside the domain rather than scattered across the UI or database.
/// </summary>
public enum UnitOfMeasurement
{
    SquareFeet,
    SquareMeter,
    RunningFeet,
    Piece,
    Set,
    Lot,
    Lumpsum
}
