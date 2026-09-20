using ClockSnowFlake;
using dy.net.model.entity;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace dy.net.tests.Integration;

/// <summary>
/// 集成测试基类：每个测试类一个独立的临时 SQLite 库，
/// CodeFirst 按业务实体白名单建表，Dispose 时整目录清理。
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private static bool _idGenerInitialized;
    private readonly string _dbDir;

    protected SqliteTestBase()
    {
        // 初始化雪花 ID 静态生成器（等价于 Program.cs 的 AddSnowFlakeId）
        if (!_idGenerInitialized)
        {
            new ServiceCollection().AddSnowFlakeId(options => options.WorkId = 1);
            _idGenerInitialized = true;
        }

        _dbDir = Path.Combine(Path.GetTempPath(), "dy-net-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dbDir);

        Db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = "DataSource=" + Path.Combine(_dbDir, "test.sqlite"),
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true
        });
        Db.CodeFirst.InitTables(BusinessEntityRegistry.Types);
    }

    protected ISqlSugarClient Db { get; }

    public void Dispose()
    {
        Db.Dispose();
        try { Directory.Delete(_dbDir, true); } catch { /* 临时目录清理失败不影响测试结果 */ }
    }
}
