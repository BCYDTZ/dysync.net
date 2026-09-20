using dy.net.model.entity;
using dy.net.repository;
using dy.net.service;
using Xunit;

namespace dy.net.tests.Integration;

public class DouyinFollowServiceTests : SqliteTestBase
{
    private DouyinFollowService CreateService() =>
        new(new DouyinFollowRepository(Db));

    private static DouyinFollowed MakeFollow(string secUid = "SU-1") => new()
    {
        SecUid = secUid,
        UperId = secUid, // GetByUperId 经 UperId 列查询，测试数据需与 SecUid 保持一致
        mySelfId = "ME-1",
        UperName = "测试博主"
    };

    [Fact]
    public async Task AddAsync_NewFollow_GeneratesIdAndFlagsAsManual()
    {
        var svc = CreateService();
        var follow = MakeFollow();

        Assert.True(await svc.AddAsync(follow));

        Assert.False(string.IsNullOrWhiteSpace(follow.Id)); // 雪花 ID 已生成
        Assert.True(follow.IsNoFollowed);                    // 手动添加标记
        Assert.True((DateTime.UtcNow - follow.LastSyncTime).Duration() < TimeSpan.FromMinutes(1));

        var persisted = await svc.GetByUperId("SU-1", "ME-1");
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task AddAsync_DuplicateSecUidAndSelfId_ReturnsFalse()
    {
        var svc = CreateService();
        await svc.AddAsync(MakeFollow("SU-1"));

        Assert.False(await svc.AddAsync(MakeFollow("SU-1"))); // 同 SecUid+mySelfId 防重

        var handFollows = await svc.GetHandFollows();
        Assert.Single(handFollows);
    }
}
