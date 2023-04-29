using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Data;
using System.Threading.Tasks;
using ZLogger;

namespace WebApiForCollectingRPG.Services;

public class GameDb : IGameDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();

        _compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new QueryFactory(_dbConn, _compiler);
    }

    public void Dispose()
    {
        Close();
    }

    private void Open()
    {
        try
        {
            _dbConn = new MySqlConnection(_dbConfig.Value.GameDb);
            _dbConn.Open();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex,
$"[Open GameDb Fail] ErrorCode: {ErrorCode.GetGameDbConnectionFail}");
        }

    }

    private void Close()
    {
        _dbConn.Close();
    }

    public async Task<ErrorCode> CreateAccountGameDataAsync(Int64 accountId)
    {
        try
        {
            await _queryFactory.Query("account_game").InsertAsync(new
            {
                account_id = accountId,
                money = 0,
                exp = 0
            });

            _logger.ZLogDebug(
    $"[GameDb.CreateAccountGameData] AccountId: {accountId}");

            return ErrorCode.None;
        } catch (Exception ex)
        {
            _logger.ZLogError(ex,
    $"[GameDb.CreateAccountGameData] ErrorCode: {ErrorCode.CreateAccountGameFailException}");
        }
        return ErrorCode.CreateAccountGameFailException;
    }

    public async Task<ErrorCode> CreateAccountItemDataAsync(Int64 accountId)
    {
        try
        {

            var cols = new[] { "account_id", "slot_id", "item_id", "item_count", "enhance_count" };
            var data = new[]
            {
                new object[]{accountId, 1, 0, 0, 0},
                new object[]{accountId, 2, 0, 0, 0},
                new object[]{accountId, 3, 0, 0, 0 },
                new object[]{accountId, 4, 0, 0, 0 },
                new object[]{accountId, 5, 0, 0, 0 },
                new object[]{accountId, 6, 0, 0, 0 },
                new object[]{accountId, 7, 0, 0, 0 },
                new object[]{accountId, 8, 0, 0, 0 },
                new object[]{accountId, 9, 0, 0, 0 },
                new object[]{accountId, 10, 0, 0, 0 },
            };

            await _queryFactory.Query("account_item").InsertAsync(cols, data);

            _logger.ZLogDebug(
    $"[GameDb.CreateAccountItemData] AccountId: {accountId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex,
    $"[GameDb.CreateAccountItemData] ErrorCode: {ErrorCode.CreateAccountItemFailException}");
        }
        return ErrorCode.CreateAccountItemFailException;
    }
}
