using System.Diagnostics;
using NotificationService.Domain.Ports.TelemetryExtension;

namespace NotificationService.UnitTests;

public class TraceExtensionsTests
{
    [Fact]
    public void StartActivity_ShouldUseParentTraceContext()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "NotificationService.Api",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        var parent = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        using var activity = TraceExtensions.StartActivity("Processar ticket", ActivityKind.Consumer, parent);

        Assert.NotNull(activity);
        Assert.Equal(parent.TraceId, activity.TraceId);
        Assert.Equal(parent.SpanId, activity.ParentSpanId);
        Assert.Equal(ActivityKind.Consumer, activity.Kind);
    }
}
