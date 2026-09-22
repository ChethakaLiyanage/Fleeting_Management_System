using FleetManagement.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FleetManagement.Infrastructure.Cache;

public class DashboardCacheInvalidator : IDashboardCache
{
    private readonly IMemoryCache _cache;
    public const string CacheKey = "dashboard:summary";

    public DashboardCacheInvalidator(IMemoryCache cache) => _cache = cache;

    public void Invalidate() => _cache.Remove(CacheKey);
}
