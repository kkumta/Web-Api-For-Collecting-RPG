﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.Dtos.Game;
using ZLogger;
using static LogManager;

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
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
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
            await _queryFactory.Query("AccountGame").InsertAsync(new
            {
                AccountId = accountId,
                Money = 0,
                Exp = 0
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
    $"[GameDb.CreateAccountGameData] AccountId: {accountId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
    $"[GameDb.CreateAccountGameData] ErrorCode: {ErrorCode.CreateAccountGameFailException}");
        }
        return ErrorCode.CreateAccountGameFailException;
    }

    public async Task<ErrorCode> CreateAccountItemDataAsync(Int64 accountId)
    {
        try
        {

            var cols = new[] { "AccountId", "SlotId", "ItemId", "ItemCount", "EnhanceCount" };
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

            await _queryFactory.Query("AccountItem").InsertAsync(cols, data);

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
    $"[GameDb.CreateAccountItemData] AccountId: {accountId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
    $"[GameDb.CreateAccountItemData] ErrorCode: {ErrorCode.CreateAccountItemFailException}");
        }
        return ErrorCode.CreateAccountItemFailException;
    }

    public async Task<Tuple<ErrorCode, AccountGame>> GetAccountGameInfoAsync(Int64 accountId)
    {
        try
        {
            var accountGameInfo = await _queryFactory.Query("AccountGame")
                .Where("AccountId", accountId)
                .FirstAsync<AccountGame>();

            if (accountGameInfo is null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
$"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetAccountGameInfoFailNotExist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, AccountGame>(ErrorCode.GetAccountGameInfoFailNotExist, null);
            }

            return new Tuple<ErrorCode, AccountGame>(ErrorCode.None, accountGameInfo);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
$"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetAccountGameInfoFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, AccountGame>(ErrorCode.GetAccountGameInfoFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<AccountItem>>> GetAccountItemListAsync(Int64 accountId)
    {
        try
        {
            var accountItemList = await _queryFactory.Query("AccountItem")
                .Where("AccountId", accountId)
                .GetAsync<AccountItem>();

            if (accountItemList is null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
$"[GameDb.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetAccountItemListFailNotExist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, IEnumerable<AccountItem>>(ErrorCode.GetAccountItemListFailNotExist, null);
            }

            return new Tuple<ErrorCode, IEnumerable<AccountItem>>(ErrorCode.None, accountItemList);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
$"[GameDb.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetAccountItemListFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, IEnumerable<AccountItem>>(ErrorCode.GetAccountItemListFailException, null);
        }
    }
}