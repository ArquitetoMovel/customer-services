using System.Diagnostics.Metrics;
using UserManagement.Domain.Ports.TelemetryExtension;

namespace UserManagement.UnitTests;

public class MetricsExtensionTests
{
    [Fact]
    public void SetWaitingAttendances_ShouldReportCountForEachPriority()
    {
        var measurements = new Dictionary<string, long>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == "UserManagement.Api" &&
                    instrument.Name == "attendance.waiting")
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag.Key == "priority")
                {
                    measurements[(string)tag.Value!] = value;
                }
            }
        });
        listener.Start();

        MetricsExtension.SetWaitingAttendances(3, "priority");
        MetricsExtension.SetWaitingAttendances(5, "normal");
        listener.RecordObservableInstruments();

        Assert.Equal(3, measurements["priority"]);
        Assert.Equal(5, measurements["normal"]);
    }
}
