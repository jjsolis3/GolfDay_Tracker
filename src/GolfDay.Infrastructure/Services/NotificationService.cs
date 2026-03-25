using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SendAsync(string userId, NotificationType type, string title, string message,
        string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToManyAsync(IEnumerable<string> userIds, NotificationType type, string title,
        string message, string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl
            });
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }
}
