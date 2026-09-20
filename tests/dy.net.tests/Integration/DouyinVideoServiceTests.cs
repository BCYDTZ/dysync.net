using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.repository;
using dy.net.service;
using Xunit;

namespace dy.net.tests.Integration;

public class DouyinVideoServiceTests : SqliteTestBase
{
    public DouyinVideoServiceTests()
    {
        // 生产环境在启动阶段（Program.cs InitializeDatabaseAsync）初始化统计行；
        // 夹具必须复制该前置，否则统计 guard 会取消所有视频变更
        new DouyinVideoStatisticsService(Db).EnsureInitializedAsync().GetAwaiter().GetResult();
    }

    private DouyinVideoService CreateService()
    {
        var videoRepo = new DouyinVideoRepository(Db);
        var cookieRepo = new DouyinCookieRepository(Db);
        var statistics = new DouyinVideoStatisticsService(Db);
        return new DouyinVideoService(videoRepo, cookieRepo, Db, statistics);
    }

    private static DouyinVideo MakeVideo(int i) => new()
    {
        Id = "v" + i,
        AwemeId = "aweme-" + i,
        VideoTitle = "视频标题" + i,
        VideoTitleSimplify = "标题" + i,
        Author = "作者甲",
        AuthorId = "author-1",
        AuthorAvatarUrl = "",
        Tag1 = "收藏",
        ViedoType = VideoTypeEnum.dy_collects,
        FileSize = 100,
        FileHash = "hash-" + i,
        VideoSavePath = "/app/collect/v" + i + ".mp4",
        VideoCoverSavePath = "/app/collect/v" + i + ".jpg",
        SyncTime = DateTime.Now,
        CreateTime = DateTime.Now,
        CookieId = "ck1"
    };

    [Fact]
    public async Task BatchInsertOrUpdate_EmptyList_ReturnsTrue()
    {
        var svc = CreateService();
        Assert.True(await svc.BatchInsertOrUpdate(new List<DouyinVideo>()));
    }

    [Fact]
    public async Task BatchInsertOrUpdate_Insert_StatisticsStayConsistent()
    {
        var svc = CreateService();
        var videos = new List<DouyinVideo> { MakeVideo(1), MakeVideo(2), MakeVideo(3) };

        Assert.True(await svc.BatchInsertOrUpdate(videos));

        var stats = await svc.GetStatics();
        Assert.Equal(3, stats.VideoCount);      // 总数一致
        Assert.Equal(3, stats.CollectCount);    // 收藏类型一致（dy_collects）
        Assert.Equal(1, stats.AuthorCount);     // 同一作者计 1
        Assert.Equal(1, stats.CategoryCount);   // 同一 Tag1 分类计 1

        var roundtrip = await svc.GetByAwemeId("aweme-2");
        Assert.NotNull(roundtrip);
        Assert.Equal("视频标题2", roundtrip.VideoTitle);
    }

    [Fact]
    public async Task BatchInsertOrUpdate_ReinsertSameAwemeIds_NoDuplicates()
    {
        var svc = CreateService();
        await svc.BatchInsertOrUpdate(new List<DouyinVideo> { MakeVideo(1), MakeVideo(2) });

        // 同 AwemeId 再次写入走更新路径，不产生重复记录
        Assert.True(await svc.BatchInsertOrUpdate(new List<DouyinVideo> { MakeVideo(1), MakeVideo(2) }));

        var stats = await svc.GetStatics();
        Assert.Equal(2, stats.VideoCount);
    }

    [Fact]
    public async Task DeleteById_StatisticsDecreaseConsistently()
    {
        var svc = CreateService();
        await svc.BatchInsertOrUpdate(new List<DouyinVideo> { MakeVideo(1), MakeVideo(2), MakeVideo(3) });

        Assert.True(await svc.DeleteById("v2"));

        var stats = await svc.GetStatics();
        Assert.Equal(2, stats.VideoCount);      // 删除后统计同步减少
        Assert.Equal(2, stats.CollectCount);
        Assert.Null(await svc.GetByAwemeId("aweme-2"));
    }
}
