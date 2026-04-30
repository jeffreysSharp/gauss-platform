namespace Gauss.Identity.Domain.Users;

public enum UserStatus
{
    PendingEmailConfirmation = 1,
    Active = 2,
    Locked = 3,
    Suspended = 4,
    Deactivated = 5
}
