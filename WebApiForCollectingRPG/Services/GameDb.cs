﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DTO.Attendance;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.Dtos.Game;
using WebApiForCollectingRPG.Repository;
using ZLogger;
using static LogManager;

namespace WebApiForCollectingRPG.Services;

public class GameDb : IGameDb
{
    private const Int32 PerPage = 20;
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;
    readonly IMasterDb _masterDb;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig, IMasterDb masterDb)
    {
        _dbConfig = dbConfig;
        _masterDb = masterDb;
        _logger = logger;

        Open();

        _compiler = new MySqlCompiler();
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

    public async Task<Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>> GetMailByMailId(Int64 accountId, Int64 mailId)
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
                return new Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>(ErrorCode.GetMailFailNotExist, null, null);
            }

            var items = await _queryFactory.Query("mail_item")
                .Where("mail_id", mailId)
                .Select("item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<MailItemInfo>();

            return new Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>(ErrorCode.None, mail, items);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailException}, AccountId: {accountId}, MailId: {mailId}");
            return new Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>(ErrorCode.GetMailFailException, null, null);
        }
    }

    public async Task<ErrorCode> CheckAttendance(Int64 accountId)
    {
        try
        {
            var attendance = await _queryFactory.Query("attendance")
                .Where("account_id", accountId)
                .Select("last_compensation_id AS LastCompensationId",
                "last_attendance_date AS LastAttendanceDate")
                .FirstOrDefaultAsync<AttendanceInfo>();

            var requestDate = Convert.ToDateTime(DateTime.Now.ToString("yy-MM-dd"));

            // 한 번도 출석하지 않은 계정일 때
            if (attendance == null)
            {
                await _queryFactory.Query("attendance").InsertAsync(new
                {
                    account_id = accountId,
                    last_compensation_id = 1,
                    last_attendance_date = requestDate
                });
            }
            else
            {
                var lastAttendanceDate = Convert.ToDateTime(attendance.LastAttendanceDate.ToString("yy-MM-dd"));

                // 출석 요청일이 마지막 출석일과 일치할 경우
                if (DateTime.Compare(requestDate, lastAttendanceDate) == 0)
                {
                    _logger.ZLogError(EventIdDic[EventType.GameDb],
                        $"[GameDb.CheckAttendance] ErrorCode: {ErrorCode.DuplicateAttendance}, AccountId: {accountId}");
                    return ErrorCode.DuplicateAttendance;
                }
                // 연속 출석한 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) == 0)
                {
                    attendance.LastCompensationId++;
                    await _queryFactory.Query("attendance").Where("account_id", accountId).UpdateAsync(new
                    {
                        last_compensation_id = attendance.LastCompensationId++,
                        last_attendance_date = requestDate
                    });
                }
                // 연속 출석하지 않은 계정일 때
                else
                {
                    await _queryFactory.Query("attendance").Where("account_id", accountId).UpdateAsync(new
                    {
                        last_compensation_id = 1,
                        last_attendance_date = requestDate
                    });
                }
            }

            // 보상과 함께 우편 보내기
            attendance = await _queryFactory.Query("attendance")
                .Where("account_id", accountId)
                .Select("last_compensation_id AS LastCompensationId",
                "last_attendance_date AS LastAttendanceDate")
                .FirstOrDefaultAsync<AttendanceInfo>();

            SendMailInfo mail = new SendMailInfo(accountId,
                attendance.LastCompensationId + "일 차 출석 보상",
                attendance.LastCompensationId + "일 차 출석 보상입니다.",
                false,
                DateTime.Now.AddDays(7));

            List<MailItemInfo> mailItems = new();
            var compensation = _masterDb.GetAttendanceCompensationByCompensationId(attendance.LastCompensationId);
            mailItems.Add(new MailItemInfo(compensation.ItemId, compensation.ItemCount));

            return await SendRewardToMailbox(accountId, mail, mailItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.CheckAttendance] ErrorCode: {ErrorCode.AttendanceInfoException}, AccountId: {accountId}");
            return ErrorCode.AttendanceInfoException;
        }
    }

    private async Task<ErrorCode> SendRewardToMailbox(Int64 accountId, SendMailInfo mail, List<MailItemInfo> mailItems)
    {
        try
        {
            Int64 mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(new
            {
                account_id = accountId,
                title = mail.Title,
                content = mail.Content,
                is_received = mail.IsReceived,
                is_in_app_product = mail.IsInAppProduct,
                expiration_time = mail.ExpirationTime,
                is_deleted = mail.IsDeleted
            });

            foreach (MailItemInfo item in mailItems)
            {
                await _queryFactory.Query("mail_item").InsertAsync(new
                {
                    mail_id = mailId,
                    item_id = item.ItemId,
                    item_count = item.ItemCount
                });
            }

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[GameDb.SendRewardToMailbox] AccountId: {accountId}, MailId: {mailId}, MailItemCount: {mailItems.Count}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.SendRewardToMailbox] ErrorCode: {ErrorCode.SendRewardToMailboxException}, AccountId: {accountId}, MailTitle: {mail.Title}, MailItemCount: {mailItems.Count}");
            return ErrorCode.SendRewardToMailboxException;
        }
    }
}