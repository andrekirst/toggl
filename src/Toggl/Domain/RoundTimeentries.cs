using Domain.Model;

namespace Domain;

public static class RoundTimeentries
{
    public static RoundedTimeentriesResult Round(this GroupTimeentriesResult groupTimeentriesResult, RoundTimeentriesOptions? options = null)
    {
        var thresholdInSeconds = options?.ThresholdInSeconds ?? RoundTimeentriesOptions.DefaultThresholdInSeconds;
        var roundOff = options?.ThresholdRoundOff ?? RoundTimeentriesOptions.DefaultThresholdRound;

        var groupTotalDuration = groupTimeentriesResult.TotalDuration;
        var roundedGroupTotalDuration = RoundGroupTotalDuration(groupTotalDuration, thresholdInSeconds, roundOff);
        var result = GetPreRoundedTimeentries(groupTimeentriesResult, roundedGroupTotalDuration, thresholdInSeconds, roundOff);

        PostProcessRoundTimeentries(result, thresholdInSeconds);

        return result;
    }

    private static void PostProcessRoundTimeentries(RoundedTimeentriesResult result, long thresholdInSeconds)
    {
        while (result.GroupTotalDuration != result.CalculatedTotalDuration)
        {
            var maxItemByRoundUpDifference = GetMaxItemByRoundUpDifference(result);
            if (maxItemByRoundUpDifference == null)
            {
                break;
            }

            result.RoundUpTimeentry(maxItemByRoundUpDifference, thresholdInSeconds);
        }
    }

    private static long RoundGroupTotalDuration(long groupTotalDuration, long thresholdInSeconds, long roundOff)
    {
        var divisionRest = groupTotalDuration % thresholdInSeconds;
        var count = groupTotalDuration / thresholdInSeconds;
        return SmallerOrEqualThreshold(divisionRest, count, thresholdInSeconds)
            ? thresholdInSeconds
            : SmallerThanRoundOff(divisionRest, roundOff)
                ? count * thresholdInSeconds
                : (count + 1) * thresholdInSeconds;
    }

    private static RoundedTimeentriesResult GetPreRoundedTimeentries(GroupTimeentriesResult groupTimeentriesResult, long roundedGroupTotalDuration, long thresholdInSeconds, long roundOff)
    {
        var result = new RoundedTimeentriesResult(roundedGroupTotalDuration);

        foreach (var groupedTimeentry in groupTimeentriesResult.GroupedTimeentries)
        {
            var divisionRest = groupedTimeentry.Duration % thresholdInSeconds;
            var count = groupedTimeentry.Duration / thresholdInSeconds;
            if (SmallerOrEqualThreshold(divisionRest, count, thresholdInSeconds))
            {
                result.Add(RoundedTimeentry.FromGroupedWithNewDuration(groupedTimeentry, thresholdInSeconds));
            }
            else if (SmallerThanRoundOff(divisionRest, roundOff))
            {
                result.Add(RoundedTimeentry.FromGroupedWithNewDuration(groupedTimeentry, count * thresholdInSeconds));
            }
            else
            {
                var x = RoundedTimeentry.FromGroupedWithNewDuration(groupedTimeentry, count * thresholdInSeconds);
                x.RoundUpDifference = divisionRest;
                result.Add(x);
            }
        }

        return result;
    }

    private static bool SmallerThanRoundOff(long divisionRest, long roundOff) => divisionRest < roundOff;
    private static bool SmallerOrEqualThreshold(long divisionRest, long count, long thresholdInSeconds) => divisionRest < thresholdInSeconds && count == 0;

    private static RoundedTimeentry? GetMaxItemByRoundUpDifference(RoundedTimeentriesResult result) =>
        result.RoundedTimeentries
            .Where(r => r.RoundUpDifference != null)
            .OrderByDescending(r => r.RoundUpDifference)
            .FirstOrDefault();
}

public class RoundedTimeentriesResult
{
    public long GroupTotalDuration { get; }

    public RoundedTimeentriesResult(long groupTotalDuration)
    {
        GroupTotalDuration = groupTotalDuration;
    }

    public List<RoundedTimeentry> RoundedTimeentries { get; set; } = new List<RoundedTimeentry>();
    public long CalculatedTotalDuration { get; private set; }

    public void Add(RoundedTimeentry roundedTimeentry)
    {
        RoundedTimeentries.Add(roundedTimeentry);
        CalculatedTotalDuration += roundedTimeentry.Duration;
    }

    public void RoundUpTimeentry(RoundedTimeentry roundedTimeentry, long duration)
    {
        roundedTimeentry.RemoveRoundUpDifference();
        roundedTimeentry.Duration += duration;
        CalculatedTotalDuration += duration;
    }

    public override string ToString()
    {
        var s = "";

        foreach (var roundedTimeentry in RoundedTimeentries)
        {
            var duration = TimeSpan.FromSeconds(roundedTimeentry.Duration);
            s += $"{roundedTimeentry.Description} - {duration}{Environment.NewLine}";
        }

        return s;
    }
}

public class RoundTimeentriesOptions
{
    public const long DefaultThresholdInSeconds = 15 * 60;
    public const long DefaultThresholdRound = 5 * 60;

    public long ThresholdInSeconds { get; set; } = DefaultThresholdInSeconds;
    public long ThresholdRoundUp { get; set; } = DefaultThresholdRound;
    public long ThresholdRoundOff { get; set; } = DefaultThresholdRound;
}