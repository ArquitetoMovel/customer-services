using System;
using System.Diagnostics;

namespace NotificationService.Domain.Ports.TelemetryExtension;

public static class TraceExtensions
{
    private static readonly ActivitySource ActivitySource = new("NotificationService.Api");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
    {
        return ActivitySource.StartActivity(name, kind, parentContext);
    }
}
