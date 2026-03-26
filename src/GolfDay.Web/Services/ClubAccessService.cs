using GolfDay.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GolfDay.Web.Services;

public interface IClubAccessService
{
    Task<bool> HasActiveMembershipAsync(string userId);
}

public class ClubAccessService : IClubAccessService
{
    private readonly IApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public ClubAccessService(IApplicationDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    public async Task<bool> HasActiveMembershipAsync(string userId)
    {
        var key = $"club_member:{userId}";
        if (_cache.TryGetValue(key, out bool cached))
            return cached;

        var result = await _db.ClubMemberships
            .AnyAsync(m => m.UserId == userId && m.IsActive);

        _cache.Set(key, result, TimeSpan.FromMinutes(5));
        return result;
    }
}
