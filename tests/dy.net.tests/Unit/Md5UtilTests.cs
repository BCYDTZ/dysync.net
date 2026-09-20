using dy.net.utils;
using Xunit;

namespace dy.net.tests.Unit;

public class Md5UtilTests
{
    [Theory]
    [InlineData("", "d41d8cd98f00b204e9800998ecf8427e")]
    [InlineData("abc", "900150983cd24fb0d6963f7d28e17f72")]
    [InlineData("ABC", "902fbdd2b1df0c4f70b4a5d23525e932")]
    public void Md5_ComputesLowercaseHexDigest(string input, string expected)
    {
        Assert.Equal(expected, input.Md5());
    }

    [Fact]
    public void Md5_Returns32CharLowercaseHex()
    {
        var result = "任意中文内容".Md5();
        Assert.Equal(32, result.Length);
        Assert.Matches("^[0-9a-f]{32}$", result);
    }
}
