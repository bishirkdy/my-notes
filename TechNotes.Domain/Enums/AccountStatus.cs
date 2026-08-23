namespace TechNotes.Domain.Enums;
// Describes whether a user account can use the system.
public enum AccountStatus
{
    // The account can sign in and use features.
    Active = 1,
    // An administrator temporarily blocked the account.
    Suspended = 2,
    // The account is marked as removed without immediately erasing its data.
    Deleted = 3
}
