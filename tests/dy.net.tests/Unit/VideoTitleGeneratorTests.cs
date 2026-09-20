using dy.net.model.dto;
using dy.net.utils;
using Xunit;

namespace dy.net.tests.Unit;

public class VideoTitleGeneratorTests
{
    [Fact]
    public void Generate_ReplacesPlaceholdersWithSanitizedValues()
    {
        var data = new VideoTitleDataTemplate
        {
            VideoTitle = "第一个视频",
            Author = "老王",
            Resolution = "1080x1920",
            FileHash = "abc123",
            Id = "999"
        };
        var result = VideoTitleGenerator.Generate("v-{Author}-{VideoTitle}-{Resolution}-{FileHash}-{Id}", data);
        Assert.Equal("v-老王-第一个视频-1080x1920-abc123-999", result);
    }

    [Fact]
    public void Generate_TitleIsSanitizedBeforeSubstitution()
    {
        // Id 赋空串：Id 为 null 时 Generate 内部 data.Id.ToString() 会抛 NRE（疑似 bug，见下方 EmptyField 用例说明）
        var data = new VideoTitleDataTemplate { VideoTitle = "a!b@c", Id = "" };
        // "a!b@c" 经 KeepChineseLettersAndNumbers 去掉 '!' 和 '@' 后应为 "abc"
        //（简报原预期 "ab-v" 为笔误，以实际正确行为为准）
        Assert.Equal("abc-v", VideoTitleGenerator.Generate("{VideoTitle}-v", data));
    }

    [Fact]
    public void Generate_EmptyFieldFallsBackToPlaceholder()
    {
        // 注意：Id 是 VideoTitleDataTemplate 中唯一没有默认空串的属性，
        // Generate 内部会无条件求值 data.Id.ToString()，Id 为 null 时直接 NullReferenceException，
        // 与 "data ??= new VideoTitleDataTemplate() 避免数据为空" 的意图矛盾——疑似 bug（已另行报告）。
        // 这里显式给 Id 赋空串，专注验证"空字段回退到占位符"这一目标行为。
        var data = new VideoTitleDataTemplate { Id = "" }; // 其余字段全空
        Assert.Equal("无", VideoTitleGenerator.Generate("{Author}", data, emptyPlaceholder: "无"));
    }

    [Fact]
    public void Generate_FormatsReleaseTimeWithGivenFormat()
    {
        // Id 赋空串：Id 为 null 时 Generate 内部 data.Id.ToString() 会抛 NRE（疑似 bug，见上方 EmptyField 用例说明）
        var data = new VideoTitleDataTemplate { ReleaseTime = new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc), Id = "" };
        Assert.Equal("20260920", VideoTitleGenerator.Generate("{ReleaseTime}", data));
        Assert.Equal("2026-09-20", VideoTitleGenerator.Generate("{ReleaseTime}", data, timeFormat: "yyyy-MM-dd"));
    }

    [Fact]
    public void Generate_NullDataIsTreatedAsEmpty()
    {
        // 回归 #31：data 为 null 时兜底对象 Id=null 曾触发 NRE
        Assert.Equal("无", VideoTitleGenerator.Generate("{Author}", null, emptyPlaceholder: "无"));
    }

    [Fact]
    public void Generate_DefaultTemplateIdIsEmptyString()
    {
        // 回归 #31：未显式赋值的 Id 不再是 null
        Assert.Equal("", VideoTitleGenerator.Generate("{Id}", new VideoTitleDataTemplate()));
    }

    [Fact]
    public void Generate_NullTemplateThrows()
    {
        Assert.Throws<ArgumentNullException>(() => VideoTitleGenerator.Generate(null, new VideoTitleDataTemplate()));
    }
}
