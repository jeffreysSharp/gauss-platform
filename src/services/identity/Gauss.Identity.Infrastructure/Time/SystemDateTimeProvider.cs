using Gauss.Identity.Application.Abstractions.Time;

namespace Gauss.Identity.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
