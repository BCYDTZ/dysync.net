using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.repository;
using dy.net.service;
using Xunit;

namespace dy.net.tests.Integration;

/// <summary>
/// 回归 #33：统计行未初始化时（升级/迁移后首次运行的真实场景），
/// HandOldFolderVideos 的数据库批量更新被事务回滚，但失败不得静默。
/// 注意：本测试类刻意不在构造函数中调用 EnsureInitializedAsync。
/// </summary>
public class HandOldFolderVideosTests : SqliteTestBase
{
    private DouyinVideoService CreateService() =>
        new DouyinVideoService(
            new DouyinVideoRepository(Db),
            new DouyinCookieRepository(Db),
            Db,
            new DouyinVideoStatisticsService(Db));

    private static DouyinVideo MakeVideo(int i) => new()
    {
        Id = "v" + i,
        AwemeId = "aweme-" + i,
        VideoTitle = "视频标题" + i,
        VideoTitleSimplify = "标题" + i,
        Author = "作者甲",
        AuthorId = "author-1",
        Tag1 = "收藏",
        ViedoType = VideoTypeEnum.dy_collects,
        FileSize = 100,
        SyncTime = DateTime.Now,
        CreateTime = DateTime.Now,
        CookieId = "ck1",
        // 指向不存在的路径：循环内跳过文件移动，末尾仍会执行批量数据库更新
        VideoSavePath = "/app/collect/收藏/不存在/标题" + i + ".mp4"
    };

    [Fact]
    public async Task HandOldFolderVideos_DatabaseUpdateFailureIsVisible()
    {
        var repo = new DouyinVideoRepository(Db);
        // 直接经仓储播种，绕过 BatchInsertOrUpdate（避免提前触发统计 guard）
        Assert.True(await repo.InsertRangeAsync(new List<DouyinVideo> { MakeVideo(1), MakeVideo(2) }) > 0);

        var svc = CreateService();

        var result = await svc.HandOldFolderVideos();

        // 修复前：静默返回 true（数据库更新被回滚但无人知晓）
        Assert.False(result);
    }
}
