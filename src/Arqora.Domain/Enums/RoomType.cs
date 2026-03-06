namespace Arqora.Domain.Enums;

/// <summary>
/// A closed set of room types that the platform supports. Using an enum
/// (instead of a free-text string) enables room-specific defaults, validation,
/// and UI dropdowns while keeping the domain model strongly typed.
/// </summary>
public enum RoomType
{
    Kitchen,
    MasterBedroom,
    KidsBedroom,
    GuestBedroom,
    LivingRoom,
    DiningRoom,
    Bathroom,
    Balcony,
    StudyRoom,
    PoojaRoom,
    Foyer,
    Utility
}
