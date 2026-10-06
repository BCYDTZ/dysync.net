using dy.net.model.entity;
using dy.net.repository;
using dy.net.service;
using dy.net.utils;
using Xunit;

namespace dy.net.tests.Integration;

public class AdminUserServiceTests : SqliteTestBase
{
    private AdminUserService CreateService() =>
        new(new AdminUserRepository(Db));

    [Fact]
    public async Task InitUser_FirstTimeCreatesUserWithMd5Password()
    {
        var svc = CreateService();

        var (code, erro) = svc.InitUser("douyin", "douyin2026");

        Assert.Equal(0, code);
        var user = await svc.GetUser();
        Assert.NotNull(user);
        Assert.Equal("douyin", user.UserName);
        Assert.Equal("douyin2026".Md5(), user.Password); // 密码必须 MD5 化入库
    }

    [Fact]
    public async Task InitUser_SecondTimeIsRejected()
    {
        var svc = CreateService();
        svc.InitUser("douyin", "douyin2026");

        var (code, erro) = svc.InitUser("another", "pass");

        Assert.Equal(-1, code);
        Assert.Equal("系统用户已存在", erro);
        var user = await svc.GetUser();
        Assert.Equal("douyin", user.UserName); // 原用户不被覆盖
    }
}
