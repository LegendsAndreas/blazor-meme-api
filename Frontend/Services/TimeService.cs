namespace BlazorApi.Services;

public class TimeService
{
    public DateTime GetCopenhagenUtcDateTime()
    {
        return DateTime.UtcNow.AddHours(2);
    }
}