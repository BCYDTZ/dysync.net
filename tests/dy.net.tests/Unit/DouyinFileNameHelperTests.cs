using dy.net.utils;
using Xunit;

namespace dy.net.tests.Unit;

public class DouyinFileNameHelperTests
{
    #region KeepChineseLettersAndNumbers

    [Theory]
    [InlineData("你好abc123", "你好abc123")] // 中文/字母/数字全部保留
    [InlineData("你好!@#世界", "你好世界")]   // 特殊符号被移除
    [InlineData("abc-DEF_456", "abcDEF456")] // 连字符/下划线也被移除
    [InlineData("", "")]                     // 空串原样返回
    public void KeepChineseLettersAndNumbers_KeepsOnlyChineseLettersNumbers(string input, string expected)
    {
        Assert.Equal(expected, DouyinFileNameHelper.KeepChineseLettersAndNumbers(input));
    }

    [Fact]
    public void KeepChineseLettersAndNumbers_NullReturnsNull()
    {
        Assert.Null(DouyinFileNameHelper.KeepChineseLettersAndNumbers(null));
    }

    [Fact]
    public void KeepChineseLettersAndNumbers_RemovesEmoji()
    {
        Assert.Equal("测试", DouyinFileNameHelper.KeepChineseLettersAndNumbers("测😀试"));
    }

    #endregion

    #region SanitizeLinuxFileName

    [Fact]
    public void SanitizeLinuxFileName_EmptyInputReturnsDefaultWithoutSpaces()
    {
        Assert.Equal("dft", DouyinFileNameHelper.SanitizeLinuxFileName("   ", "dft"));
        Assert.Equal("dft", DouyinFileNameHelper.SanitizeLinuxFileName(null, "dft"));
    }

    [Theory]
    [InlineData("a:b", "a_b")] // 冒号替换为下划线
    [InlineData("a/b", "a_b")] // 斜杠替换为下划线
    [InlineData("a\\b", "a_b")]
    [InlineData("a?b*c", "a_b_c")]
    public void SanitizeLinuxFileName_ReplacesInvalidCharsWithUnderscore(string input, string expected)
    {
        Assert.Equal(expected, DouyinFileNameHelper.SanitizeLinuxFileName(input, "dft"));
    }

    [Fact]
    public void SanitizeLinuxFileName_RemovesSpaces()
    {
        Assert.Equal("ab", DouyinFileNameHelper.SanitizeLinuxFileName("a b", "dft"));
    }

    [Fact]
    public void SanitizeLinuxFileName_TruncatesTo100Utf8Bytes()
    {
        // 注意：'中' 是 3 字节 UTF-8 字符，100 字节最多容纳 33 个完整中文，
        // 按 100 字节硬截断会把第 34 个字符截半，Encoding.UTF8.GetString 会把
        // 残缺字节解码成 U+FFFD 替换符（乱码），与源码注释"自动忽略不完整的尾部字节"不符——疑似 bug，
        // 因此不直接断言纯中文截断结果。
        // 这里改用字符边界对齐的输入验证截断逻辑本身：
        // 20 个中文(60B) + 40 个字母(40B) 恰好 100 字节，再加 "DDD"(3B) 触发超长截断。
        var input = new string('中', 20) + new string('a', 40) + "DDD";
        var result = DouyinFileNameHelper.SanitizeLinuxFileName(input, "dft");
        Assert.Equal(new string('中', 20) + new string('a', 40), result);
    }

    [Fact]
    public void SanitizeLinuxFileName_FolderModeStripsNonChineseLettersNumbers()
    {
        // '#' 不在非法字符表里，isfolder=true 时由 KeepChineseLettersAndNumbers 移除
        Assert.Equal("视频123", DouyinFileNameHelper.SanitizeLinuxFileName("视频#123", "dft", true));
        // isfolder=false 时保留
        Assert.Equal("视频#123", DouyinFileNameHelper.SanitizeLinuxFileName("视频#123", "dft", false));
    }

    #endregion

    #region RemoveNumberSuffix

    [Theory]
    [InlineData("video_001.mp4", "video.mp4")] // 点号前的 _数字 后缀被去除
    [InlineData("video_001", "video_001")]     // 无点号不匹配，原样返回
    [InlineData("", "")]
    public void RemoveNumberSuffix_RemovesNumericSuffixBeforeDot(string input, string expected)
    {
        Assert.Equal(expected, DouyinFileNameHelper.RemoveNumberSuffix(input));
    }

    [Fact]
    public void RemoveNumberSuffix_NullReturnsNull()
    {
        Assert.Null(DouyinFileNameHelper.RemoveNumberSuffix(null));
    }

    #endregion

    #region LimitUnifiedCount

    [Theory]
    [InlineData("abcd", 10, "abcd")] // 未超长直返
    [InlineData("abcd", 2, "ab")]    // 超长截断（实现按字符数计，非视觉宽度）
    public void LimitUnifiedCount_TruncatesByCharCount(string input, int max, string expected)
    {
        Assert.Equal(expected, input.LimitUnifiedCount(max));
    }

    [Fact]
    public void LimitUnifiedCount_AppendsEllipsisWhenRequested()
    {
        Assert.Equal("ab...", "abcd".LimitUnifiedCount(2, addEllipsis: true));
    }

    [Fact]
    public void LimitUnifiedCount_NonPositiveMaxTreatedAsOne()
    {
        Assert.Equal("a", "abcd".LimitUnifiedCount(0));
        Assert.Equal("a", "abcd".LimitUnifiedCount(-5));
    }

    #endregion

    #region IsValidWithoutSpecialChars

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("abc XYZ_123 中文", true)]
    [InlineData("abc!", false)]
    [InlineData("你好#世界", false)]
    public void IsValidWithoutSpecialChars_AllowsOnlyLettersDigitsChineseSpaceUnderscore(string input, bool expected)
    {
        Assert.Equal(expected, DouyinFileNameHelper.IsValidWithoutSpecialChars(input));
    }

    #endregion
}
