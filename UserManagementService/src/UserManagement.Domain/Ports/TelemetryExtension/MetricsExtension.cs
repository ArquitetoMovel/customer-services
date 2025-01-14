using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace UserManagement.Domain.Ports.TelemetryExtension;

public static class MetricsExtension
{
    private static readonly Meter Meter = new("UserManagement.Api");
    private static readonly Counter<long> AttendanceCounter;
    private static readonly UpDownCounter<long> ActiveAttendancesCounter;
    private static readonly ObservableGauge<long> WaitingAttendancesGauge;
    private static long _waitingAttendances;

    static MetricsExtension()
    {
        AttendanceCounter = Meter.CreateCounter<long>(
            name: "attendance.total",
            unit: "{attendances}",
            description: "Total number of attendances received"
        );

        ActiveAttendancesCounter = Meter.CreateUpDownCounter<long>(
            name: "attendance.active",
            unit: "{active_attendances}",
            description: "Number of active attendances"
        );

        WaitingAttendancesGauge = Meter.CreateObservableGauge<long>(
            name: "attendance.waiting",
            unit: "{waiting_attendances}",
            description: "Number of attendances in waiting status",
            observeValue: () => _waitingAttendances
        );
    }

    public static void IncrementAttendanceCount(string priority)
    {
        var tags = new TagList
        {
            { "priority", priority }
        };
        
        AttendanceCounter.Add(1, tags);
    }

    public static void IncrementActiveAttendances(string priority)
    {
        var tags = new TagList
        {
            { "priority", priority }
        };
        
        ActiveAttendancesCounter.Add(1, tags);
    }

    public static void DecrementActiveAttendances(string priority)
    {
        var tags = new TagList
        {
            { "priority", priority }
        };
        
        ActiveAttendancesCounter.Add(-1, tags);
    }

    public static void SetWaitingAttendances(long count, string priority)
    {
        var tags = new TagList { { "priority", priority } };
        _waitingAttendances = count;
    }
}
