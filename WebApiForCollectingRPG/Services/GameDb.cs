using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Dtos.Game;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class GameDb : IGameDb
{
    private const int PerPage = 20;
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
            await _queryFactory.Query("account_game").InsertAsync(new
            {
                account_id = accountId,
                money = 0,
                exp = 0
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

    public async Task<Tuple<ErrorCode, AccountGame>> GetAccountGameInfoAsync(Int64 accountId)
    {
        try
        {
            var accountGameInfo = await _queryFactory.Query("account_game")
                .Where("account_id", accountId)
                .Select("money AS Money",
                "exp AS Exp")
                .FirstOrDefaultAsync<AccountGame>();

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
            var accountItemList = await _queryFactory.Query("account_item")
                .Where("account_id", accountId)
                .Select("item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount")
                .GetAsync<AccountItem>();

            return new Tuple<ErrorCode, IEnumerable<AccountItem>>(ErrorCode.None, accountItemList);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
$"[GameDb.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetAccountItemListFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, IEnumerable<AccountItem>>(ErrorCode.GetAccountItemListFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int64 accountId, Int32 page)
    {
        if (page <= 0)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb],
$"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, AccountId: {accountId}, Page: {page}");
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailNotExistPage, null);
        }

        try
        {
            var mails = await _queryFactory.Query("mail")
                .Where("account_id", accountId)
                .Where("is_deleted", false)
                .Select("mail_id AS MailId", "title AS Title", "is_received AS IsReceived", "expiration_time AS ExpirationTime")
                .OrderByDesc("created_at")
                .PaginateAsync<MailListInfo>(page, PerPage);

            if (page > mails.TotalPages)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
$"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, AccountId: {accountId}, Page: {page}");
                return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailNotExistPage, null);
            }

            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.None, mails.List);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
$"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailException}, AccountId: {accountId}, Page: {page}");
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, MailDetailInfo>> GetMailByMailId(Int64 accountId, Int64 mailId)
    {
        try
        {
            var mail = await _queryFactory.Query("mail")
                .Where("account_id", accountId)
                .Where("mail_id", mailId)
                .Select("mail_id AS MailId", 
                "title AS Title", 
                "content AS Content",
                "is_received AS IsReceived",
                "is_in_app_product AS IsInAppProduct",
                "created_at AS CreatedAt",
                "expiration_time AS ExpirationTime")
                .FirstOrDefaultAsync<MailDetailInfo>();

            if (mail == null || mailId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
    $"[GameDb.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailNotExist}, AccountId: {accountId}, MailId: {mailId}");
                return new Tuple<ErrorCode, MailDetailInfo>(ErrorCode.GetMailFailNotExist, null);
            }

            return new Tuple<ErrorCode, MailDetailInfo>(ErrorCode.None, mail);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
$"[GameDb.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailException}, AccountId: {accountId}, MailId: {mailId}");
            return new Tuple<ErrorCode, MailDetailInfo>(ErrorCode.GetMailFailException, null);
        }
    }
}