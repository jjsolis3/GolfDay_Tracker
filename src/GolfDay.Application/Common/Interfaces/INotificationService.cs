using GolfDay.Domain.Enums;

namespace GolfDay.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(string userId, NotificationType type, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default);
    Task SendToManyAsync(IEnumerable<string> userIds, NotificationType type, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
}
