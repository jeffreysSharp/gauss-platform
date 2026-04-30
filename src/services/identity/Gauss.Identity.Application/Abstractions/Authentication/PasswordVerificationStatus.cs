namespace Gauss.Identity.Application.Abstractions.Authentication;

public enum PasswordVerificationStatus
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
