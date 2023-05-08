using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApiForCollectingRPG.DAO;
using WebApiForCollectingRPG.DTO.Attendance;
using WebApiForCollectingRPG.DTO.Mail;
using WebApiForCollectingRPG.DTO.Game;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.DTO.InAppProduct;

namespace WebApiForCollectingRPG.Services;

public class GameDb : IGameDb
{
    private const Int32 PerPage = 20;
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;
    readonly IMasterService _masterService;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig, IMasterService masterService)
    {
        _dbConfig = dbConfig;
        _masterService = masterService;
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

    public async Task<Tuple<ErrorCode, AccountGameInfo>> GetAccountGameInfoAsync(Int64 accountId)
    {
        try
        {
            var accountGameInfo = await _queryFactory.Query("account_game")
                .Where("account_id", accountId)
                .Select("money AS Money",
                "exp AS Exp")
                .FirstOrDefaultAsync<AccountGameInfo>();

            if (accountGameInfo is null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetAccountGameInfoFailNotExist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, AccountGameInfo>(ErrorCode.GetAccountGameInfoFailNotExist, null);
            }

            return new Tuple<ErrorCode, AccountGameInfo>(ErrorCode.None, accountGameInfo);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetAccountGameInfoFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, AccountGameInfo>(ErrorCode.GetAccountGameInfoFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<AccountItemInfo>>> GetAccoutItemInfoListAsync(Int64 accountId)
    {
        try
        {
            var accountItemList = await _queryFactory.Query("account_item")
                .Where("account_id", accountId)
                .Select("item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic")
                .GetAsync<AccountItemInfo>();

            return new Tuple<ErrorCode, IEnumerable<AccountItemInfo>>(ErrorCode.None, accountItemList);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetAccountItemListFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, IEnumerable<AccountItemInfo>>(ErrorCode.GetAccountItemListFailException, null);
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
                .Where(new
                {
                    account_id = accountId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
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
                .Where(new
                {
                    account_id = accountId,
                    mail_id = mailId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
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

            var requestDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

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
                var lastAttendanceDate = Convert.ToDateTime(attendance.LastAttendanceDate.ToString("yyyy-MM-dd"));

                // 출석 요청일이 마지막 출석일과 일치할 경우
                if (DateTime.Compare(requestDate, lastAttendanceDate) == 0)
                {
                    _logger.ZLogError(EventIdDic[EventType.GameDb],
                        $"[GameDb.CheckAttendance] ErrorCode: {ErrorCode.DuplicateAttendance}, AccountId: {accountId}");
                    return ErrorCode.DuplicateAttendance;
                }
                // 연속 출석하지 않은 계정이거나 30일 출석 보상을 받은 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) != 0 || attendance.LastCompensationId == 30)
                {
                    await _queryFactory.Query("attendance").Where("account_id", accountId).UpdateAsync(new
                    {
                        last_compensation_id = 1,
                        last_attendance_date = requestDate
                    });
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
            var compensation = _masterService.GetAttendanceCompensationByCompensationId(attendance.LastCompensationId);
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

    public async Task<ErrorCode> ReceiveMailItems(Int64 accountId, Int64 mailId)
    {
        try
        {
            // 우편 조회
            var mail = await _queryFactory.Query("mail")
                .Where(new
                {
                    account_id = accountId,
                    mail_id = mailId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
                .Select("mail_id AS MailId",
                "account_id AS AccountId",
                "title AS Title",
                "content AS Content",
                "is_received AS IsReceived",
                "is_in_app_product AS IsInAppProduct",
                "created_at AS CreatedAt",
                "expiration_time AS ExpirationTime",
                "is_deleted AS IsDeleted")
                .FirstOrDefaultAsync<Mail>();

            if (mail == null || mail.MailId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailMailNotExist}, AccountId: {accountId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailMailNotExist;
            }
            else if (mail.IsReceived)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, AccountId: {accountId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailNotExist;
            }

            // 우편 아이템 조회
            var mailItems = await _queryFactory.Query("mail_item")
                .Where("mail_id", mailId)
                .Select("item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<MailItemInfo>();

            if (mailItems == null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, AccountId: {accountId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailNotExist;
            }

            // 우편 아이템 수령 처리
            await _queryFactory.Query("mail").Where("mail_id", mailId).UpdateAsync(new
            {
                is_received = true
            });


            foreach (MailItemInfo mailItemInfo in mailItems)
            {
                var itemInfo = _masterService.GetItemByItemId(mailItemInfo.ItemId);

                // 아이템이 돈일 경우
                if (_masterService.IsMoney(mailItemInfo.ItemId))
                {
                    var money = await _queryFactory.Query("account_game")
                        .Where("account_id", accountId)
                        .Select("money AS Money")
                        .FirstOrDefaultAsync<Int64>();

                    await _queryFactory.Query("account_game").Where("account_id", accountId).UpdateAsync(new
                    {
                        money = money + mailItemInfo.ItemCount
                    });
                }
                // 겹칠 수 있는 아이템일 경우
                else if (_masterService.IsStackableItem(mailItemInfo.ItemId))
                {
                    var accountItem = await _queryFactory.Query("account_item")
                        .Where("item_id", mailItemInfo.ItemId)
                        .Select("account_item_id AS AccountItemId",
                        "account_id AS AccountId",
                        "item_id AS ItemId",
                        "item_count AS ItemCount",
                        "enhance_count AS EnhanceCount",
                        "attack AS Attack",
                        "defence AS Defence",
                        "magic AS Magic")
                        .FirstOrDefaultAsync<AccountItem>();

                    // 해당 아이템을 보유하고 있지 않을 경우 AccountItem 생성
                    if (accountItem == null || accountItem.AccountItemId == 0)
                    {
                        await _queryFactory.Query("account_item").InsertAsync(new
                        {
                            account_id = accountId,
                            item_id = mailItemInfo.ItemId,
                            item_count = mailItemInfo.ItemCount,
                            enhance_count = 0,
                            attack = itemInfo.Attack,
                            defence = itemInfo.Defence,
                            magic = itemInfo.Magic
                        });
                    }
                    // 보유하고 있을 경우 현재 보유 수량에 더한다.
                    else
                    {
                        await _queryFactory.Query("account_item")
                            .Where("account_item_id", accountItem.AccountItemId)
                            .UpdateAsync(new
                            {
                                item_count = accountItem.ItemCount + mailItemInfo.ItemCount
                            });
                    }
                }
                // 겹칠 수 없는 아이템일 경우 AccountItem 생성
                else
                {
                    await _queryFactory.Query("account_item").InsertAsync(new
                    {
                        account_id = accountId,
                        item_id = mailItemInfo.ItemId,
                        item_count = mailItemInfo.ItemCount,
                        enhance_count = 0,
                        attack = itemInfo.Attack,
                        defence = itemInfo.Defence,
                        magic = itemInfo.Magic
                    });
                }
            }

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[GameDb.ReceiveMailItems] AccountId: {accountId}, MailId: {mailId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsException}, AccountId: {accountId}");
            return ErrorCode.ReceiveMailItemsException;
        }
    }

    public async Task<ErrorCode> SendInAppProduct(Int64 accountId, Int64 receiptId, Int16 productId)
    {
        try
        {
            var receipt = await _queryFactory.Query("receipt")
                .Where("receipt_id", receiptId)
                .FirstOrDefaultAsync<Receipt>();

            if (receipt != null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.SendInAppProduct] ErrorCode: {ErrorCode.ReceiptAlreadyUsed}, ReceiptId: {receiptId}");
                return ErrorCode.ReceiptAlreadyUsed;
            }

            await _queryFactory.Query("receipt").InsertAsync(new
            {
                receipt_id = receiptId,
                account_id = accountId,
                product_id = productId,
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[GameDb.SendInAppProduct] Add Receipt Success! ReceiptId: {receiptId} AccountId: {accountId}, ProductId: {productId}");

            SendMailInfo mail = new SendMailInfo(accountId,
                "인앱 상품(" + productId + ")" + " 구입에 따른 아이템 지급",
                "영수증 번호: " + receiptId +
                "\n상품 번호: " + productId,
                true,
                Convert.ToDateTime(DateTime.MaxValue.ToString("yyyy-MM-dd HH:mm:ss")));

            var inAppItems = _masterService.GetInAppItemsByProductId(productId);
            var mailItems = inAppItems.ConvertAll(inAppItem =>
            {
                return new MailItemInfo()
                {
                    ItemId = inAppItem.ItemId,
                    ItemCount = inAppItem.ItemCount
                };
            });

            return await SendRewardToMailbox(accountId, mail, mailItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.SendInAppProduct] ErrorCode: {ErrorCode.SendInAppProductException}, ReceiptId: {receiptId}");
            return ErrorCode.SendInAppProductException;
        }
    }

    private async Task<ErrorCode> SendRewardToMailbox(Int64 accountId, SendMailInfo mail, List<MailItemInfo> mailItems)
    {
        try
        {
            var mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(new
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

    public async Task<Tuple<ErrorCode, bool>> EnhanceItem(Int64 accountId, Int64 accountItemId)
    {
        try
        {
            // 로그인한 계정이 소유한 아이템이 맞는지 확인
            var accountItem = await _queryFactory.Query("account_item")
                .Where(new
                {
                    account_id = accountId,
                    account_item_id = accountItemId,
                })
                .Select("account_item_id AS AccountItemId",
                "account_id AS AccountId",
                "item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic")
                .FirstOrDefaultAsync<AccountItem>();

            if (accountItem == null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.AccountItemNotExist}, AccountId: {accountId}, AccountItemId: {accountItemId}");
                return new Tuple<ErrorCode, bool>(ErrorCode.AccountItemNotExist, false);
            }

            var item = _masterService.GetItemByItemId(accountItem.ItemId);

            // 강화 가능한 아이템인지 확인
            if (item.EnhanceMaxCount == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.NotEnchantableItem}, AccountId: {accountId}, AccountItemId: {accountItemId}, ItemId: {item.ItemId}");
                return new Tuple<ErrorCode, bool>(ErrorCode.NotEnchantableItem, false);
            }

            // 강화 횟수가 남아있는지 확인
            if (item.EnhanceMaxCount <= accountItem.EnhanceCount)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.OverMaxEnhanceCount}, AccountId: {accountId}, AccountItemId: {accountItemId}, ItemId: {item.ItemId}");
                return new Tuple<ErrorCode, bool>(ErrorCode.OverMaxEnhanceCount, false);
            }

            // 강화 단계에 따른 강화 성공 확률 적용
            /**
                강화 성공 확률(5단계까지) = 내림(1/목표 강화단계)
                강화 성공 확률(6단계부터) = 내림(내림(1/목표 강화단계)/2)  
                강화 성공 확률(10단계부터) = 내림(내림(내림(1/목표 강화단계)/2)/2)
                1단계: 100%
                2단계: 50%
                3단계: 33%
                4단계: 25%
                5단계: 20%
                6단계: 8%
                7단계: 7%
                8단계: 6%
                9단계: 5%
                10단계: 2%
            **/
            var targetEnhanceCount = accountItem.EnhanceCount + 1;
            var successPercent = Math.Truncate((double)1 / targetEnhanceCount * 100);
            if (targetEnhanceCount > 5)
            {
                successPercent = Math.Truncate(successPercent / 2);
            }
            if (targetEnhanceCount > 9)
            {
                successPercent = Math.Truncate(successPercent / 2);
            }

            // 강화 성공 확률에 따른 강화 시도
            double[] probs;
            var isSuccess = false;
            if (successPercent <= 50)
            {
                probs = new double[] { successPercent, 100 - successPercent };
                if (Util.Game.Choose(probs) == 0)
                {
                    isSuccess = true;
                }
            }
            else
            {
                probs = new double[] { 100 - successPercent, successPercent };
                if (Util.Game.Choose(probs) == 1)
                {
                    isSuccess = true;
                }
            }

            // 강화 성공
            if (isSuccess)
            {
                accountItem.Attack = (Int64)Math.Ceiling((double)accountItem.Attack * 1.1);
                accountItem.Defence = (Int64)Math.Ceiling((double)accountItem.Defence * 1.1);
                await _queryFactory.Query("account_item").Where("account_item_id", accountItemId).UpdateAsync(new
                {
                    attack = accountItem.Attack,
                    defence = accountItem.Defence,
                    enhance_count = accountItem.EnhanceCount + 1,
                });

                return new Tuple<ErrorCode, bool>(ErrorCode.None, true);
            }

            // 강화 실패
            await _queryFactory.Query("account_item").Where("account_item_id", accountItemId).DeleteAsync();
            return new Tuple<ErrorCode, bool>(ErrorCode.None, false);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
            $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.EnhanceItemException}, AccountId: {accountId}, AccountItemId: {accountItemId}");
            return new Tuple<ErrorCode, bool>(ErrorCode.EnhanceItemException, false);
        }
    }
}