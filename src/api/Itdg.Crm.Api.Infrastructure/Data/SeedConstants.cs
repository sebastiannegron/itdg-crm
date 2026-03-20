namespace Itdg.Crm.Api.Infrastructure.Data;

public static class SeedConstants
{
    public static readonly Guid DefaultTenantId = new("10000000-0000-0000-0000-000000000001");
    public static readonly DateTimeOffset SeedTimestamp = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
