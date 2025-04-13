public class WeeklyTimer : IDisposable
{
    private readonly Timer _timer;
    private readonly Action _callback;
    private readonly TimeSpan _timeOfDay;
    private readonly HashSet<DayOfWeek> _daysOfWeek;

    public WeeklyTimer(Action callback, TimeSpan timeOfDay, params DayOfWeek[] daysOfWeek)
    {
        _callback = callback;
        _timeOfDay = timeOfDay;
        _daysOfWeek = new HashSet<DayOfWeek>(daysOfWeek);

        _timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        ScheduleNext();
    }

    private void OnTimerElapsed(object? state)
    {
        _callback();
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        var now = DateTime.Now;
        var next = Enumerable.Range(0, 7)
            .Select(offset => now.Date.AddDays(offset))
            .Where(d => _daysOfWeek.Contains(d.DayOfWeek))
            .Select(d => d.Add(_timeOfDay))
            .FirstOrDefault(dt => dt > now);

        var delay = next - now;

        _timer.Change(delay, Timeout.InfiniteTimeSpan);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}