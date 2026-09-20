using dy.net.utils;
using Xunit;

namespace dy.net.tests.Unit;

public class DateTimeUtilTests
{
    private static readonly DateTime EpochUtc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Convert10BitTimestamp_ZeroIsEpoch()
    {
        // 实现返回本地时间，断言换算回 UTC，避免测试依赖机器时区
        Assert.Equal(EpochUtc, DateTimeUtil.Convert10BitTimestamp(0).ToUniversalTime());
    }

    [Fact]
    public void Convert10BitTimestamp_86400IsPlusOneDay()
    {
        Assert.Equal(EpochUtc.AddDays(1), DateTimeUtil.Convert10BitTimestamp(86400).ToUniversalTime());
    }

    [Fact]
    public void GetUnixTimestampMilliseconds_IsCloseToNow()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var ts = DateTimeUtil.GetUnixTimestampMilliseconds();
        Assert.InRange(ts, now - 60_000, now + 60_000);
    }
}
