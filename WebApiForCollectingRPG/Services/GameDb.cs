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
using WebApiForCollectingRPG.Repository;
using Microsoft.AspNetCore.Http;

namespace WebApiForCollectingRPG.Services;

public class GameDb : IGameDb
{
    private const Int32 PerPage = 20;
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameDb> _logger;
    readonly IMasterService _masterService;
    readonly IHttpContextAccessor _httpContextAccessor;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> dbConfig, IMasterService masterService, IHttpContextAccessor httpContextAccessor)
    {
        _dbConfig = dbConfig;
        _masterService = masterService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

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

    public async Task<Tuple<ErrorCode, Int64>> CreatePlayerAsync(Int64 accountId)
    {
        try
        {
            Int64 playerId = await _queryFactory.Query("account_player").InsertGetIdAsync<Int64>(new
            {
                account_id = accountId
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[CreatePlayer] AccountId: {accountId}, PlayerId:{playerId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, playerId);

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[CreatePlayer] ErrorCode: {ErrorCode.CreatePlayerFailException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.CreatePlayerFailException, 0);
        }
    }

    public async Task<Tuple<ErrorCode, Int64>> FindPlayerIdByAccountId(Int64 accountId)
    {
        try
        {
            Int64 playerId = await _queryFactory.Query("account_player")
                .Where("account_id", accountId)
                .Select("player_id AS PlayerId")
                .FirstOrDefaultAsync<Int64>();

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[FindPlayerIdByAccountId] AccountId: {accountId}, PlayerId:{playerId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, playerId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[FindPlayerIdByAccountId] ErrorCode: {ErrorCode.FindPlayerIdByAccountIdException}, AccountId: {accountId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.FindPlayerIdByAccountIdException, 0);
        }
    }

    public async Task<ErrorCode> CreatePlayerGameDataAsync(Int64 playerId)
    {
        try
        {

            await _queryFactory.Query("player_game").InsertAsync(new
            {
                player_id = playerId,
                money = 0,
                exp = 0
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[GameDb.CreatePlayerGameDataAsync] PlayerId: {playerId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.CreatePlayerGameDataAsync] ErrorCode: {ErrorCode.CreatePlayerGameFailException}");
        }
        return ErrorCode.CreatePlayerGameFailException;
    }

    public async Task<Tuple<ErrorCode, PlayerGameInfo>> GetPlayerGameInfoAsync(Int64 playerId)
    {
        try
        {
            var accountGameInfo = await _queryFactory.Query("player_game")
                .Where("player_id", playerId)
                .Select("money AS Money",
                "exp AS Exp")
                .FirstOrDefaultAsync<PlayerGameInfo>();

            if (accountGameInfo is null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetPlayerGameInfoFailNotExist}, PlayerId: {playerId}");
                return new Tuple<ErrorCode, PlayerGameInfo>(ErrorCode.GetPlayerGameInfoFailNotExist, null);
            }

            return new Tuple<ErrorCode, PlayerGameInfo>(ErrorCode.None, accountGameInfo);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetPlayerGameInfoFailException}");
            return new Tuple<ErrorCode, PlayerGameInfo>(ErrorCode.GetPlayerGameInfoFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<PlayerItemInfo>>> GetPlayerItemInfoListAsync(Int64 playerId)
    {
        try
        {
            var accountItemList = await _queryFactory.Query("player_item")
                .Where("player_id", playerId)
                .Select("item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic")
                .GetAsync<PlayerItemInfo>();

            return new Tuple<ErrorCode, IEnumerable<PlayerItemInfo>>(ErrorCode.None, accountItemList);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetPlayerItemListFailException}");
            return new Tuple<ErrorCode, IEnumerable<PlayerItemInfo>>(ErrorCode.GetPlayerItemListFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<MailListInfo>>> GetMailsByPage(Int32 page)
    {
        var (errorCode, playerId) = GetPlayerIdFromHttpContext();
        if (errorCode != ErrorCode.None)
        {
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(errorCode, null);
        }

        if (page <= 0)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb],
                $"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, PlayerId: {playerId}, Page: {page}");
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailNotExistPage, null);
        }

        try
        {
            var mails = await _queryFactory.Query("mail")
                .Where(new
                {
                    player_id = playerId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
                .Select("mail_id AS MailId", "title AS Title", "is_received AS IsReceived", "expiration_time AS ExpirationTime")
                .OrderByDesc("created_at")
                .PaginateAsync<MailListInfo>(page, PerPage);

            if (page > mails.TotalPages)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, PlayerId: {playerId}, Page: {page}");
                return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailNotExistPage, null);
            }

            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.None, mails.List);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailException}, PlayerId: {playerId}, Page: {page}");
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>> GetMailByMailId(Int64 mailId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>(errorCode, null, null);
            }

            var mail = await _queryFactory.Query("mail")
                .Where(new
                {
                    player_id = playerId,
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
                    $"[GameDb.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
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
                $"[GameDb.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailException}, MailId: {mailId}");
            return new Tuple<ErrorCode, MailDetailInfo, IEnumerable<MailItemInfo>>(ErrorCode.GetMailFailException, null, null);
        }
    }

    public async Task<ErrorCode> CheckAttendance()
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            var attendance = await _queryFactory.Query("attendance")
                .Where("player_id", playerId)
                .Select("last_compensation_id AS LastCompensationId",
                "last_attendance_date AS LastAttendanceDate")
                .FirstOrDefaultAsync<AttendanceInfo>();

            var requestDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            // 한 번도 출석하지 않은 계정일 때
            if (attendance == null)
            {
                await _queryFactory.Query("attendance").InsertAsync(new
                {
                    player_id = playerId,
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
                        $"[GameDb.CheckAttendance] ErrorCode: {ErrorCode.DuplicateAttendance}, AccountId: {playerId}");
                    return ErrorCode.DuplicateAttendance;
                }
                // 연속 출석하지 않은 계정이거나 30일 출석 보상을 받은 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) != 0 || attendance.LastCompensationId == 30)
                {
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = 1,
                        last_attendance_date = requestDate
                    });
                }
                // 연속 출석한 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) == 0)
                {
                    attendance.LastCompensationId++;
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = attendance.LastCompensationId++,
                        last_attendance_date = requestDate
                    });
                }
            }

            // 보상과 함께 우편 보내기
            attendance = await _queryFactory.Query("attendance")
                .Where("player_id", playerId)
                .Select("last_compensation_id AS LastCompensationId",
                "last_attendance_date AS LastAttendanceDate")
                .FirstOrDefaultAsync<AttendanceInfo>();

            SendMailInfo mail = new SendMailInfo(playerId.Value,
                attendance.LastCompensationId + "일 차 출석 보상",
                attendance.LastCompensationId + "일 차 출석 보상입니다.",
                false,
                DateTime.Now.AddDays(7));

            List<MailItemInfo> mailItems = new();
            var compensation = _masterService.GetAttendanceCompensationByCompensationId(attendance.LastCompensationId);
            mailItems.Add(new MailItemInfo(compensation.ItemId, compensation.ItemCount));

            return await SendRewardToMailbox(mail, mailItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.CheckAttendance] ErrorCode: {ErrorCode.AttendanceInfoException}");
            return ErrorCode.AttendanceInfoException;
        }
    }

    public async Task<ErrorCode> ReceiveMailItems(Int64 mailId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            // 우편 조회
            var mail = await _queryFactory.Query("mail")
                .Where(new
                {
                    player_id = playerId,
                    mail_id = mailId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
                .Select("mail_id AS MailId",
                "player_id AS PlayerId",
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
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailMailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailMailNotExist;
            }
            else if (mail.IsReceived)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
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
                    $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
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
                    var money = await _queryFactory.Query("player_game")
                        .Where("player_id", playerId)
                        .Select("money AS Money")
                        .FirstOrDefaultAsync<Int64>();

                    await _queryFactory.Query("player_game").Where("player_id", playerId).UpdateAsync(new
                    {
                        money = money + mailItemInfo.ItemCount
                    });
                }
                // 겹칠 수 있는 아이템일 경우
                else if (_masterService.IsStackableItem(mailItemInfo.ItemId))
                {
                    var playerItem = await _queryFactory.Query("player_item")
                        .Where("item_id", mailItemInfo.ItemId)
                        .Select("player_item_id AS PlayerItemId",
                        "player_id AS PlayerId",
                        "item_id AS ItemId",
                        "item_count AS ItemCount",
                        "enhance_count AS EnhanceCount",
                        "attack AS Attack",
                        "defence AS Defence",
                        "magic AS Magic")
                        .FirstOrDefaultAsync<PlayerItem>();

                    // 해당 아이템을 보유하고 있지 않을 경우 AccountItem 생성
                    if (playerItem == null || playerItem.PlayerItemId == 0)
                    {
                        await _queryFactory.Query("player_item").InsertAsync(new
                        {
                            player_id = playerId,
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
                        await _queryFactory.Query("player_item")
                            .Where("player_item_id", playerItem.PlayerItemId)
                            .UpdateAsync(new
                            {
                                item_count = playerItem.ItemCount + mailItemInfo.ItemCount
                            });
                    }
                }
                // 겹칠 수 없는 아이템일 경우 AccountItem 생성
                else
                {
                    await _queryFactory.Query("player_item").InsertAsync(new
                    {
                        player_id = playerId,
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
                $"[GameDb.ReceiveMailItems] PlayerId: {playerId}, MailId: {mailId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsException}");
            return ErrorCode.ReceiveMailItemsException;
        }
    }

    public async Task<ErrorCode> SendInAppProduct(Int64 receiptId, Int16 productId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

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
                player_id = playerId,
                product_id = productId,
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameDb],
                $"[GameDb.SendInAppProduct] Add Receipt Success! ReceiptId: {receiptId} PlayerId: {playerId}, ProductId: {productId}");

            SendMailInfo mail = new SendMailInfo(playerId.Value,
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

            return await SendRewardToMailbox(mail, mailItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.SendInAppProduct] ErrorCode: {ErrorCode.SendInAppProductException}, ReceiptId: {receiptId}");
            return ErrorCode.SendInAppProductException;
        }
    }

    private async Task<ErrorCode> SendRewardToMailbox(SendMailInfo mail, List<MailItemInfo> mailItems)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            var mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(new
            {
                player_id = playerId,
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
                $"[GameDb.SendRewardToMailbox] PlayerId: {playerId}, MailId: {mailId}, MailItemCount: {mailItems.Count}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.SendRewardToMailbox] ErrorCode: {ErrorCode.SendRewardToMailboxException}, MailTitle: {mail.Title}, MailItemCount: {mailItems.Count}");
            return ErrorCode.SendRewardToMailboxException;
        }
    }

    public async Task<Tuple<ErrorCode, bool>> EnhanceItem(Int64 playerItemId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, bool>(errorCode, false);
            }

            // 로그인한 계정이 소유한 아이템이 맞는지 확인
            var playerItem = await _queryFactory.Query("player_item")
                .Where(new
                {
                    player_id = playerId,
                    player_item_id = playerItemId,
                })
                .Select("player_item_id AS PlayerItemId",
                "player_id AS PlayerId",
                "item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic")
                .FirstOrDefaultAsync<PlayerItem>();

            if (playerItem == null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.PlayerItemNotExist}, PlayerId: {playerId}, PlayerItemId: {playerItemId}");
                return new Tuple<ErrorCode, bool>(ErrorCode.PlayerItemNotExist, false);
            }

            var item = _masterService.GetItemByItemId(playerItem.ItemId);

            // 강화 가능한 아이템인지 확인
            if (item.EnhanceMaxCount == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.NotEnchantableItem}, PlayerId: {playerId}, PlayerItemId: {playerItemId}, ItemId: {item.ItemId}");
                return new Tuple<ErrorCode, bool>(ErrorCode.NotEnchantableItem, false);
            }

            // 강화 횟수가 남아있는지 확인
            if (item.EnhanceMaxCount <= playerItem.EnhanceCount)
            {
                _logger.ZLogError(EventIdDic[EventType.GameDb],
                    $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.OverMaxEnhanceCount}, PlayerId: {playerId}, PlayerItemId: {playerItemId}, ItemId: {item.ItemId}");
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
            var targetEnhanceCount = playerItem.EnhanceCount + 1;
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

            if (!isSuccess)
            {
                await _queryFactory.Query("player_item").Where("player_item_id", playerItemId).DeleteAsync();
                return new Tuple<ErrorCode, bool>(ErrorCode.None, false);
            }

            playerItem.Attack = (Int64)Math.Ceiling((double)playerItem.Attack * 1.1);
            playerItem.Defence = (Int64)Math.Ceiling((double)playerItem.Defence * 1.1);
            await _queryFactory.Query("player_item").Where("player_item_id", playerItemId).UpdateAsync(new
            {
                attack = playerItem.Attack,
                defence = playerItem.Defence,
                enhance_count = playerItem.EnhanceCount + 1,
            });

            return new Tuple<ErrorCode, bool>(ErrorCode.None, true);

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb], ex,
                $"[GameDb.EnhanceItem] ErrorCode: {ErrorCode.EnhanceItemException}");
            return new Tuple<ErrorCode, bool>(ErrorCode.EnhanceItemException, false);
        }
    }

    private (ErrorCode, Int64?) GetPlayerIdFromHttpContext()
    {
        var playerId = (_httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser)?.PlayerId;

        if (playerId == null)
        {
            _logger.ZLogError(EventIdDic[EventType.GameDb],
                $"[GameDb.GetPlayerIdFromHttpContext] ErrorCode: {ErrorCode.PlayerIdNotExist}");
            return new(ErrorCode.PlayerIdNotExist, playerId);
        }

        return (ErrorCode.None, playerId);
    }
}