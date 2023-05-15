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
using WebApiForCollectingRPG.DTO.Player;
using ZLogger;
using static LogManager;
using WebApiForCollectingRPG.Repository;
using Microsoft.AspNetCore.Http;
using WebApiForCollectingRPG.DTO.Enhance;
using WebApiForCollectingRPG.DTO.Dungeon;

namespace WebApiForCollectingRPG.Services;

public class GameService : IGameService
{
    private const Int32 PerPage = 20;
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<GameService> _logger;
    readonly IMemoryCacheService _memoryCacheService;
    readonly IHttpContextAccessor _httpContextAccessor;

    IDbConnection _dbConn;
    MySqlCompiler _compiler;
    QueryFactory _queryFactory;

    public GameService(ILogger<GameService> logger, IOptions<DbConfig> dbConfig, IMemoryCacheService memoryCacheService, IHttpContextAccessor httpContextAccessor)
    {
        _dbConfig = dbConfig;
        _memoryCacheService = memoryCacheService;
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
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
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

            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[CreatePlayer] AccountId: {accountId}, PlayerId:{playerId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, playerId);

        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
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

            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[FindPlayerIdByAccountId] AccountId: {accountId}, PlayerId:{playerId}");
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, playerId);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
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

            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[GameService.CreatePlayerGameDataAsync] PlayerId: {playerId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.CreatePlayerGameDataAsync] ErrorCode: {ErrorCode.CreatePlayerGameFailException}");
        }
        return ErrorCode.CreatePlayerGameFailException;
    }

    public async Task<Tuple<ErrorCode, PlayerGameDTO>> GetPlayerGameInfoAsync(Int64 playerId)
    {
        try
        {
            var accountGameInfo = await _queryFactory.Query("player_game")
                .Where("player_id", playerId)
                .Select("money AS Money",
                "exp AS Exp")
                .FirstOrDefaultAsync<PlayerGameDTO>();

            if (accountGameInfo is null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetPlayerGameInfoFailNotExist}, PlayerId: {playerId}");
                return new Tuple<ErrorCode, PlayerGameDTO>(ErrorCode.GetPlayerGameInfoFailNotExist, null);
            }

            return new Tuple<ErrorCode, PlayerGameDTO>(ErrorCode.None, accountGameInfo);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetAccountGameInfoAsync] ErrorCode: {ErrorCode.GetPlayerGameInfoFailException}");
            return new Tuple<ErrorCode, PlayerGameDTO>(ErrorCode.GetPlayerGameInfoFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<PlayerItemDTO>>> GetPlayerItemInfoListAsync(Int64 playerId)
    {
        try
        {
            var accountItemList = await _queryFactory.Query("player_item")
                .Where("player_id", playerId)
                .Select("player_item_id AS PlayerItemId",
                "item_id AS ItemId",
                "item_count AS ItemCount",
                "enhance_count AS EnhanceCount",
                "attack AS Attack",
                "defence AS Defence",
                "magic AS Magic")
                .GetAsync<PlayerItemDTO>();

            return new Tuple<ErrorCode, IEnumerable<PlayerItemDTO>>(ErrorCode.None, accountItemList);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetAccountItemListAsync] ErrorCode: {ErrorCode.GetPlayerItemListFailException}");
            return new Tuple<ErrorCode, IEnumerable<PlayerItemDTO>>(ErrorCode.GetPlayerItemListFailException, null);
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
            _logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, PlayerId: {playerId}, Page: {page}");
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
                .Select("mail_id AS MailId",
                "title AS Title",
                "is_received AS IsReceived",
                "expiration_time AS ExpirationTime",
                "is_read AS IsRead",
                "has_item AS HasItem")
                .OrderByDesc("created_at")
                .PaginateAsync<MailListInfo>(page, PerPage);

            if (page > mails.TotalPages)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailNotExistPage}, PlayerId: {playerId}, Page: {page}");
                return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailNotExistPage, null);
            }

            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.None, mails.List);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetMailsByPage] ErrorCode: {ErrorCode.GetMailsFailException}, PlayerId: {playerId}, Page: {page}");
            return new Tuple<ErrorCode, IEnumerable<MailListInfo>>(ErrorCode.GetMailsFailException, null);
        }
    }

    public async Task<Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>> GetMailByMailId(Int64 mailId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>(errorCode, null, null);
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
                .FirstOrDefaultAsync<MailDetail>();

            if (mail == null || mailId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
                return new Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>(ErrorCode.GetMailFailNotExist, null, null);
            }

            var items = await _queryFactory.Query("mail_item")
                .Where("mail_id", mailId)
                .Select("item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<ItemDTO>();

            await _queryFactory.Query("mail").Where("mail_id", mailId).UpdateAsync(new
            {
                is_read = true
            });

            return new Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>(ErrorCode.None, mail, items);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetMailByMailId] ErrorCode: {ErrorCode.GetMailFailException}, MailId: {mailId}");
            return new Tuple<ErrorCode, MailDetail, IEnumerable<ItemDTO>>(ErrorCode.GetMailFailException, null, null);
        }
    }

    public async Task<ErrorCode> CheckAttendance()
    {
        Int16 todayCompensationId = 1;
        AttendanceInfo attendance = null;
        Int64? playerId = 0;
        try
        {
            (var errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            attendance = await _queryFactory.Query("attendance")
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
                    last_compensation_id = todayCompensationId,
                    last_attendance_date = requestDate
                });
            }
            else
            {
                var lastAttendanceDate = Convert.ToDateTime(attendance.LastAttendanceDate.ToString("yyyy-MM-dd"));

                // 출석 요청일이 마지막 출석일과 일치할 경우
                if (DateTime.Compare(requestDate, lastAttendanceDate) == 0)
                {
                    _logger.ZLogError(EventIdDic[EventType.GameService],
                        $"[GameService.CheckAttendance] ErrorCode: {ErrorCode.DuplicateAttendance}, AccountId: {playerId}");
                    return ErrorCode.DuplicateAttendance;
                }
                // 연속 출석하지 않은 계정이거나 30일 출석 보상을 받은 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) != 0 || attendance.LastCompensationId == 30)
                {
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = todayCompensationId,
                        last_attendance_date = requestDate
                    });
                }
                // 연속 출석한 계정일 때
                else if (DateTime.Compare(lastAttendanceDate.AddDays(1), requestDate) == 0)
                {
                    todayCompensationId = (short)(attendance.LastCompensationId + 1);
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = todayCompensationId,
                        last_attendance_date = requestDate
                    });
                }
            }

            // 보상과 함께 우편 보내기
            SendMailDTO mail = new(playerId.Value,
                todayCompensationId + "일 차 출석 보상",
                todayCompensationId + "일 차 출석 보상입니다.",
                false,
                true,
                DateTime.Now.AddDays(7));

            List<ItemDTO> mailItems = new();
            var compensation = _memoryCacheService.GetAttendanceCompensationByCompensationId(todayCompensationId);
            mailItems.Add(new ItemDTO(compensation.ItemId, compensation.ItemCount));

            errorCode = await SendRewardToMailbox(mail, mailItems);
            if (errorCode != ErrorCode.None)
            {
                // 우편 보내는 중 오류 발생했을 경우 출석부 rollback
                if (attendance == null) // 출석 기록 행을 새로 만들었을 경우 삭제
                {
                    attendance = await _queryFactory.Query("attendance")
                        .Where("player_id", playerId)
                        .Select("last_compensation_id AS LastCompensationId",
                        "last_attendance_date AS LastAttendanceDate")
                        .FirstOrDefaultAsync<AttendanceInfo>();
                    if (attendance != null)
                    {
                        await _queryFactory.Query("attendance").Where("player_id", playerId).DeleteAsync();
                    }
                }
                else if (todayCompensationId == 1) // 기존 출석 기록이 있는 경우 기존 기록으로 갱신
                {
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = attendance.LastCompensationId,
                        last_attendance_date = attendance.LastAttendanceDate
                    });
                }
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.CheckAttendance] ErrorCode: {ErrorCode.SendRewardToMailboxError}, " +
                    $"Message: An error occurred while check attendance, so has been rolled back.");
                return ErrorCode.SendRewardToMailboxError;
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            // 출석체크 도중 예외 발생했을 경우 rollback
            if (playerId != 0)
            {
                if (attendance == null) // 출석 기록이 없는 경우
                {
                    await _queryFactory.Query("attendance").Where("player_id", playerId).DeleteAsync();
                }
                else if (todayCompensationId == 1) // 출석 기록이 있는 경우
                {
                    await _queryFactory.Query("attendance").Where("player_id", playerId).UpdateAsync(new
                    {
                        last_compensation_id = attendance.LastCompensationId,
                        last_attendance_date = attendance.LastAttendanceDate
                    });
                }
            }

            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.CheckAttendance] ErrorCode: {ErrorCode.CheckAttendanceException}, " +
                $"Message: An error occurred while check attendance, so has been rolled back.");
            return ErrorCode.CheckAttendanceException;
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

            var mail = await _queryFactory.Query("mail")
                .Where(new
                {
                    player_id = playerId,
                    mail_id = mailId,
                    is_deleted = false,
                })
                .Where("expiration_time", ">", DateTime.Now)
                .Select("mail_id AS MailId",
                "is_received AS IsReceived",
                "has_item AS HasItem")
                .FirstOrDefaultAsync<MailDTO>();

            if (mail == null || mail.MailId == 0)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailMailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailMailNotExist;
            }
            else if (mail.IsReceived || !mail.HasItem)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailNotExist;
            }

            var mailItems = await _queryFactory.Query("mail_item")
                .Where("mail_id", mailId)
                .Select("item_id AS ItemId", "item_count AS ItemCount")
                .GetAsync<ItemDTO>();

            if (mailItems == null)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsFailNotExist}, PlayerId: {playerId}, MailId: {mailId}");
                return ErrorCode.ReceiveMailItemsFailNotExist;
            }

            await _queryFactory.Query("mail").Where("mail_id", mailId).UpdateAsync(new
            {
                is_read = true,
                is_received = true,
                has_item = false
            });

            errorCode = await ReceiveItemsActions(playerId, mailItems);
            if (errorCode != ErrorCode.None)
            {
                // mail rollback
                await _queryFactory.Query("mail").Where("mail_id", mailId).UpdateAsync(new
                {
                    is_read = false,
                    is_received = false,
                    has_item = true
                });

                return errorCode;
            }
            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[GameService.ReceiveMailItems] PlayerId: {playerId}, MailId: {mailId}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.ReceiveMailItems] ErrorCode: {ErrorCode.ReceiveMailItemsException}");
            return ErrorCode.ReceiveMailItemsException;
        }
    }

    private async Task<ErrorCode> ReceiveItemsActions(long? playerId, IEnumerable<ItemDTO> items)
    {
        // 롤백을 위해 동작 번호와 사용된 데이터(Money, PlayerItemId, ItemCount)를 가진다. 
        List<(Int16, Int64, Int64, Int32)> rollbacks = new();
        try
        {
            foreach (ItemDTO item in items)
            {
                var itemInfo = _memoryCacheService.GetItemByItemId(item.ItemId);

                if (_memoryCacheService.IsMoney(item.ItemId))
                {
                    var money = await _queryFactory.Query("player_game")
                        .Where("player_id", playerId)
                        .Select("money AS Money")
                        .FirstOrDefaultAsync<Int64>();

                    await _queryFactory.Query("player_game").Where("player_id", playerId).UpdateAsync(new
                    {
                        money = money + item.ItemCount
                    });
                    rollbacks.Add((1, money, 0, 0)); // Money 갱신 동작, 기존 Money 데이터
                }
                else if (_memoryCacheService.IsStackableItem(item.ItemId))
                {
                    var playerItem = await _queryFactory.Query("player_item")
                        .Where(new
                        {
                            item_id = item.ItemId,
                            player_id = playerId,
                        })
                        .Select("player_item_id AS PlayerItemId",
                        "player_id AS PlayerId",
                        "item_id AS ItemId",
                        "item_count AS ItemCount")
                        .FirstOrDefaultAsync<PlayerStackableItemDTO>();

                    // 해당 아이템을 보유하고 있지 않을 경우 PlayerItem 생성
                    if (playerItem == null || playerItem.PlayerItemId == 0)
                    {
                        var playerItemId = await _queryFactory.Query("player_item").InsertGetIdAsync<Int64>(new
                        {
                            player_id = playerId,
                            item_id = item.ItemId,
                            item_count = item.ItemCount,
                            enhance_count = itemInfo.EnhanceMaxCount,
                            attack = itemInfo.Attack,
                            defence = itemInfo.Defence,
                            magic = itemInfo.Magic
                        });
                        rollbacks.Add((2, 0, playerItemId, item.ItemCount)); // PlayerItem 생성 동작
                    }
                    else
                    {
                        await _queryFactory.Query("player_item")
                            .Where("player_item_id", playerItem.PlayerItemId)
                            .UpdateAsync(new
                            {
                                item_count = playerItem.ItemCount + item.ItemCount
                            });
                        rollbacks.Add((3, 0, playerItem.PlayerItemId, playerItem.ItemCount)); // PlayerItem 갱신 동작, 기존 ItemCount 데이터
                    }
                }
                else
                {
                    var playerItemId = await _queryFactory.Query("player_item").InsertGetIdAsync<Int64>(new
                    {
                        player_id = playerId,
                        item_id = item.ItemId,
                        item_count = item.ItemCount,
                        enhance_count = 0,
                        attack = itemInfo.Attack,
                        defence = itemInfo.Defence,
                        magic = itemInfo.Magic
                    });
                    rollbacks.Add((2, 0, playerItemId, item.ItemCount)); // PlayerItem 생성 동작
                }
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            try
            {
                foreach (var rollback in rollbacks)
                {
                    var (actionId, OriginMoney, playerItemId, OriginItemCount) = rollback;
                    switch (actionId)
                    {
                        case 1: // Money 갱신 롤백
                            await _queryFactory.Query("player_game").Where("player_id", playerId).UpdateAsync(new
                            {
                                money = OriginMoney
                            });
                            break;
                        case 2: // Item 생성 롤백
                            await _queryFactory.Query("player_item").Where("player_item_id", playerItemId).DeleteAsync();
                            break;
                        case 3: // ItemCount 갱신 롤백
                            await _queryFactory.Query("player_item").Where("player_item_id", playerItemId).UpdateAsync(new
                            {
                                item_count = OriginItemCount
                            });
                            break;
                    }
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService], rollbackEx,
                    $"[GameService.ReceiveMailItemActions] ErrorCode: {ErrorCode.ReceiveItemsActionsRollbackException}, Message: Exception occurred during rollback.");
                return ErrorCode.ReceiveItemsActionsRollbackException;
            }

            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.ReceiveItemsActions] ErrorCode: {ErrorCode.ReceiveItemsActionsException}, Message: An error occurred while receiving items, so has been rolled back.");
            return ErrorCode.ReceiveItemsActionsException;
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
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.SendInAppProduct] ErrorCode: {ErrorCode.ReceiptAlreadyUsed}, ReceiptId: {receiptId}");
                return ErrorCode.ReceiptAlreadyUsed;
            }

            await _queryFactory.Query("receipt").InsertAsync(new
            {
                receipt_id = receiptId,
                player_id = playerId,
                product_id = productId,
            });

            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[GameService.SendInAppProduct] Add Receipt Success! ReceiptId: {receiptId} PlayerId: {playerId}, ProductId: {productId}");

            SendMailDTO mail = new(playerId.Value,
                "인앱 상품(" + productId + ")" + " 구입에 따른 아이템 지급",
                "영수증 번호: " + receiptId +
                "\n상품 번호: " + productId,
                true,
                true,
                Convert.ToDateTime(DateTime.MaxValue.ToString("yyyy-MM-dd HH:mm:ss")));

            var inAppItems = _memoryCacheService.GetInAppItemsByProductId(productId);
            var mailItems = inAppItems.ConvertAll(inAppItem =>
            {
                return new ItemDTO()
                {
                    ItemId = inAppItem.ItemId,
                    ItemCount = inAppItem.ItemCount
                };
            });

            errorCode = await SendRewardToMailbox(mail, mailItems);
            if (errorCode != ErrorCode.None)
            {
                // receipt 삽입 rollback
                await _queryFactory.Query("receipt").Where("receipt_id", receiptId).DeleteAsync();
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.SendInAppProduct] ErrorCode: {ErrorCode.SendRewardToMailboxError}, ReceiptId: {receiptId}," +
                    $"Message: An error occurred while send in app product, so has been rolled back.");
                return ErrorCode.SendRewardToMailboxError;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            // receipt가 이미 저장되었다면 rollback
            if (await _queryFactory.Query("receipt").Where("receipt_id", receiptId).CountAsync<Int32>() != 0)
            {
                await _queryFactory.Query("receipt").Where("receipt_id", receiptId).DeleteAsync();
            }

            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.SendInAppProduct] ErrorCode: {ErrorCode.SendInAppProductException}, ReceiptId: {receiptId}," +
                $"Message: An error occurred while send in app product, so has been rolled back.");
            return ErrorCode.SendInAppProductException;
        }
    }

    private async Task<ErrorCode> SendRewardToMailbox(SendMailDTO mail, List<ItemDTO> items)
    {
        var mailId = 0L;
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(new
            {
                player_id = playerId,
                title = mail.Title,
                content = mail.Content,
                is_received = mail.IsReceived,
                is_in_app_product = mail.IsInAppProduct,
                has_item = mail.HasItem,
                is_read = mail.IsRead,
                expiration_time = mail.ExpirationTime,
                is_deleted = mail.IsDeleted
            });

            foreach (ItemDTO item in items)
            {
                await _queryFactory.Query("mail_item").InsertAsync(new
                {
                    mail_id = mailId,
                    item_id = item.ItemId,
                    item_count = item.ItemCount
                });
            }

            _logger.ZLogDebug(EventIdDic[EventType.GameService],
                $"[GameService.SendRewardToMailbox] PlayerId: {playerId}, MailId: {mailId}, MailItemCount: {items.Count}");

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            // Mail은 저장됐는데 MailItem 저장 도중 Exception이 발생한 경우 Mail 롤백
            if (mailId != 0)
            {
                await _queryFactory.Query("mail").Where("mail_id", mailId).DeleteAsync();

                // 이미 저장된 MailItem이 있는데 도중에 Exception이 발생한 경우 MailItem 롤백
                var itemCount = await _queryFactory.Query("mail_item").Where("mail_id", mailId).CountAsync<Int32>();
                if (itemCount > 0)
                {
                    await _queryFactory.Query("mail_item").Where("mail_id", mailId).DeleteAsync();
                }
            }

            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.SendRewardToMailbox] ErrorCode: {ErrorCode.SendRewardToMailboxException}, MailTitle: {mail.Title}, MailItemCount: {items.Count}, " +
                $"Message: An error occurred while send reward to mailbox, so has been rolled back.");
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

            (errorCode, var playerItem) = await GetPlayerItemEnhanceDTO(playerId, playerItemId);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, bool>(errorCode, false);
            }

            var item = _memoryCacheService.GetItemByItemId(playerItem.ItemId);

            errorCode = CanEnhance(item.EnhanceMaxCount, playerItem.EnhanceCount);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, bool>(errorCode, false);
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

            // 강화 성공 확률 구하기
            bool isSuccess = IsEnhancementSuccessful(playerItem.EnhanceCount + 1);

            if (!isSuccess)
            {
                await _queryFactory.Query("player_item").Where("player_item_id", playerItemId).DeleteAsync();
                return new Tuple<ErrorCode, bool>(ErrorCode.None, false);
            }

            // 강화 적용
            if (item.AttributeId == 1) // 무기
            {
                playerItem.Attack = (Int64)Math.Ceiling((double)playerItem.Attack * 1.1);
            }
            else if (item.AttributeId == 2) // 방어구
            {
                playerItem.Defence = (Int64)Math.Ceiling((double)playerItem.Defence * 1.1);
            }
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
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.EnhanceItem] ErrorCode: {ErrorCode.EnhanceItemException}");
            return new Tuple<ErrorCode, bool>(ErrorCode.EnhanceItemException, false);
        }
    }

    private bool IsEnhancementSuccessful(int targetEnhanceCount)
    {
        var successPercent = Math.Truncate((double)1 / targetEnhanceCount * 100);
        if (targetEnhanceCount > 5)
        {
            successPercent = Math.Truncate(successPercent / 2);
        }
        if (targetEnhanceCount > 9)
        {
            successPercent = Math.Truncate(successPercent / 2);
        }

        double[] probs;
        if (successPercent <= 50)
        {
            probs = new double[] { successPercent, 100 - successPercent };
            if (Util.Game.Choose(probs) == 0)
            {
                return true;
            }
        }
        else
        {
            probs = new double[] { 100 - successPercent, successPercent };
            if (Util.Game.Choose(probs) == 1)
            {
                return true;
            }
        }
        return false;
    }

    private ErrorCode CanEnhance(Int64 enhanceMaxCount, Int16 enhanceCount)
    {
        // 강화 가능한 아이템인지 확인
        if (enhanceMaxCount == 0)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.EnhanceItem] ErrorCode: {ErrorCode.NotEnchantableItem}");
            return ErrorCode.NotEnchantableItem;
        }

        // 강화 횟수가 남아있는지 확인
        if (enhanceMaxCount <= enhanceCount)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.EnhanceItem] ErrorCode: {ErrorCode.OverMaxEnhanceCount}");
            return ErrorCode.OverMaxEnhanceCount;
        }
        return ErrorCode.None;
    }

    private async Task<Tuple<ErrorCode, PlayerItemEnhanceDTO>> GetPlayerItemEnhanceDTO(long? playerId, long playerItemId)
    {
        // 로그인한 계정이 소유한 아이템이 맞는지 확인
        var playerItem = await _queryFactory.Query("player_item")
            .Where(new
            {
                player_id = playerId,
                player_item_id = playerItemId,
            })
            .Select("player_item_id AS PlayerItemId",
            "item_id AS ItemId",
            "enhance_count AS EnhanceCount",
            "attack AS Attack",
            "defence AS Defence",
            "magic AS Magic")
            .FirstOrDefaultAsync<PlayerItemEnhanceDTO>();

        if (playerItem == null)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.EnhanceItem] ErrorCode: {ErrorCode.PlayerItemNotExist}, PlayerId: {playerId}, PlayerItemId: {playerItemId}");
            return new Tuple<ErrorCode, PlayerItemEnhanceDTO>(ErrorCode.PlayerItemNotExist, null);
        }
        return new Tuple<ErrorCode, PlayerItemEnhanceDTO>(ErrorCode.None, playerItem);
    }

    public async Task<ErrorCode> DeletePlayerAsync(Int64 playerId)
    {
        try
        {
            await _queryFactory.Query("account_player").Where("player_id", playerId).DeleteAsync();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetPlayerIdFromHttpContext] ErrorCode: {ErrorCode.PlayerIdNotExist}");
            return ErrorCode.DeletePlayerAsyncException;
        }

        return ErrorCode.None;
    }

    public async Task<Tuple<ErrorCode, IEnumerable<StageDetail>>> GetAllStagesAsync()
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<StageDetail>>(errorCode, null);
            }

            (errorCode, var highestClearedStage) = await GetHighestClearedStage(playerId);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<StageDetail>>(errorCode, null);
            }

            List<StageDetail> stages = new List<StageDetail>();
            for (int idx = 1; idx <= highestClearedStage; idx++)
            {
                stages.Add(new StageDetail(idx, true));
            }
            for (int idx = highestClearedStage + 1; idx <= _memoryCacheService.GetTotalStageCount(); idx++)
            {
                stages.Add(new StageDetail(idx, false));
            }

            return new Tuple<ErrorCode, IEnumerable<StageDetail>>(errorCode, stages);
        } catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetAllStagesAsync] ErrorCode: {ErrorCode.GetAllStagesAsyncException}");
            return new Tuple<ErrorCode, IEnumerable<StageDetail>>(ErrorCode.GetAllStagesAsyncException, null);
        }
    }

    public async Task<Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>> EnterStageAsync(Int32 requestedStageId)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(errorCode, null, null);
            }

            (errorCode, var highestClearedStage) = await GetHighestClearedStage(playerId);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(errorCode, null, null);
            }

            if (highestClearedStage + 1 < requestedStageId)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.EnterStageAsync] ErrorCode: {ErrorCode.InaccessibleStage}");
                return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(ErrorCode.InaccessibleStage, null, null);
            }

            (errorCode, var items) = _memoryCacheService.GetStageItemsByStageId(requestedStageId);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(errorCode, null, null);
            }

            (errorCode, var npcs) = _memoryCacheService.GetAttackNpcsByStageId(requestedStageId);
            if (errorCode != ErrorCode.None)
            {
                return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(errorCode, null, null);
            }

            return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(ErrorCode.None, items, npcs);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.EnterStageAsync] ErrorCode: {ErrorCode.EnterStageAsyncException}");
            return new Tuple<ErrorCode, IEnumerable<Int64>, IEnumerable<AttackNpcDTO>>(ErrorCode.EnterStageAsyncException, null, null);
        }
    }

    private async Task<Tuple<ErrorCode, Int32>> GetHighestClearedStage(long? playerId)
    {
        try
        {
            var highestClearedStage = await _queryFactory.Query("highest_cleared_stage")
                .Where("player_id", playerId)
                .Select("stage_id")
                .FirstOrDefaultAsync<Int32>();

            return new Tuple<ErrorCode, Int32>(ErrorCode.None, highestClearedStage);
        } catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.GetHighestClearedStage] ErrorCode: {ErrorCode.GetHighestClearedStageException}");
            return new Tuple<ErrorCode, Int32>(ErrorCode.GetHighestClearedStageException, 0);
        }
    }

    public async Task<ErrorCode> SaveStageRewardToPlayer(Int32 stageId, List<Int64> itemIds)
    {
        try
        {
            var (errorCode, playerId) = GetPlayerIdFromHttpContext();
            if (errorCode != ErrorCode.None)
            {
                return errorCode;
            }

            var stackableItems = new Dictionary<Int64, Int32>();
            var items = new List<ItemDTO>();

            foreach ( var itemId in itemIds ) {
                if (_memoryCacheService.IsStackableItem(itemId))
                {
                    if (stackableItems.ContainsKey(itemId))
                    {
                        stackableItems[itemId] = stackableItems[itemId] + 1;
                    } else
                    {
                        stackableItems.Add(itemId, 1);
                    }
                } else
                {
                    items.Add(new ItemDTO(itemId, 1));
                }
            }

            foreach (var item in stackableItems)
            {
                items.Add(new ItemDTO(item.Key, item.Value));
            }

            errorCode = await ReceiveItemsActions(playerId, items);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.SaveStageRewardToPlayer] ErrorCode: {errorCode}");
                return errorCode;
            }

            Int64 totalExp = 0;
            (errorCode, var attackNpcs) = _memoryCacheService.GetAttackNpcsByStageId(stageId);
            foreach (var attackNpc in attackNpcs)
            {
                (errorCode, var exp) = _memoryCacheService.GetAttackNpcExpByNpcId(attackNpc.NpcId);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError(EventIdDic[EventType.GameService],
                        $"[GameService.SaveStageRewardToPlayer] ErrorCode: {errorCode}");
                    return errorCode;
                }
                totalExp += exp * attackNpc.NpcCount;
            }

            errorCode = await AddPlayerExpAsync(playerId, totalExp);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.SaveStageRewardToPlayer] ErrorCode: {errorCode}");
                return errorCode;
            }

            errorCode = await UpdateHighestClearedStageAsync(playerId, stageId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError(EventIdDic[EventType.GameService],
                    $"[GameService.SaveStageRewardToPlayer] ErrorCode: {errorCode}");
                return errorCode;
            }

            return ErrorCode.None;
        } catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.SaveStageRewardToPlayer] ErrorCode: {ErrorCode.SaveStageRewardToPlayerException}");
            return ErrorCode.SaveStageRewardToPlayerException;
        }
    }

    private async Task<ErrorCode> UpdateHighestClearedStageAsync(Int64? playerId, Int32 stageId)
    {
        try
        {
            Int32? previousHighestClearedStage = await _queryFactory.Query("highest_cleared_stage").Where("player_id", playerId)
                .Select("stage_id AS StageId")
                .FirstOrDefaultAsync<Int32>();

            if (previousHighestClearedStage == null || previousHighestClearedStage == 0)
            {
                await _queryFactory.Query("highest_cleared_stage").InsertAsync(new
                {
                    player_id = playerId,
                    stage_id = stageId
                });
            }
            else if (previousHighestClearedStage < stageId)
            {
                await _queryFactory.Query("highest_cleared_stage").Where("player_id", playerId).UpdateAsync(new
                {
                    stage_id = stageId
                });
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.UpdateHighestClearedStageAsync] ErrorCode: {ErrorCode.UpdateHighestClearedStageAsyncException}");
            return ErrorCode.UpdateHighestClearedStageAsyncException;
        }
    }

    private async Task<ErrorCode> AddPlayerExpAsync(Int64? playerId, Int64 addedExp)
    {
        try
        {
            await _queryFactory.Query("player_game").Where("player_id", playerId).UpdateAsync(new
            {
                exp = addedExp
            });

            return ErrorCode.None;
        } catch (Exception ex)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService], ex,
                $"[GameService.AddPlayerExpAsync] ErrorCode: {ErrorCode.AddPlayerExpAsyncException}");
            return ErrorCode.AddPlayerExpAsyncException;
        }
    }

    private (ErrorCode, Int64?) GetPlayerIdFromHttpContext()
    {
        var playerId = (_httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser)?.PlayerId;

        if (playerId == null)
        {
            _logger.ZLogError(EventIdDic[EventType.GameService],
                $"[GameService.GetPlayerIdFromHttpContext] ErrorCode: {ErrorCode.PlayerIdNotExist}");
            return new(ErrorCode.PlayerIdNotExist, playerId);
        }

        return (ErrorCode.None, playerId);
    }
}