using dy.net.model.entity;
using Xunit;

namespace dy.net.tests.Unit;

public class SmokeTests
{
    /// <summary>
    /// 冒烟：实体白名单注册完整（同时验证测试项目能引用主项目并跑起来）
    /// </summary>
    [Fact]
    public void BusinessEntityRegistry_ContainsCoreEntities()
    {
        Assert.Contains(typeof(DouyinVideo), BusinessEntityRegistry.Types);
        Assert.Contains(typeof(DouyinCookie), BusinessEntityRegistry.Types);
        Assert.Contains(typeof(AdminUserInfo), BusinessEntityRegistry.Types);
        Assert.Equal(12, BusinessEntityRegistry.Types.Length);
    }
}
