namespace Sorairo.Common.Helpers;

public static class FormatHelper
{
    public static string FormatPlaybackTime(TimeSpan time)
    {
        var hours = time.Hours;
        var minutes = time.Minutes;
        var seconds = time.Seconds;
        if (hours > 0)
        {
            return $"{hours.ToString().PadLeft(2, '0')}:{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}";
        }
        return $"{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}";
    }
}
