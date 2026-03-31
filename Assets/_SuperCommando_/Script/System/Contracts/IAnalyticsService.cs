public interface IAnalyticsService
{
    bool IsAvailable { get; }

    void TrackEvent(string eventName);
}
