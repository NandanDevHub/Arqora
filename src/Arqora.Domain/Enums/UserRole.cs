namespace Arqora.Domain.Enums;

/// <summary>
/// Defining roles as a domain enum (rather than only in ASP.NET Identity)
/// lets us enforce role-based rules inside the domain layer itself,
/// keeping business logic independent of the identity framework.
/// </summary>
public enum UserRole
{
    Admin,
    Designer,
    Homeowner
}
