using dy.net.model.entity;
using dy.net.repository;
using dy.net.service;
using Xunit;

namespace dy.net.tests.Integration;

public class DouyinCookieServiceTests : SqliteTestBase
{
    private DouyinCookieService CreateService() =>
        new(new DouyinCookieRepository(Db));

    private static DouyinCookie MakeCookie(string id = "ck1") => new()
    {
        Id = id,
        UserName = "账号一",
        Cookies = "ttwid=abc; msToken=xyz",
        SecUserId = "SU-1",
        MyUserId = "ME-1",
        Status = 1,
        SavePath = "/app/collect",
        FavSavePath = "/app/favorite",
        UpSavePath = "/app/uper"
    };

    [Fact]
    public async Task Add_ThenGetAllAsync_ReturnsCookie()
    {
        var svc = CreateService();

        Assert.True(await svc.Add(MakeCookie()));

        var all = await svc.GetAllAsync();
        var single = Assert.Single(all);
        Assert.Equal("ck1", single.Id);
        Assert.Equal("账号一", single.UserName);
    }

    [Fact]
    public async Task FastResetCookie_ResetsStatusToNormal()
    {
        var svc = CreateService();
        var cookie = MakeCookie();
        cookie.StatusCode = 2;
        cookie.StatusMsg = "失效";
        await svc.Add(cookie);

        Assert.True(await svc.FastResetCookie("ck1", "new-cookie-value"));

        var reset = await svc.GetByIdAsync("ck1");
        Assert.Equal(0, reset.StatusCode);
        Assert.Equal("正常", reset.StatusMsg);
    }

    [Fact]
    public async Task DeleteByIdAsync_RemovesCookie()
    {
        var svc = CreateService();
        await svc.Add(MakeCookie());

        Assert.True(await svc.DeleteByIdAsync("ck1"));
        Assert.Empty(await svc.GetAllAsync());
    }
}
