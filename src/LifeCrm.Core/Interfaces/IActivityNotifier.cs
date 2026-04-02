namespace LifeCrm.Core.Interfaces
{
    public interface IActivityNotifier
    {
        Task NotifyAsync(string orgId, string eventType, object payload, CancellationToken ct = default);
    }
}
