using System.Diagnostics;

namespace UserManagement.Domain.Ports.TelemetryExtension;

public static class TracesExtension
{
    private static readonly ActivitySource ActivitySource = new("UserManagement.Api");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static Activity? StartHttpActivity(string name)
    {
        return ActivitySource.StartActivity(name, ActivityKind.Client);
    }

    public static void AddTag(this Activity activity, string key, object value)
    {
        activity?.SetTag(key, value);
    }

    public static void AddEvent(this Activity activity, string eventName, Dictionary<string, object>? tags = null)
    {
        var activityEvent = new ActivityEvent(eventName, DateTimeOffset.UtcNow, new ActivityTagsCollection(tags));
        activity?.AddEvent(activityEvent);
    }

    public static Activity? StartChildActivity(this Activity parentActivity, string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind, parentActivity.Context);
    }

    public static void SetError(this Activity activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity?.AddTag("error.type", exception.GetType().Name);
        activity?.AddTag("error.message", exception.Message);
        activity?.AddTag("error.stack_trace", exception.StackTrace);
    }
}